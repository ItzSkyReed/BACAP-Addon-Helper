using BacapGenerator.Models.Advancements;
using BacapGenerator.Models.Datapacks;
using Spectre.Console;
using UI.Interfaces;
using UI.Styling;

namespace UI.Actions.Advancements;

/// <summary>
/// Action that calculates and displays statistics for all loaded datapacks.
/// </summary>
public class AdvancementStatsAction(DatapackRegistry registry) : IManageAdvancementsAction
{
    public string Title => "Stats";

    public Task ExecuteAsync()
    {
        TuiTheme.RenderHeader("Datapack Statistics");

        var tiers = Enum.GetValues<BacapAdvancementTier>();

        // Build columns dynamically
        var columns = new List<string> { "Datapack" };
        columns.AddRange(tiers.Select(t => $"[{t.Color()}]{t.DisplayName()}[/]"));
        columns.Add("[bold yellow]Total[/]");

        var table = TuiTheme.CreateTable([.. columns]);

        foreach (var (id, datapack) in registry)
        {
            var tierCounts = datapack.Advancements
                .OfType<BacapAdvancement>()
                .GroupBy(adv => adv.Tier)
                .ToDictionary(group => group.Key, group => group.Count());

            var row = new List<string> { $"[cyan]{id}[/]" };

            foreach (var tier in tiers)
            {
                var count = tierCounts.GetValueOrDefault(tier, 0);
                row.Add(count.ToString());
            }

            var total = tierCounts.Values.Sum();
            row.Add($"[bold yellow]{total}[/]");

            table.AddRow([.. row]);
        }

        TuiTheme.RenderElement(table);
        TuiTheme.WaitForKey();

        return Task.CompletedTask;
    }
}