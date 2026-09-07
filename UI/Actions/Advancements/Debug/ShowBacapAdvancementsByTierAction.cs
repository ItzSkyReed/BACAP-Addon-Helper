using BacapGenerator.Models.Advancements;
using BacapGenerator.Models.Datapacks;
using Spectre.Console;
using UI.Interfaces;
using UI.Styling;

namespace UI.Actions.Advancements.Debug;

/// <summary>
/// Action that provides an interactive menu to view valid BACAP advancements filtered by their tier.
/// </summary>
public class ShowBacapAdvancementsByTierAction(DatapackRegistry registry) : IDebugAdvancementsAction
{
    private const string AllTiersOption = "[bold cyan]All Tiers[/]";

    /// <inheritdoc/>
    public string Title => "View advancements by Tier";

    /// <summary>
    /// Executes the action, rendering a summary overview and prompting the user for tier selection.
    /// </summary>
    /// <returns>A completed task representing the asynchronous operation.</returns>
    public async Task ExecuteAsync()
    {
        while (true)
        {
            TuiTheme.RenderHeader("Advancements by Tier");

            var allBacapAdvancements = registry.Values
                .SelectMany(dp => dp.Advancements.OfType<BacapAdvancement>())
                .ToList();

            if (allBacapAdvancements.Count == 0)
            {
                TuiTheme.ShowWarning("No valid BACAP advancements found across all loaded datapacks.");
                TuiTheme.WaitForKey();
                return;
            }

            var tierCounts = allBacapAdvancements
                .GroupBy(adv => adv.Tier)
                .ToDictionary(g => g.Key, g => g.Count());

            var summaryTable = TuiTheme.CreateTable("Tier", "Total Count");

            foreach (var tier in Enum.GetValues<BacapAdvancementTier>())
            {
                var count = tierCounts.GetValueOrDefault(tier, 0);
                summaryTable.AddRow(
                    $"[yellow]{tier}[/]",
                    count > 0 ? $"[green]{count}[/]" : "[grey]0[/]"
                );
            }

            TuiTheme.RenderElement(summaryTable);
            TuiTheme.Space();

            // Build choices for the standardized prompt
            var choices = new List<string> { AllTiersOption };
            foreach (var tier in Enum.GetValues<BacapAdvancementTier>())
            {
                var count = tierCounts.GetValueOrDefault(tier, 0);
                choices.Add($"{tier} ({count})");
            }
            choices.Add(TuiTheme.BackOptionString);

            var choice = await TuiTheme.PromptSelectionOrDefaultAsync("Select a [green]tier[/] to inspect or view all (press [bold]Q[/] to return):", choices);

            if (choice is null or TuiTheme.BackOptionString)
                break;

            if (choice == AllTiersOption)
            {
                BacapAdvancementTierViewer.RenderTree(registry, selectedTier: null);
                TuiTheme.WaitForKey();
                continue;
            }

            var tierName = choice.Split(' ')[0];
            if (!Enum.TryParse<BacapAdvancementTier>(tierName, out var parsedTier))
                continue;

            BacapAdvancementTierViewer.RenderTree(registry, parsedTier);
            TuiTheme.WaitForKey();
        }
    }
}