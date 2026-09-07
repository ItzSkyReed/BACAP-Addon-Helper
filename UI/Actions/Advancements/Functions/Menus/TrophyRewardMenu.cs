using System.Text.RegularExpressions;
using BacapGenerator.Models.Advancements;
using BacapGenerator.Models.Advancements.Functions.Trophy;
using BacapGenerator.Services.IO;
using Core.DataComponents.Components;
using Core.DataComponents.Interfaces;
using Core.Registries;
using Core.TextComponents.Components;
using Core.TextComponents.Models;
using Spectre.Console;

using UI.Actions.Common;
using UI.Actions.Common.Components;
using UI.Actions.Common.Components.Wizards;
using UI.Interfaces;
using UI.Styling;

namespace UI.Actions.Advancements.Functions.Menus;

/// <summary>
/// Provides an interactive console menu for viewing, creating, configuring, and removing BACAP trophy rewards.
/// </summary>
public static partial class TrophyRewardMenu
{
    [GeneratedRegex("^#(?:[0-9a-fA-F]{6})$")]
    private static partial Regex HexColorRegex();

    /// <summary>
    /// Opens the trophy reward management loop for the target advancement.
    /// </summary>
    /// <param name="advancement">The target advancement to configure.</param>
    /// <example>
    /// <code>
    /// TrophyRewardMenu.Open(bacapAdvancement);
    /// </code>
    /// </example>
    public static async Task Open(BacapAdvancement advancement)
    {
        var trophies = advancement.TrophyRewardFunction.Trophies.ToList();
        var mcData = advancement.Datapack.MinecraftData;

        while (true)
        {
            TuiTheme.RenderHeader($"Trophy Rewards: {advancement.TitleText}");

            var backAction = new BackAction();
            var addAction = new AddAction("[green]+ Add New Trophy[/]");

            var choices = trophies
                .Select(ITuiAction (trophy) => new TrophyEntryAction(trophy))
                .ToList();

            choices.Add(addAction);
            choices.Add(backAction);

            var selected = await TuiTheme.PromptSelectionOrDefaultAsync(
                "Select a trophy to inspect/edit (press [bold]Q[/] to return):",
                choices,
                action => action.Title);

            if (selected is null || selected == backAction)
            {
                advancement.TrophyRewardFunction.SetTrophies(trophies);
                AdvancementIoManager.SaveAdvancement(advancement);
                return;
            }

            if (selected == addAction)
            {
                var newTrophy = PromptCreateTrophy(advancement, mcData);
                trophies.Add(newTrophy);
                SaveIntermediate(advancement, trophies);
                TuiTheme.ShowSuccess($"Added trophy '{Markup.Escape(newTrophy.Title ?? newTrophy.Item.Id)}'.");
                TuiTheme.WaitForKey();
                continue;
            }

            if (selected is not TrophyEntryAction trophyAction)
                continue;

            var index = trophies.IndexOf(trophyAction.Trophy);
            if (index >= 0)
            {
                await EditSingleTrophy(trophies, index, mcData, advancement);
            }
        }
    }

    /// <summary>
    /// Step-by-step wizard prompting all required trophy properties.
    /// </summary>
    /// <param name="advancement">The advancement to configure the trophy for.</param>
    /// <param name="mcData">The loaded Minecraft registry data.</param>
    /// <returns>A fully initialized <see cref="TrophyReward"/> instance.</returns>
    private static TrophyReward PromptCreateTrophy(BacapAdvancement advancement, MinecraftData mcData)
    {
        TuiTheme.RenderHeader("Add New Trophy Reward");

        var itemId = ItemPromptUtils.PromptItemId(mcData);
        var count = ItemPromptUtils.PromptItemCount();

        var defaultTitle = $"{advancement.TitleText} Trophy";
        var title = AnsiConsole.Prompt(
            new TextPrompt<string>("Enter trophy title (translation key or text):")
                .DefaultValue(defaultTitle)
        ).Trim();

        var color = PromptTrophyColor(mcData, defaultColor: "#B0CCD8");

        var rawLore = AnsiConsole.Prompt(
            new TextPrompt<string>("Enter trophy description / lore (supports long text, will auto-wrap):")
                .AllowEmpty()
        ).Trim();

        var deliveryType = AnsiConsole.Prompt(
            new SelectionPrompt<TrophyDeliveryType>()
                .Title("Select delivery mechanism:")
                .AddChoices(TrophyDeliveryType.Inventory, TrophyDeliveryType.DeathLocation)
                .UseConverter(d => d switch
                {
                    TrophyDeliveryType.Inventory => "Inventory (/give @s)",
                    TrophyDeliveryType.DeathLocation => "Death Location (/summon item ~ ~ ~)",
                    _ => d.ToString()
                })
        );

        var extraComponents = new List<IDataComponent>();
        if (!string.IsNullOrWhiteSpace(rawLore))
        {
            var wrappedLines = LoreWizard.WrapText(rawLore);
            var loreTextComponents = wrappedLines
                .Select(line => (TextComponent)new TranslatableComponent(
                    Translate: line,
                    Style: new TextStyle(Color: color)
                ))
                .ToList();

            extraComponents.Add(new LoreComponent(loreTextComponents));
        }

        return TrophyReward.CreateNew(
            datapack: advancement.Datapack,
            itemId: itemId,
            titleKey: title,
            titleColor: color,
            count: count,
            deliveryType: deliveryType,
            extraSetComponents: extraComponents
        );
    }

