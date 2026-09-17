using BacapGenerator.Datapacks;
using BacapGenerator.Datapacks.Models.Settings;
using BacapGenerator.Generation;
using BacapGenerator.LanguagePack.Models;

namespace BacapGenerator.LanguagePack.Services;

/// <summary>
/// Orchestrates updating <c>base_translation.json</c> files across all registered modifiable datapacks,
/// resolving compatibility addon relationships automatically.
/// </summary>
public sealed class BaseTranslationUpdateService(BaseTranslationGenerator generator)
{
    public BaseTranslationUpdateService() : this(new BaseTranslationGenerator())
    {
    }

    /// <summary>
    /// Scans the datapack registry, matches parent addons with their compatibility extensions,
    /// and generates updated base translation files.
    /// </summary>
    /// <param name="datapackRegistry">The registry containing all loaded datapacks.</param>
    /// <returns>A list of result entries detailing the generated files or failures.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="datapackRegistry"/> is <see langword="null"/>.</exception>
    /// <example>
    /// <code>
    /// var updateService = new BaseTranslationUpdateService();
    /// IReadOnlyList&lt;BaseTranslationUpdateResult&gt; results = updateService.UpdateAll(registry);
    /// </code>
    /// </example>
    public IReadOnlyList<BaseTranslationUpdateResult> UpdateAll(DatapackRegistry datapackRegistry)
    {
        ArgumentNullException.ThrowIfNull(datapackRegistry);

        var results = new List<BaseTranslationUpdateResult>();


        // Only scan standalone and primary Addons that specify a language pack
        var primaryPacks = datapackRegistry.Values
            .Where(dp => dp.Settings is { Type: DatapackType.Addon, LanguagePackSettigs: not null } &&
                         !string.IsNullOrWhiteSpace(dp.Settings.LanguagePackSettigs.Path))
            .ToList();

        foreach (var mainPack in primaryPacks)
        {
            // Resolve all child compatibility addons linked to this parent pack
            var compatAddons = datapackRegistry.Values
                .Where(dp => dp.Settings.Type == DatapackType.CompatibilityAddon &&
                             string.Equals(dp.Settings.ParentDatapackId, mainPack.Id, StringComparison.OrdinalIgnoreCase))
                .ToList();

            try
            {
                var file = generator.Generate(mainPack, compatAddons);
                results.Add(new BaseTranslationUpdateResult(mainPack, compatAddons, file));
            }
            catch (Exception ex)
            {
                results.Add(new BaseTranslationUpdateResult(mainPack, compatAddons, null, ex.Message));
            }
        }

        return results;
    }
}