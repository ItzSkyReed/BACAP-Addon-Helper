using BacapGenerator.Models.Advancements;
using BacapGenerator.Services.IO;
using BacapGenerator.Utils;
using Core.Items;
using Core.Registries;
using Spectre.Console;
using UI.Actions.Common;
using UI.Actions.Common.Components;
using UI.Styling;

namespace UI.Actions.Advancements.Functions;

/// <summary>
/// Provides an interactive console menu for viewing, adding, editing, and removing item rewards.
/// </summary>
public static class ItemRewardMenu
{
    private sealed record AddItemAction;

    /// <summary>
    /// Opens the item reward management loop for the given advancement.
    /// </summary>
    /// <param name="advancement">The target advancement to configure.</param>
    public static void Open(BacapAdvancement advancement)
    {
        var items = advancement.ItemRewardFunction.RewardItems.ToList();
        var mcData = advancement.Datapack.MinecraftData;

        while (true)
        {
            TuiTheme.RenderHeader($"Item Rewards: {advancement.TitleText}");

            var choices = new List<object>(items)
            {
                new AddItemAction(),
                new BackAction()
            };

            var selected = TuiTheme.PromptSelection(
                "Select an item to edit or choose an action:",
                choices,
                item => item switch
                {
                    ItemStack stack => FormatItemStack(stack, mcData),
                    AddItemAction => "[green]+ Add New Item[/]",
                    BackAction => TuiTheme.BackOptionString,
                    _ => item.ToString()!
                });

            switch (selected)
            {
                case BackAction:
                    // Persist final state to disk
                    advancement.ItemRewardFunction.SetRewardItems(items);
                    AdvancementIoManager.SaveAdvancement(advancement);
                    return;

                case AddItemAction:
                    var newItem = PromptCreateItem(mcData);
                    items.Add(newItem);
                    advancement.ItemRewardFunction.SetRewardItems(items);
                    AdvancementIoManager.SaveAdvancement(advancement);
                    TuiTheme.ShowSuccess($"Added {newItem.Count}x {newItem.Id}");

                    break;

                case ItemStack selectedStack:
                    var index = items.IndexOf(selectedStack);
                    if (index >= 0)
                    {
                        EditSingleItem(items, index, mcData, advancement);
                    }

                    break;
            }
        }
    }