    /// <summary>
    /// Represents the available configuration actions for a specific trophy reward.
    /// </summary>
    private enum TrophyEditOption
    {
        ChangeBaseId,
        ChangeAmount,
        ChangeTitle,
        ChangeColor,
        ChangeLore,
        ToggleDelivery,
        EditComponents,
        Delete,
        Back
    }

    /// <summary>
    /// Displays a sub-menu for an individual trophy reward with dynamic status previews.
    /// </summary>
    /// <param name="trophies">The active list of trophy rewards.</param>
    /// <param name="index">The index of the trophy being edited.</param>
    /// <param name="mcData">The loaded Minecraft registry data.</param>
    /// <param name="advancement">The advancement being modified.</param>
    private static async Task EditSingleTrophy(
        List<TrophyReward> trophies,
        int index,
        MinecraftData mcData,
        BacapAdvancement advancement)
    {
        while (true)
        {
            var current = trophies[index];
            TuiTheme.RenderHeader($"Edit Trophy: {FormatTrophy(current)}");

            var color = current.TitleColor ?? "#B0CCD8";
            var loreCount = current.DescriptionLines.Count;
            var lorePreview = loreCount switch
            {
                0 => "None",
                1 => $"\"{Markup.Escape(Truncate(current.DescriptionLines[0], 24))}\"",
                _ => $"{loreCount} lines"
            };

            var deliveryPreview = current.DeliveryType switch
            {
                TrophyDeliveryType.Inventory => "Inventory (/give)",
                TrophyDeliveryType.DeathLocation => "Death Location (/summon)",
                _ => current.DeliveryType.ToString()
            };

            var componentCount = current.Item.Components.Added.Count;

            var action = AnsiConsole.Prompt(
                new SelectionPrompt<TrophyEditOption>()
                    .Title("Choose an action:")
                    .AddChoices(Enum.GetValues<TrophyEditOption>())
                    .UseConverter(opt => opt switch
                    {
                        TrophyEditOption.ChangeBaseId => $"Change Base Item ID [grey]({current.Item.Id})[/]",
                        TrophyEditOption.ChangeAmount => $"Change Amount [grey]({current.Item.Count}x)[/]",
                        TrophyEditOption.ChangeTitle => $"Change Title [grey]({Markup.Escape(current.Title ?? "None")})[/]",
                        TrophyEditOption.ChangeColor => $"Change Color [{color}]■[/] [grey]({color})[/]",
                        TrophyEditOption.ChangeLore => $"Change Description (Lore) [grey]({lorePreview})[/]",
                        TrophyEditOption.ToggleDelivery => $"Toggle Delivery Type [grey]({deliveryPreview})[/]",
                        TrophyEditOption.EditComponents => $"Edit Extra Components [grey]({componentCount} active)[/]",
                        TrophyEditOption.Delete => "[red]Delete Trophy[/]",
                        TrophyEditOption.Back => TuiTheme.BackOptionString,
                        _ => opt.ToString()
                    }));

            switch (action)
            {
                case TrophyEditOption.ChangeBaseId:
                    var newId = ItemPromptUtils.PromptItemId(mcData, current.Item.Id);
                    trophies[index] = current with { Item = current.Item with { Id = newId } };
                    SaveIntermediate(advancement, trophies);
                    break;

                case TrophyEditOption.ChangeAmount:
                    var newCount = ItemPromptUtils.PromptItemCount(current.Item.Count);
                    trophies[index] = current with { Item = current.Item with { Count = newCount } };
                    SaveIntermediate(advancement, trophies);
                    break;

                case TrophyEditOption.ChangeTitle:
                    var newTitle = AnsiConsole.Prompt(
                        new TextPrompt<string>("Enter new title:")
                            .DefaultValue(current.Title ?? string.Empty)
                    );
                    var style = new TextStyle(Color: current.TitleColor ?? "#B0CCD8", Bold: true, Italic: false);
                    current.Item.Components.Set(new CustomNameComponent(new TranslatableComponent(newTitle, Style: style)));
                    current.Standardize(advancement.Datapack);
                    SaveIntermediate(advancement, trophies);
                    break;

                case TrophyEditOption.ChangeColor:
                    var newColor = PromptTrophyColor(mcData, current.TitleColor ?? "#B0CCD8");
                    UpdateTrophyColor(current, newColor);
                    SaveIntermediate(advancement, trophies);
                    break;

                case TrophyEditOption.ChangeLore:
                    UpdateTrophyLore(current);
                    SaveIntermediate(advancement, trophies);
                    break;

                case TrophyEditOption.ToggleDelivery:
                    var nextType = current.DeliveryType == TrophyDeliveryType.Inventory
                        ? TrophyDeliveryType.DeathLocation
                        : TrophyDeliveryType.Inventory;
                    trophies[index] = current with { DeliveryType = nextType };
                    SaveIntermediate(advancement, trophies);
                    TuiTheme.ShowSuccess($"Delivery set to {nextType}.");
                    TuiTheme.WaitForKey();
                    break;

                case TrophyEditOption.EditComponents:
                    var updatedItem = await ItemComponentsMenu.Open(current.Item, mcData);
                    trophies[index] = current with { Item = updatedItem };
                    SaveIntermediate(advancement, trophies);
                    break;

                case TrophyEditOption.Delete:
                    if (AnsiConsole.Confirm($"Remove trophy [yellow]{Markup.Escape(current.Title ?? current.Item.Id)}[/]?", defaultValue: false))
                    {
                        trophies.RemoveAt(index);
                        SaveIntermediate(advancement, trophies);
                        TuiTheme.ShowSuccess("Trophy removed.");
                        TuiTheme.WaitForKey();
                        return;
                    }
                    break;

                case TrophyEditOption.Back:
                    return;
            }
        }
    }

