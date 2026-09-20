namespace BacapGenerator.LanguagePacks.Models;

/// <summary>
/// Represents a single Minecraft language file (e.g., <c>ru_ru.json</c>, <c>es_es.json</c>).
/// </summary>
public class LanguageFile
{
    /// <summary>
    /// Gets the major language group code.
    /// </summary>
    /// <example>
    /// In 'es_ar' -> 'es'.
    /// </example>
    public required string MajorLanguageGroup { get; init; }

    /// <summary>
    /// Gets the minor language group or region code.
    /// </summary>
    /// <example>
    /// In 'es_ar' -> 'ar'.
    /// </example>
    public required string MinorLanguageGroup { get; init; }

    /// <summary>
    /// Gets the full language locale code (e.g., 'es_ar', 'ru_ru').
    /// </summary>
    public string Code => $"{MajorLanguageGroup}_{MinorLanguageGroup}";

    /// <summary>
    /// Gets the underlying file reference on disk.
    /// </summary>
    public required FileInfo File { get; init; }

    /// <summary>
    /// Gets the parsed translation key-value mappings present in the file.
    /// </summary>
    public Dictionary<string, string> Translations { get; init; } = new(StringComparer.Ordinal);
}