    /// <summary>
    /// Displays an editing sub-menu for an individual item stack.
    /// </summary>
    /// <param name="items">The active working list of item stacks.</param>
    /// <param name="index">The index of the item being edited.</param>
    /// <param name="mcData">The loaded Minecraft registry data.</param>
    /// <param name="advancement">The advancement being modified.</param>
    private static void EditSingleItem(
        List<ItemStack> items,
        int index,
        MinecraftData mcData,
        BacapAdvancement advancement)
    {
        while (true)
        {
            var current = items[index];
            TuiTheme.RenderHeader($"Edit: {FormatItemStack(current, mcData)}");

            var action = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Choose an action:")
                    .AddChoices(
                        "Change ID",
                        "Change Amount",
                        "Edit Components",
                        "[red]Delete Item[/]",
                        "Back"
                    ));

            switch (action)
            {
                case "Change ID":
                    var newId = PromptItemId(mcData, current.Id);
                    items[index] = current with { Id = newId };
                    SaveIntermediate(advancement, items);
                    break;

                case "Change Amount":
                    var newCount = PromptItemCount(current.Count);
                    items[index] = current with { Count = newCount };
                    SaveIntermediate(advancement, items);
                    break;

                case "Edit Components":
                    items[index] = ItemComponentsMenu.Open(current, mcData);
                    SaveIntermediate(advancement, items);
                    break;

                case "[red]Delete Item[/]":
                    if (AnsiConsole.Confirm($"Remove [yellow]{current.Id}[/] from rewards?", defaultValue: false))
                    {
                        items.RemoveAt(index);
                        SaveIntermediate(advancement, items);
                        TuiTheme.ShowSuccess("Item removed.");
                        TuiTheme.WaitForKey();
                        return;
                    }

                    break;

                case "Back":
                    return;
            }
        }
    }

    /// <summary>
    /// Prompts the user to create a new <see cref="ItemStack"/> by asking for ID and count.
    /// </summary>
    /// <param name="mcData">The loaded Minecraft registry data.</param>
    /// <returns>A new <see cref="ItemStack"/> instance, or <see langword="null"/> if cancelled.</returns>
    private static ItemStack PromptCreateItem(MinecraftData mcData)
    {
        TuiTheme.RenderHeader("Add New Item Reward");

        var id = PromptItemId(mcData);
        var count = PromptItemCount(1);

        return new ItemStack(id, count);
    }

    /// <summary>
    /// Prompts the user for a valid Minecraft item identifier, ensuring existence in the registry
    /// and providing smart suggestions when typos or partial matches are detected.
    /// </summary>
    /// <param name="mcData">The loaded Minecraft registry data.</param>
    /// <param name="defaultId">An optional default item ID to suggest.</param>
    /// <returns>A standardized Minecraft item identifier with namespace (e.g., "minecraft:diamond").</returns>
    private static string PromptItemId(MinecraftData mcData, string? defaultId = null)
    {
        var prompt = new TextPrompt<string>("Enter Minecraft Item ID (e.g. 'diamond' or 'minecraft:iron_ingot'):")
            .ValidationErrorMessage("[red]Invalid item ID format.[/]")
            .Validate(input =>
            {
                var stripped = MinecraftUtils.StripNamespace(input.Trim().ToLowerInvariant());

                if (mcData.Items.ContainsKey(stripped))
                    return ValidationResult.Success();

                var suggestions = FindSimilarItems(stripped, mcData.Items.Keys, maxResults: 3);

                if (suggestions.Count > 0)
                {
                    var formattedSuggestions = string.Join(", ", suggestions.Select(s => $"[yellow]{Markup.Escape(s)}[/]"));
                    return ValidationResult.Error(
                        $"[red]Item '{Markup.Escape(stripped)}' was not found.[/] Did you mean: {formattedSuggestions}?");
                }

                return ValidationResult.Error($"[red]Item '{Markup.Escape(stripped)}' was not found in the Minecraft registry.[/]");
            });

        if (!string.IsNullOrWhiteSpace(defaultId))
            prompt.DefaultValue(defaultId);

        var result = AnsiConsole.Prompt(prompt).Trim().ToLowerInvariant();
        var strippedResult = MinecraftUtils.StripNamespace(result);

        return $"minecraft:{strippedResult}";
    }

    /// <summary>
    /// Finds the closest matching item keys based on substring containment and Levenshtein edit distance.
    /// </summary>
    /// <param name="query">The user-provided query string.</param>
    /// <param name="keys">The collection of valid item identifiers from the registry.</param>
    /// <param name="maxResults">The maximum number of suggestions to return.</param>
    /// <returns>A list of closest matching item identifiers.</returns>
    /// <example>
    /// <code>
    /// var suggestions = FindSimilarItems("dimond", mcData.Items.Keys, maxResults: 3);
    /// // Returns: ["diamond", "diamond_sword", "diamond_ore"]
    /// </code>
    /// </example>
    private static List<string> FindSimilarItems(string query, IEnumerable<string> keys, int maxResults = 3)
    {
        if (string.IsNullOrWhiteSpace(query))
            return [];

        // 1. First priority: item key contains query as a substring (e.g. "netherite" -> "netherite_ingot")
        var enumerable = keys as string[] ?? keys.ToArray();
        var substringMatches = enumerable
            .Where(k => k.Contains(query, StringComparison.OrdinalIgnoreCase))
            .OrderBy(k => k.Length)
            .Take(maxResults)
            .ToList();

        if (substringMatches.Count > 0)
            return substringMatches;

        // 2. Second priority: Levenshtein distance for typos (e.g. "dimond" -> "diamond")
        var maxDistance = Math.Max(2, query.Length / 2);

        return enumerable
            .Select(k => (Key: k, Distance: ComputeLevenshteinDistance(query, k)))
            .Where(x => x.Distance <= maxDistance)
            .OrderBy(x => x.Distance)
            .ThenBy(x => x.Key.Length)
            .Take(maxResults)
            .Select(x => x.Key)
            .ToList();
    }

    /// <summary>
    /// Computes the Levenshtein edit distance between two strings using stack-allocated memory.
    /// </summary>
    /// <param name="source">The source string to compare.</param>
    /// <param name="target">The target string to compare against.</param>
    /// <returns>The minimum number of single-character edits required to convert source into target.</returns>
    private static int ComputeLevenshteinDistance(ReadOnlySpan<char> source, ReadOnlySpan<char> target)
    {
        if (source.IsEmpty) return target.Length;
        if (target.IsEmpty) return source.Length;

        // Allocate distance rows on stack for maximum performance without heap allocations
        Span<int> previousRow = stackalloc int[target.Length + 1];
        Span<int> currentRow = stackalloc int[target.Length + 1];

        for (var j = 0; j <= target.Length; j++)
            previousRow[j] = j;

        for (var i = 0; i < source.Length; i++)
        {
            currentRow[0] = i + 1;

            for (var j = 0; j < target.Length; j++)
            {
                var cost = source[i] == target[j] ? 0 : 1;
                currentRow[j + 1] = Math.Min(
                    Math.Min(currentRow[j] + 1, previousRow[j + 1] + 1),
                    previousRow[j] + cost
                );
            }

            currentRow.CopyTo(previousRow);
        }

        return currentRow[target.Length];
    }

    /// <summary>
    /// Prompts the user for a positive item count.
    /// </summary>
    /// <param name="currentCount">The current or default count.</param>
    /// <returns>A validated integer greater than zero.</returns>
    private static int PromptItemCount(int currentCount)
    {
        return AnsiConsole.Prompt(
            new TextPrompt<int>("Enter item count:")
                .DefaultValue(currentCount)
                .ValidationErrorMessage("[red]Please enter a valid positive number.[/]")
                .Validate(val => val switch
                {
                    <= 0 => ValidationResult.Error("[red]Item count must be greater than 0.[/]"),
                    _ => ValidationResult.Success()
                })
        );
    }

    /// <summary>
    /// Formats an <see cref="ItemStack"/> for display inside Spectre.Console lists and headers.
    /// </summary>
    private static string FormatItemStack(ItemStack item, MinecraftData mcData)
    {
        var strippedId = MinecraftUtils.StripNamespace(item.Id);
        var displayName = mcData.Items.TryGetValue(strippedId, out var entry)
            ? entry.DisplayName
            : strippedId;

        var componentMarker = !item.Components.IsEmpty ? " [cyan](+Components)[/]" : string.Empty;
        return $"[yellow]{item.Count}x[/] [white]{Markup.Escape(displayName)}[/] [grey]({item.Id})[/]{componentMarker}";
    }

    /// <summary>
    /// Helper to synchronize and save after intermediate changes.
    /// </summary>
    private static void SaveIntermediate(BacapAdvancement advancement, List<ItemStack> items)
    {
        advancement.ItemRewardFunction.SetRewardItems(items);
        AdvancementIoManager.SaveAdvancement(advancement);
    }
}