    /// <summary>
    /// Truncates a string to a maximum length, appending an ellipsis if truncated.
    /// </summary>
    private static string Truncate(string value, int maxLength)
    {
        return value.Length <= maxLength ? value : $"{value[..maxLength]}...";
    }

    /// <summary>
    /// Updates the custom name and all lore line colors across the trophy stack.
    /// </summary>
    /// <param name="trophy">The trophy reward to update.</param>
    /// <param name="newColor">The new hex color code.</param>
    private static void UpdateTrophyColor(TrophyReward trophy, string newColor)
    {
        var title = trophy.Title ?? "Trophy";
        var nameStyle = new TextStyle(Color: newColor, Bold: true, Italic: false);
        trophy.Item.Components.Set(new CustomNameComponent(new TranslatableComponent(title, Style: nameStyle)));

        if (!trophy.Item.Components.TryGet<LoreComponent>(out var existingLore))
        {
            return;
        }

        var updatedLines = existingLore.Lines
            .Select(line => line switch
            {
                TranslatableComponent tc => tc with { Style = (tc.Style ?? new TextStyle()) with { Color = newColor } },
                PlainTextComponent ptc => ptc with { Style = (ptc.Style ?? new TextStyle()) with { Color = newColor } },
                _ => line
            })
            .ToList();

        trophy.Item.Components.Set(new LoreComponent(updatedLines));
    }

