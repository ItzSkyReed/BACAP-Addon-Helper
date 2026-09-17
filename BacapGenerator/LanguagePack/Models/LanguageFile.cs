namespace BacapGenerator.LanguagePack.Models;

/// <summary>
/// Represents single language file
/// </summary>
public class LanguageFile
{
    /// <summary>
    /// Gets the major language group
    /// </summary>
    /// <example>
    /// In 'es_ar' -> 'es'
    /// </example>
    public string MajorLanguageGroup { get; init; }

    /// <summary>
    /// Gets the minor language group
    /// </summary>
    /// <example>
    /// In 'es_ar' -> 'ar'
    /// </example>
    public string MinorLanguageGroup { get; init; }

    /// <summary>
    /// Translations in the language file
    /// </summary>
    public Dictionary<string, string> Translations { get; init; }
}