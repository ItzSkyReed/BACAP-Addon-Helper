using BacapGenerator.Utils;
using Core.Items;
using Core.Registries;
using Spectre.Console;

namespace UI.Actions.Common;

/// <summary>
/// Provides reusable CLI prompts and validation helpers for Minecraft item identifiers, counts, and stacks.
/// </summary>
public static class ItemPromptUtils
{
    /// <summary>
    /// Prompts the user for a valid Minecraft item identifier, ensuring existence in the registry
    /// and providing smart suggestions when typos or partial matches are detected.
    /// </summary>
    /// <param name="mcData">The loaded Minecraft registry data.</param>
    /// <param name="defaultId">An optional default item ID to suggest.</param>
    /// <returns>A standardized Minecraft item identifier with namespace (e.g., <c>"minecraft:diamond"</c>).</returns>
    /// <example>
    /// <code>
    /// string itemId = ItemPromptUtils.PromptItemId(mcData, "minecraft:iron_ingot");
    /// </code>
    /// </example>
    public static string PromptItemId(MinecraftData mcData, string? defaultId = null)
    {
        var prompt = new TextPrompt<string>("Enter Minecraft Item ID (e.g. 'diamond' or 'minecraft:cobblestone_stairs'):")
            .ValidationErrorMessage("[red]Invalid item ID format.[/]")
            .Validate(input =>
            {
                var stripped = MinecraftUtils.StripNamespace(input.Trim().ToLowerInvariant());

                if (mcData.Items.ContainsKey(stripped))
                    return ValidationResult.Success();

                var suggestions = FindSimilarItems(stripped, mcData.Items.Keys, maxResults: 3);
                if (suggestions.Count <= 0)
                    return ValidationResult.Error($"[red]Item '{Markup.Escape(stripped)}' was not found in the Minecraft registry.[/]");

                var formattedSuggestions = string.Join(", ", suggestions.Select(s => $"[yellow]{Markup.Escape(s)}[/]"));
                return ValidationResult.Error($"[red]Item '{Markup.Escape(stripped)}' was not found.[/] Did you mean: {formattedSuggestions}?");
            });

        if (!string.IsNullOrWhiteSpace(defaultId))
        {
            prompt.DefaultValue(defaultId);
        }

        var result = AnsiConsole.Prompt(prompt).Trim().ToLowerInvariant();
        return $"minecraft:{MinecraftUtils.StripNamespace(result)}";
    }

    /// <summary>
    /// Prompts the user for a strictly positive item count.
    /// </summary>
    /// <param name="currentCount">The current or default count.</param>
    /// <returns>A validated integer greater than zero.</returns>
    /// <example>
    /// <code>
    /// int count = ItemPromptUtils.PromptItemCount(currentCount: 1);
    /// </code>
    /// </example>
    public static int PromptItemCount(int currentCount = 1)
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
    /// Formats an <see cref="ItemStack"/> for display inside Spectre.Console selection lists and headers.
    /// </summary>
    /// <param name="item">The item stack to format.</param>
    /// <param name="mcData">The loaded Minecraft registry data.</param>
    /// <returns>A formatted markup string representing the item stack.</returns>
    public static string FormatItemStack(ItemStack item, MinecraftData mcData)
    {
        var strippedId = MinecraftUtils.StripNamespace(item.Id);
        var displayName = mcData.Items.TryGetValue(strippedId, out var entry)
            ? entry.DisplayName
            : strippedId;

        var componentMarker = !item.Components.IsEmpty ? " [cyan](+Components)[/]" : string.Empty;
        return $"[yellow]{item.Count}x[/] [white]{Markup.Escape(displayName)}[/] [grey]({item.Id})[/]{componentMarker}";
    }

    /// <summary>
    /// Finds the closest matching item keys based on substring containment and Levenshtein edit distance.
    /// </summary>
    /// <param name="query">The user-provided query string.</param>
    /// <param name="keys">The collection of valid item identifiers from the registry.</param>
    /// <param name="maxResults">The maximum number of suggestions to return.</param>
    /// <returns>A list of closest matching item identifiers.</returns>
    public static List<string> FindSimilarItems(string query, IEnumerable<string> keys, int maxResults = 3)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return [];
        }

        var enumerable = keys as string[] ?? keys.ToArray();
        var substringMatches = enumerable
            .Where(k => k.Contains(query, StringComparison.OrdinalIgnoreCase))
            .OrderBy(k => k.Length)
            .Take(maxResults)
            .ToList();

        if (substringMatches.Count > 0)
        {
            return substringMatches;
        }

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
    /// Computes the Levenshtein edit distance between two character spans using stack-allocated memory.
    /// </summary>
    /// <param name="source">The source string to compare.</param>
    /// <param name="target">The target string to compare against.</param>
    /// <returns>The minimum number of single-character edits required to convert source into target.</returns>
    public static int ComputeLevenshteinDistance(ReadOnlySpan<char> source, ReadOnlySpan<char> target)
    {
        if (source.IsEmpty) return target.Length;
        if (target.IsEmpty) return source.Length;

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
}