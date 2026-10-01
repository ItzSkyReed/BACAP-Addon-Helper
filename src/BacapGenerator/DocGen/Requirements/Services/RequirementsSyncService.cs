using BacapGenerator.Advancements.Models;
using BacapGenerator.Datapacks;
using BacapGenerator.DocGen.Requirements.Models;
using BacapGenerator.Io;
using JetBrains.Annotations;

namespace BacapGenerator.DocGen.Requirements.Services;

/// <summary>
/// Service that coordinates the synchronization of advancement requirement documentation for web export.
/// </summary>
public static class RequirementsSyncService
{
    /// <summary>
    /// Synchronizes the requirements YAML file for all datapacks that have document generation enabled.
    /// Extracts missing advancements and appends them with the configured section names.
    /// </summary>
    /// <param name="registry">The source datapack registry.</param>
    /// <param name="requirementsFilePath">The exact path to the target requirements YAML file.</param>
    /// <returns>A result object containing diagnostic information about the sync operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="registry"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="requirementsFilePath"/> is null or whitespace.</exception>
    [PublicAPI]
    public static RequirementsSyncResult SyncAll(DatapackRegistry registry, string requirementsFilePath)
    {
        ArgumentNullException.ThrowIfNull(registry);
        ArgumentException.ThrowIfNullOrWhiteSpace(requirementsFilePath);

        try
        {
            // Filter datapacks where DocumentGenerator is enabled
            var activeDatapacks = registry.Values
                .Where(dp => dp.Settings.DocumentGeneratorSettings?.Enabled == true)
                .ToList();

            if (activeDatapacks.Count == 0)
                return RequirementsSyncResult.Skipped("No datapacks have document generation enabled.");

            // Map: McPath -> HashSet of required SectionNames
            var map = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);

            foreach (var dp in activeDatapacks)
            {
                var sectionName = dp.Settings.DocumentGeneratorSettings!.SectionName;

                foreach (var adv in dp.Advancements.OfType<BacapAdvancement>())
                {

                    if (!map.TryGetValue(adv.McPath, out var sections))
                    {
                        sections = new HashSet<string>(StringComparer.Ordinal);
                        map[adv.McPath] = sections;
                    }

                    sections.Add(sectionName);
                }
            }

            if (map.Count == 0)
                return RequirementsSyncResult.Skipped("No valid BACAP advancements found in the enabled datapacks.");

            // Perform file IO
            var (addedCount, injectedCount) = RequirementsIoManager.SyncRequirementsFile(requirementsFilePath, map);

            return RequirementsSyncResult.Success(
                totalTracked: map.Count,
                addedAdvancements: addedCount,
                injectedSections: injectedCount);
        }
        catch (Exception ex)
        {
            return RequirementsSyncResult.Failed(ex.Message);
        }
    }
}