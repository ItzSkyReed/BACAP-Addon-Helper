using BacapGenerator.Datapacks;
using BacapGenerator.Datapacks.Extensions;
using BacapGenerator.Generation;
using BacapGenerator.Io;
using BacapGenerator.LanguagePacks.Models;

namespace BacapGenerator.LanguagePacks.Services;

/// <summary>
/// Service that coordinates end-to-end translation pack synchronization:
/// updates <c>base_translation.json</c>, discovers missing keys, and patches individual language files.
/// </summary>
public static class TranslationPackSyncService
{
    /// <summary>
    /// Synchronizes translation files and updates base templates across all registered primary addon groups.
    /// </summary>
    /// <param name="registry">The source datapack registry.</param>
    /// <returns>A list of diagnostic results per addon group.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="registry"/> is <see langword="null"/>.</exception>
    /// <example>
    /// <code>
    /// IReadOnlyList&lt;TranslationPackSyncResult&gt; results = TranslationPackSyncService.SyncAll(datapackRegistry);
    /// </code>
    /// </example>
    public static IReadOnlyList<TranslationPackSyncResult> SyncAll(DatapackRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(registry);

        var groups = registry.GetLanguagePackAddonGroups();
        var results = new List<TranslationPackSyncResult>(groups.Count);

        foreach (var group in groups)
        {
            try
            {
                var settings = group.Primary.Settings.LanguagePackSettigs;
                if (settings is null || string.IsNullOrWhiteSpace(settings.Path))
                {
                    results.Add(new TranslationPackSyncResult(group, null, 0, [], "Language pack settings or path are missing."));
                    continue;
                }

                // Discover all active translation keys across the main pack and compatibility addons
                var allKeys = TranslationKeyDiscoveryService.DiscoverKeys(group.All);
                var allKeysSet = allKeys.ToHashSet(StringComparer.Ordinal);

                // Generate base_translation.json
                var baseFile = BaseTranslationGenerator.Generate(group.Primary, group.CompatibilityAddons);

                // Load the language resource pack from disk via the IO Manager
                var pack = LanguagePackIoManager.LoadPack(settings.Path);

                // Find missing keys across language files (syncing dialect variants first)
                var missingReports = MissingTranslationsFinder.FindMissing(pack, allKeys, syncMajorGroupsFirst: true);

                // Update each individual file
                var summaries = new List<LanguageFileSyncSummary>(missingReports.Count);

                foreach (var report in missingReports)
                {
                    // Detect obsolete keys present in the file but absent from active datapacks
                    var unusedKeys = report.File.Translations.Keys
                        .Where(k => !allKeysSet.Contains(k) && !settings.IgnoredKeysSet.Contains(k))
                        .ToList();

                    var keysToRemove = settings.RemoveUnusedKeys ? unusedKeys : [];
                    var wasPatched = false;

                    if (report.HasMissingKeys || keysToRemove.Count > 0)
                    {
                        wasPatched = LanguagePackIoManager.UpdateLanguageFile(
                            report.File,
                            report.MissingKeys,
                            keysToRemove);
                    }

                    // Sync in-memory translations if keys were removed on disk
                    if (settings.RemoveUnusedKeys && wasPatched)
                    {
                        foreach (var unusedKey in unusedKeys)
                            report.File.Translations.Remove(unusedKey);
                    }

                    summaries.Add(new LanguageFileSyncSummary(
                        report.File,
                        report.File.Translations.Count,
                        report.MissingKeys.Count,
                        keysToRemove.Count,
                        wasPatched));
                }

                results.Add(new TranslationPackSyncResult(group, baseFile, allKeys.Count, summaries));
            }
            catch (Exception ex)
            {
                results.Add(new TranslationPackSyncResult(group, null, 0, [], ex.Message));
            }
        }

        return results;
    }
}