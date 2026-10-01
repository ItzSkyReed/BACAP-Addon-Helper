using BacapGenerator.Advancements.Models;
using BacapGenerator.Configuration;
using BacapGenerator.Datapacks;
using BacapGenerator.Datapacks.Models;
using BacapGenerator.Datapacks.Models.Settings;
using BacapGenerator.DocGen.Documentation.Services;
using BacapGenerator.Io;
using JetBrains.Annotations;

namespace BacapGenerator.DocGen;

/// <summary>
/// Orchestrates the synchronization of YAML requirements and the generation of JSON documentation.
/// </summary>
public static class DocumentationPipeline
{
    /// <summary>
    /// Runs the documentation pipeline.
    /// If new YAML stubs are created, it aborts generation and returns <see cref="DocumentationSyncResult.StubsGenerated"/>.
    /// </summary>
    [PublicAPI]
    public static DocumentationSyncResult Run(DatapackRegistry registry, DocumentGeneratorConfig? docConfig)
    {
        if (docConfig is null)
            return DocumentationSyncResult.Skipped("The 'document_generator' section is not configured.");

        var primaryAddons = registry.Values
            .Where(dp => dp.Settings is { DatapackType: DatapackType.Addon, DocumentGeneratorSettings.Enabled: true })
            .ToList();

        if (primaryAddons.Count == 0)
            return DocumentationSyncResult.Skipped("No addons currently have document generation enabled.");

        var totalAdded = 0;
        var totalInjected = 0;

        // Sync YAML Requirements
        foreach (var primaryAddon in primaryAddons)
        {
            var compatAddons = registry.Values
                .Where(dp => dp.Settings.DatapackType == DatapackType.CompatibilityAddon
                             && string.Equals(dp.Settings.ParentDatapackId, primaryAddon.Id, StringComparison.OrdinalIgnoreCase)
                             && dp.Settings.DocumentGeneratorSettings?.Enabled == true);

            var groupDatapacks = new List<Datapack> { primaryAddon };
            groupDatapacks.AddRange(compatAddons);

            var sectionsMap = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);

            foreach (var dp in groupDatapacks)
            {
                var sectionName = dp.Settings.DocumentGeneratorSettings!.SectionName;

                foreach (var adv in dp.Advancements.OfType<BacapAdvancement>())
                {
                    if (!sectionsMap.TryGetValue(adv.McPath, out var sections))
                    {
                        sections = new HashSet<string>(StringComparer.Ordinal);
                        sectionsMap[adv.McPath] = sections;
                    }
                    sections.Add(sectionName);
                }
            }

            var yamlPath = Path.Combine(docConfig.RequirementsDirectory!, $"{primaryAddon.Id}.yaml");
            var (added, injected) = RequirementsIoManager.SyncRequirementsFile(yamlPath, sectionsMap);

            totalAdded += added;
            totalInjected += injected;
        }

        // If we had to add new requirements, abort so the user can fill them
        if (totalAdded > 0 || totalInjected > 0)
            return DocumentationSyncResult.StubsGeneratedResult(totalAdded, totalInjected);

        // Generate JSON
        DocumentationExportService.GenerateExportFiles(registry, docConfig);

        return DocumentationSyncResult.Success(primaryAddons.Count);
    }
}