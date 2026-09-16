using BacapGenerator.Advancements.Models;
using BacapGenerator.Io;
using Core.Items;
using Core.Registries;
using Spectre.Console;
using UI.Actions.Common;
using UI.Actions.Common.Components;
using UI.Interfaces;
using UI.Styling;

namespace UI.Actions.Advancements.Functions.Menus;

/// <summary>
/// Provides an interactive console menu for viewing, adding, editing, and removing item rewards.
/// </summary>
public static class ItemRewardMenu
{
    /// <summary>
    /// Represents the available configuration actions for a specific item reward.
    /// </summary>
    private enum ItemEditOption
    {
        ChangeId,
        ChangeAmount,
        EditComponents,
        Delete,
        Back
    }

    /// <summary>
    /// Opens the item reward management loop for the given advancement.
    /// </summary>
    /// <param name="advancement">The target advancement to configure.</param>
    /// <example>
    /// <code>
    /// ItemRewardMenu.Open(bacapAdvancement);
    /// </code>
    /// </example>
    public static async Task Open(BacapAdvancement advancement)
    {
        var items = advancement.ItemRewardFunction.RewardItems.ToList();
        var mcData = advancement.Datapack.MinecraftData;

        while (true)
        {
            TuiTheme.RenderHeader($"Item Rewards: {advancement.TitleText}");

            var backAction = new BackAction();
            var addAction = new AddAction("[green]+ Add New Item[/]");

            var choices = items
                .Select(ITuiAction (item) => new ItemEntryAction(item, mcData))
                .ToList();

            choices.Add(addAction);
            choices.Add(backAction);

            var selected = await TuiTheme.PromptSelectionOrDefaultAsync(
                "Select an item to edit (press [bold]Q[/] to return):",
                choices,
                action => action.Title);

            if (selected is null || selected == backAction)
            {
                advancement.ItemRewardFunction.SetRewardItems(items);
                AdvancementIoManager.SaveRewardFunction(advancement, advancement.ItemRewardFunction);
                return;
            }

            if (selected == addAction)
            {
                var newItem = PromptCreateItem(mcData);
                items.Add(newItem);
                SaveIntermediate(advancement, items);
                TuiTheme.ShowSuccess($"Added {newItem.Count}x {newItem.Id}");
                continue;
            }

            if (selected is not ItemEntryAction itemAction)
                continue;

            var index = items.IndexOf(itemAction.Item);
            if (index >= 0)
                await EditSingleItem(items, index, mcData, advancement);
        }
    }

    /// <summary>
    /// Displays an editing sub-menu for an individual item stack with dynamic status previews.
    /// </summary>
    /// <param name="items">The active working list of item stacks.</param>
    /// <param name="index">The index of the item being edited.</param>
    /// <param name="mcData">The loaded Minecraft registry data.</param>
    /// <param name="advancement">The advancement being modified.</param>
    private static async Task EditSingleItem(
        List<ItemStack> items,
        int index,
        MinecraftData mcData,
        BacapAdvancement advancement)
    {
        while (true)
        {
            var current = items[index];
            TuiTheme.RenderHeader($"Edit: {ItemPromptUtils.FormatItemStack(current, mcData)}");

            var componentCount = current.Components.Added.Count;
            var componentsPreview = componentCount switch
            {
                0 => "None",
                _ => $"{componentCount} active"
            };

            var action = AnsiConsole.Prompt(
                new SelectionPrompt<ItemEditOption>()
                    .Title("Choose an action:")
                    .AddChoices(Enum.GetValues<ItemEditOption>())
                    .UseConverter(opt => opt switch
                    {
                        ItemEditOption.ChangeId => $"Change ID [grey]({current.Id})[/]",
                        ItemEditOption.ChangeAmount => $"Change Amount [grey]({current.Count}x)[/]",
                        ItemEditOption.EditComponents => $"Edit Components [grey]({componentsPreview})[/]",
                        ItemEditOption.Delete => "[red]Delete Item[/]",
                        ItemEditOption.Back => TuiTheme.BackOptionString,
                        _ => opt.ToString()
                    }));

            switch (action)
            {
                case ItemEditOption.ChangeId:
                    var newId = ItemPromptUtils.PromptItemId(mcData, current.Id);
                    items[index] = current with { Id = newId };
                    SaveIntermediate(advancement, items);
                    break;

                case ItemEditOption.ChangeAmount:
                    var newCount = ItemPromptUtils.PromptItemCount(current.Count);
                    items[index] = current with { Count = newCount };
                    SaveIntermediate(advancement, items);
                    break;

                case ItemEditOption.EditComponents:
                    items[index] = await ItemComponentsMenu.Open(current, mcData);
                    SaveIntermediate(advancement, items);
                    break;

                case ItemEditOption.Delete:
                    if (await AnsiConsole.ConfirmAsync($"Remove [yellow]{current.Id}[/] from rewards?", defaultValue: false))
                    {
                        items.RemoveAt(index);
                        SaveIntermediate(advancement, items);
                        TuiTheme.ShowSuccess("Item removed.");
                        TuiTheme.WaitForKey();
                        return;
                    }
                    break;

                case ItemEditOption.Back:
                    return;
            }
        }
    }

    /// <summary>
    /// Prompts the user to create a new <see cref="ItemStack"/> by asking for ID and count.
    /// </summary>
    /// <param name="mcData">The loaded Minecraft registry data.</param>
    /// <returns>A new <see cref="ItemStack"/> instance.</returns>
    private static ItemStack PromptCreateItem(MinecraftData mcData)
    {
        TuiTheme.RenderHeader("Add New Item Reward");

        var id = ItemPromptUtils.PromptItemId(mcData);
        var count = ItemPromptUtils.PromptItemCount();

        return new ItemStack(id, count);
    }

    /// <summary>
    /// Synchronizes the in-memory items with the function AST and writes changes to disk.
    /// </summary>
    /// <param name="advancement">The target advancement.</param>
    /// <param name="items">The current list of item rewards.</param>
    private static void SaveIntermediate(BacapAdvancement advancement, List<ItemStack> items)
    {
        advancement.ItemRewardFunction.SetRewardItems(items);
        AdvancementIoManager.SaveRewardFunction(advancement, advancement.ItemRewardFunction);
    }
}