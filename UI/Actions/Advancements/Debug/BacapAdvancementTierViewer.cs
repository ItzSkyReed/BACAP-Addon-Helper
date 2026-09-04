using BacapGenerator.Models.Advancements;
using BacapGenerator.Models.Datapacks;
using Spectre.Console;
using UI.Styling;

namespace UI.Actions.Advancements.Debug;

/// <summary>
/// Renders structured trees of BACAP advancements grouped by datapack, tier, and tab.
/// </summary>
public static class BacapAdvancementTierViewer
{
    /// <summary>
    /// Renders a tree view of BACAP advancements filtered by an optional tier.
    /// </summary>
    /// <param name="registry">The datapack registry containing all loaded advancements.</param>
    /// <param name="selectedTier">The specific tier to filter by, or <see langword="null"/> to render all tiers.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="registry"/> is null.</exception>
    /// <example>
    /// <code>
    /// BacapAdvancementTierViewer.RenderTree(registry, BacapAdvancementTier.Challenge);
    /// </code>
    /// </example>
    public static void RenderTree(DatapackRegistry registry, BacapAdvancementTier? selectedTier)
    {
        ArgumentNullException.ThrowIfNull(registry);

        var filterTitle = selectedTier.HasValue
            ? $"Tier: {selectedTier.Value}"
            : "All Tiers";

        TuiTheme.RenderHeader($"BACAP Advancements - {filterTitle}");

        var foundAny = false;

        // Iterate directly over registry key-value pairs (DatapackId, Datapack)
        foreach (var (id, datapack) in registry)
        {
            var advancements = datapack.Advancements
                .OfType<BacapAdvancement>()
                .Where(adv => !selectedTier.HasValue || adv.Tier == selectedTier.Value)
                .ToList();

            if (advancements.Count == 0)
                continue;

            foundAny = true;

            var rootTree = TuiTheme.CreateTree($"[bold cyan]{id}[/] [grey]({advancements.Count} total)[/]");

            var tierGroups = advancements
                .GroupBy(adv => adv.Tier)
                .OrderBy(g => g.Key);

            foreach (var tierGroup in tierGroups)
            {
                var tierNode = rootTree.AddNode($"[bold yellow]{tierGroup.Key}[/] [grey]({tierGroup.Count()})[/]");

                var tabGroups = tierGroup
                    .GroupBy(adv => adv.Tab)
                    .OrderBy(g => g.Key.DisplayName);

                foreach (var tabGroup in tabGroups)
                {
                    var tabNode = tierNode.AddNode($"[blue]{tabGroup.Key.DisplayName}[/] [grey]({tabGroup.Count()})[/]");

                    foreach (var adv in tabGroup.OrderBy(a => a.File.Name))
                    {
                        var escapedTitle = Markup.Escape(adv.TitleText);
                        tabNode.AddNode($"[grey]{adv.File.Name}[/] - [white]{escapedTitle}[/]");
                    }
                }
            }

            TuiTheme.RenderElement(rootTree);
            TuiTheme.Space();
        }

        if (!foundAny)
            TuiTheme.ShowInfo($"No advancements found for tier: {selectedTier}");
    }
}