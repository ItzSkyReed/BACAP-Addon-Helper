namespace BacapGenerator.LanguagePacks.Models;

/// <summary>
/// Represents a loaded Minecraft language resource pack containing all localized translation files.
/// </summary>
public sealed class LanguagePack
{
    /// <summary>
    /// Gets the root directory of the language resource pack.
    /// </summary>
    public DirectoryInfo RootDirectory { get; }

    /// <summary>
    /// Gets the directory containing the localization JSON files (<c>assets/minecraft/lang</c>).
    /// </summary>
    public DirectoryInfo LangDirectory { get; }

    /// <summary>
    /// Gets the collection of loaded language files.
    /// </summary>
    public IReadOnlyList<LanguageFile> Files { get; }

    /// <summary>
    /// Gets language files grouped by their major language code (e.g., "es" -> [es_ar, es_cl, es_es]).
    /// </summary>
    public IReadOnlyDictionary<string, IReadOnlyList<LanguageFile>> ByMajorGroup { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="LanguagePack"/> class.
    /// </summary>
    /// <param name="rootDirectory">The root directory of the language pack.</param>
    /// <param name="files">The loaded language files.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="rootDirectory"/> or <paramref name="files"/> is <see langword="null"/>.</exception>
    public LanguagePack(DirectoryInfo rootDirectory, IEnumerable<LanguageFile> files)
    {
        ArgumentNullException.ThrowIfNull(rootDirectory);
        ArgumentNullException.ThrowIfNull(files);

        RootDirectory = rootDirectory;
        LangDirectory = new DirectoryInfo(Path.Combine(rootDirectory.FullName, "assets", "minecraft", "lang"));
        Files = files.ToList().AsReadOnly();

        ByMajorGroup = Files
            .GroupBy(f => f.MajorLanguageGroup, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                g => g.Key, IReadOnlyList<LanguageFile> (g) => g.ToList().AsReadOnly(),
                StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Synchronizes translations across all dialect files within each major language group.
    /// If one dialect (e.g., 'es_es') contains a non-empty translation, all other dialects in the group receive it.
    /// </summary>
    /// <returns>The total number of translation values propagated across dialect files.</returns>
    /// <example>
    /// <code>
    /// int copied = languagePack.SynchronizeMajorGroupTranslations();
    /// </code>
    /// </example>
    public int SynchronizeMajorGroupTranslations()
    {
        var totalSynced = 0;

        foreach (var (_, groupFiles) in ByMajorGroup)
        {
            if (groupFiles.Count <= 1)
                continue;

            // Merge all non-empty translations across the entire major group
            var merged = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var file in groupFiles)
            {
                foreach (var (key, value) in file.Translations)
                {
                    if (!string.IsNullOrWhiteSpace(value))
                        merged.TryAdd(key, value);
                }
            }

            // Distribute missing keys to files that lack them
            foreach (var file in groupFiles)
            {
                foreach (var (key, value) in merged)
                {
                    if (file.Translations.TryGetValue(key, out var existing) && !string.IsNullOrWhiteSpace(existing))
                        continue;

                    file.Translations[key] = value;
                    totalSynced++;
                }
            }
        }

        return totalSynced;
    }
}