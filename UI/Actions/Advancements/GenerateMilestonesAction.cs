using BacapGenerator.Models.Advancements;
using BacapGenerator.Models.Datapacks;
using BacapGenerator.Models.Datapacks.Settings;
using BacapGenerator.Services;
using Spectre.Console;
using UI.Interfaces;
using UI.Styling;

namespace UI.Actions.Advancements;

/// <summary>
/// Action that generates and updates milestone and advancement legend criteria across all eligible ReadWrite datapacks.
/// </summary>
/// <param name="registry">The datapack registry containing loaded datapack instances.</param>
public class GenerateMilestonesAction(
    DatapackRegistry registry) : IManageAdvancementsAction
{
    public string Title => "Generate Milestones & Advancement Legend";

    public Task ExecuteAsync()
    {
        var targetDatapacks = registry.All.Values
            .Where(dp => dp.Settings is { Access: DatapackAccess.ReadWrite, MilestoneMcPaths.Count: > 0 } &&
                         !string.IsNullOrWhiteSpace(dp.Settings.AdvancementLegendMcPath))
            .ToList();

        if (targetDatapacks.Count == 0)
        {
            TuiTheme.ShowWarning("No datapacks found with ReadWrite access, MilestoneMcPaths, and AdvancementLegendMcPath configured.");
            TuiTheme.WaitForKey();
            return Task.CompletedTask;
        }

        TuiTheme.RenderHeader(Title);

        var updatedAdvancements = new List<BacapAdvancement>();

        foreach (var datapack in targetDatapacks)
        {
            // Get all BacapAdvancements ONCE
            var bacapAdvancements = datapack.Advancements.OfType<BacapAdvancement>().ToList();

            // Create O(1) lookup dictionary by McPath
            var advByPath = bacapAdvancements.ToDictionary(a => a.McPath);

            // Filter out Root/Hidden and group by Tab folder name for Milestones
            var validAdvancements = bacapAdvancements
                .Where(a =>
                    a.Tier is not (BacapAdvancementTier.Root or BacapAdvancementTier.Hidden) &&
                    a.McPath != datapack.Settings.AdvancementLegendMcPath)
                .ToList();

            var advancementsByTab = validAdvancements
                .GroupBy(a => a.Tab.FolderName)
                .ToDictionary(g => g.Key, g => g.ToList());

            // Generate Milestones
            foreach (var (tabFolder, milestonePath) in datapack.Settings.MilestoneMcPaths!)
            {
                // Ensure the milestone file actually exists in the datapack
                if (!advByPath.TryGetValue(milestonePath, out var milestoneAdv))
                    continue;

                // Get advancements for this tab (or empty if none exist)
                var tabAdvancements = advancementsByTab.GetValueOrDefault(tabFolder, []);

                milestoneAdv.Advancement = MilestoneGenerator.GenerateMilestone(milestoneAdv, tabAdvancements);
                updatedAdvancements.Add(milestoneAdv);
            }

            // Generate Legend
            if (!advByPath.TryGetValue(datapack.Settings.AdvancementLegendMcPath!, out var legendAdv))
                continue;

            // Pass ALL valid advancements to the legend generator
            legendAdv.Advancement = MilestoneGenerator.GenerateAdvancementLegend(legendAdv, validAdvancements);
            updatedAdvancements.Add(legendAdv);
        }

        if (updatedAdvancements.Count == 0)
        {
            TuiTheme.ShowWarning("No milestones or legends matched the criteria to generate.");
            TuiTheme.WaitForKey();
            return Task.CompletedTask;
        }

        var confirm = AnsiConsole.Confirm(
            $"Found [green]{targetDatapacks.Count}[/] datapack(s). Ready to generate and overwrite [green]{updatedAdvancements.Count}[/] milestone/legend files?");

        if (!confirm)
            return Task.CompletedTask;

        TuiTheme.Space();

        TuiTheme.RunProgress(
            "Writing generated milestones and legends to disk...",
            updatedAdvancements,
            AdvancementIoManager.SaveAdvancement
        );

        TuiTheme.Space();
        TuiTheme.ShowSuccess($"Successfully generated and saved {updatedAdvancements.Count} milestone and legend files!");
        TuiTheme.WaitForKey();

        return Task.CompletedTask;
    }
}