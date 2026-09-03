using BacapGenerator.Models.Advancements;
using BacapGenerator.Models.Datapacks;
using BacapGenerator.Services.Generators;
using BacapGenerator.Services.IO;
using BacapGenerator.Services.Selectors;

namespace BacapGenerator.Services.Global;

/// <summary>
/// Orchestrates the generation and saving of special global advancements
/// like Milestones and the Advancement Legend for a given datapack.
/// </summary>
public static class GlobalAdvancementsService
{
    /// <summary>
    /// Generates and saves all milestones and the legend advancement for the datapack.
    /// </summary>
    /// <param name="datapack">The target datapack to process.</param>
    public static void GenerateAndSaveAll(Datapack datapack)
    {
        var settings = datapack.Settings;

        // Ensure there is anything to do
        if (settings.MilestoneMcPaths == null || settings.MilestoneMcPaths.Count == 0)
            return;

        var bacapAdvancements = datapack.Advancements.OfType<BacapAdvancement>().ToList();
        var advByPath = bacapAdvancements.ToDictionary(a => a.McPath);

        // Filter using our extension method
        var validAdvancements = bacapAdvancements.GetValidPlayable(settings.AdvancementLegendMcPath).ToList();

        var advancementsByTab = validAdvancements
            .GroupBy(a => a.Tab.FolderName)
            .ToDictionary(g => g.Key, g => g.ToList());

        // Generate and save Milestones
        foreach (var (tabFolder, milestonePath) in settings.MilestoneMcPaths)
        {
            if (!advByPath.TryGetValue(milestonePath, out var milestoneAdv))
                continue;

            var tabAdvancements = advancementsByTab.GetValueOrDefault(tabFolder, []);
            milestoneAdv.Advancement = MilestoneGenerator.GenerateMilestone(milestoneAdv, tabAdvancements);

            AdvancementIoManager.SaveAdvancement(milestoneAdv);
        }

        //  Generate and save Legend
        if (string.IsNullOrWhiteSpace(settings.AdvancementLegendMcPath) ||
            !advByPath.TryGetValue(settings.AdvancementLegendMcPath, out var legendAdv))
            return;

        legendAdv.Advancement = MilestoneGenerator.GenerateAdvancementLegend(legendAdv, validAdvancements);

        AdvancementIoManager.SaveAdvancement(legendAdv);
    }
}