    /// <summary>
    /// Prompts a new description and wraps it into translatable lore lines matching the trophy's title color.
    /// </summary>
    /// <param name="trophy">The trophy reward to update.</param>
    private static void UpdateTrophyLore(TrophyReward trophy)
    {
        var existingText = string.Join(" ", trophy.DescriptionLines);
        var input = AnsiConsole.Prompt(
            new TextPrompt<string>("Enter new lore description (or leave empty to clear):")
                .DefaultValue(existingText)
                .AllowEmpty()
        ).Trim();

        if (string.IsNullOrWhiteSpace(input))
        {
            trophy.Item.Components.Remove<LoreComponent>();
            TuiTheme.ShowSuccess("Lore cleared.");
            TuiTheme.WaitForKey();
            return;
        }

        var color = trophy.TitleColor ?? "#B0CCD8";
        var wrappedLines = LoreWizard.WrapText(input, maxWidth: 45);
        var loreComponents = wrappedLines
            .Select(line => (TextComponent)new TranslatableComponent(
                Translate: line,
                Style: new TextStyle(Color: color)
            ))
            .ToList();

        trophy.Item.Components.Set(new LoreComponent(loreComponents));
        TuiTheme.ShowSuccess($"Updated lore with {wrappedLines.Count} line(s).");
        TuiTheme.WaitForKey();
    }

    /// <summary>
    /// Prompts the user to select a color from the Minecraft registry or enter a custom hex string.
    /// </summary>
    /// <param name="mcData">The loaded Minecraft registry data containing known text colors.</param>
    /// <param name="defaultColor">The fallback hex color code used when entering a custom value (defaults to <c>"#B0CCD8"</c>).</param>
    /// <returns>A validated hex color representation (e.g. <c>"#FFAA00"</c>).</returns>
    private static string PromptTrophyColor(MinecraftData mcData, string defaultColor)
    {
        const string customHexChoice = "Enter Custom HEX (#RRGGBB)";

        List<string> choices = [customHexChoice, ..mcData.TextColors.Keys];

        var selection = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Select title & lore color:")
                .EnableSearch()
                .PageSize(15)
                .AddChoices(choices)
                .UseConverter(key =>
                {
                    if (key == customHexChoice)
                        return $"[yellow]■[/] [yellow]{key}[/]";

                    var hex = mcData.TextColors[key];
                    return $"[{hex}]■[/] [white]{Markup.Escape(key)}[/] [grey]({hex})[/]";
                })
        );

        if (selection != customHexChoice)
            return mcData.TextColors[selection];

        return AnsiConsole.Prompt(
            new TextPrompt<string>("Enter HEX color code (e.g. #FFAA00):")
                .DefaultValue(defaultColor)
                .Validate(hex => HexColorRegex().IsMatch(hex.Trim())
                    ? ValidationResult.Success()
                    : ValidationResult.Error("[red]Invalid HEX format. Expected format: #RRGGBB[/]"))
        ).Trim();
    }

    /// <summary>
    /// Formats a trophy reward for display in TUI lists and headers.
    /// </summary>
    /// <param name="trophy">The trophy reward to format.</param>
    /// <returns>A markup string describing the trophy.</returns>
    private static string FormatTrophy(TrophyReward trophy)
    {
        var title = trophy.Title ?? "Unnamed Trophy";
        var color = trophy.TitleColor ?? "gold";
        var escapedTitle = Markup.Escape(title);

        var deliveryTag = trophy.DeliveryType == TrophyDeliveryType.DeathLocation
            ? " [red][[DeathLoc]][/]"
            : string.Empty;

        return $"[{color}]{escapedTitle}[/] [grey]({trophy.Item.Id})[/]{deliveryTag}";
    }

    /// <summary>
    /// Synchronizes the in-memory trophies with the function AST and writes changes to disk.
    /// </summary>
    /// <param name="advancement">The target advancement.</param>
    /// <param name="trophies">The current list of trophy rewards.</param>
    private static void SaveIntermediate(BacapAdvancement advancement, List<TrophyReward> trophies)
    {
        advancement.TrophyRewardFunction.SetTrophies(trophies);
        AdvancementIoManager.SaveAdvancement(advancement);
    }

    /// <summary>
    /// Menu action wrapper representing an existing trophy reward.
    /// </summary>
    /// <param name="trophy">The trophy reward instance.</param>
    private sealed class TrophyEntryAction(TrophyReward trophy) : ITuiAction
    {
        public TrophyReward Trophy { get; } = trophy;

        public string Title => FormatTrophy(Trophy);

        public Task ExecuteAsync() => Task.CompletedTask;
    }
}