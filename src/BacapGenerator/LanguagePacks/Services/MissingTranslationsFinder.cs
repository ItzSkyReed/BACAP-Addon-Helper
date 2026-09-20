using BacapGenerator.LanguagePacks.Models;

namespace BacapGenerator.LanguagePacks.Services;

/// <summary>
/// Service that evaluates loaded language files against required datapack translation keys
/// and identifies missing localizations.
/// </summary>
public static class MissingTranslationsFinder
{
    /// <summary>
    /// Compares all language files in a pack against required datapack translation keys.
    /// </summary>
    /// <param name="languagePack">The loaded language pack containing target files.</param>
    /// <param name="requiredKeys">The collection of translation keys required by the datapack.</param>
    /// <param name="syncMajorGroupsFirst">If <see langword="true"/>, copies translations across dialect variants before evaluating missing keys.</param>
    /// <returns>A list of diagnostic reports per language file.</returns>
    /// <exception cref="ArgumentNullException">Thrown when any required argument is <see langword="null"/>.</exception>
    /// <example>
    /// <code>
    /// var finder = new MissingTranslationsFinder();
    /// IReadOnlyList&lt;LanguageFileMissingReport&gt; reports = finder.FindMissing(pack, discoveredKeys);
    /// </code>
    /// </example>
    public static IReadOnlyList<LanguageFileMissingReport> FindMissing(
        LanguagePack languagePack,
        IReadOnlyCollection<string> requiredKeys,
        bool syncMajorGroupsFirst = true)
    {
        ArgumentNullException.ThrowIfNull(languagePack);
        ArgumentNullException.ThrowIfNull(requiredKeys);

        if (syncMajorGroupsFirst)
            languagePack.SynchronizeMajorGroupTranslations();

        var reports = new List<LanguageFileMissingReport>(languagePack.Files.Count);

        foreach (var file in languagePack.Files)
        {
            var missing = new List<string>();

            foreach (var key in requiredKeys)
            {
                if (!file.Translations.TryGetValue(key, out var translation) || string.IsNullOrWhiteSpace(translation))
                    missing.Add(key);
            }

            reports.Add(new LanguageFileMissingReport(file, missing));
        }

        return reports;
    }
}