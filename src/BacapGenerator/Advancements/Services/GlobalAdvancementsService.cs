using BacapGenerator.Advancements.Filtering;
using BacapGenerator.Advancements.Models;
using BacapGenerator.Datapacks.Models;
using BacapGenerator.Generation;
using BacapGenerator.Io;

namespace BacapGenerator.Advancements.Services;

/// <summary>
/// Orchestrates the generation and saving of special global advancements
/// like Milestones and the Advancement Legend for a given datapack.
/// </summary>
public sealed class GlobalAdvancementsService
{
    /// <summary>
    /// Generates and saves all milestones and the legend advancement for the datapack.
    /// </summary>
    /// <param name="datapack">The target datapack to process.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="datapack"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when milestone paths are missing in settings or not found in the loaded datapack.</exception>
    public void GenerateAndSaveAll(Datapack datapack)
    {
        ArgumentNullException.ThrowIfNull(datapack);

        var settings = datapack.Settings;

        if (settings.MilestoneMcPaths is not { Count: > 0 })
        {
            throw new InvalidOperationException(
                $"Datapack '{datapack.Id}' has no MilestoneMcPaths configured (check YAML binding for 'milestone_mc_paths').");
        }

        var bacapAdvancements = datapack.Advancements.OfType<BacapAdvancement>().ToList();
        var advByPath = bacapAdvancements.ToDictionary(a => a.McPath, StringComparer.OrdinalIgnoreCase);

        var validAdvancements = bacapAdvancements.GetValidPlayable(settings.AdvancementLegendMcPath).ToList();

        var advancementsByTab = validAdvancements
            .GroupBy(a => a.Tab)
            .ToDictionary(g => g.Key, g => g.ToList());

        foreach (var (tab, milestonePath) in settings.MilestoneMcPaths)
        {
            if (!advByPath.TryGetValue(milestonePath, out var milestoneAdv))
            {
                throw new InvalidOperationException(
                    $"Milestone '{milestonePath}' for tab '{tab}' was not found among loaded BacapAdvancements. " +
                    $"Total BacapAdvancements: {bacapAdvancements.Count}. Sample McPath: '{bacapAdvancements.FirstOrDefault()?.McPath}'.");
            }

            var tabAdvancements = advancementsByTab.GetValueOrDefault(tab, []);
            milestoneAdv.Advancement = MilestoneGenerator.GenerateMilestone(milestoneAdv, tabAdvancements);

            AdvancementIoManager.SaveAdvancement(milestoneAdv);
        }

        if (!string.IsNullOrWhiteSpace(settings.AdvancementLegendMcPath))
        {
            if (!advByPath.TryGetValue(settings.AdvancementLegendMcPath, out var legendAdv))
            {
                throw new InvalidOperationException(
                    $"Legend advancement '{settings.AdvancementLegendMcPath}' was not found in loaded BacapAdvancements.");
            }

            legendAdv.Advancement = MilestoneGenerator.GenerateAdvancementLegend(legendAdv, validAdvancements);
            AdvancementIoManager.SaveAdvancement(legendAdv);
        }
    }
}