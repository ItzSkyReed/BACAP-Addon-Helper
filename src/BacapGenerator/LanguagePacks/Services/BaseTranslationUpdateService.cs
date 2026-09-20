using BacapGenerator.Datapacks;
using BacapGenerator.Datapacks.Extensions;
using BacapGenerator.Generation;
using BacapGenerator.LanguagePacks.Models;

namespace BacapGenerator.LanguagePacks.Services;

/// <summary>
/// Orchestrates updating <c>base_translation.json</c> files across all registered modifiable datapacks,
/// resolving compatibility addon relationships automatically.
/// </summary>
public static class BaseTranslationUpdateService
{

    /// <summary>
    /// Scans the datapack registry, matches parent addons with their compatibility extensions,
    /// and generates updated base translation files.
    /// </summary>
    /// <param name="datapackRegistry">The registry containing all loaded datapacks.</param>
    /// <returns>A list of result entries detailing the generated files or failures.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="datapackRegistry"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<BaseTranslationUpdateResult> UpdateAll(DatapackRegistry datapackRegistry)
    {
        ArgumentNullException.ThrowIfNull(datapackRegistry);

        var groups = datapackRegistry.GetLanguagePackAddonGroups();
        var results = new List<BaseTranslationUpdateResult>(groups.Count);

        foreach (var group in groups)
        {
            try
            {
                var file = BaseTranslationGenerator.Generate(group.Primary, group.CompatibilityAddons);
                results.Add(new BaseTranslationUpdateResult(group.Primary, group.CompatibilityAddons, file));
            }
            catch (Exception ex)
            {
                results.Add(new BaseTranslationUpdateResult(group.Primary, group.CompatibilityAddons, null, ex.Message));
            }
        }

        return results;
    }
}