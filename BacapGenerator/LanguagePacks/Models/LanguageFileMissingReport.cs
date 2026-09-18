namespace BacapGenerator.LanguagePacks.Models;

/// <summary>
/// Diagnostic report entry describing missing translation keys for a single language file.
/// </summary>
/// <param name="File">The language file model.</param>
/// <param name="MissingKeys">The collection of keys that are not yet translated in this file.</param>
public record LanguageFileMissingReport(
    LanguageFile File,
    IReadOnlyList<string> MissingKeys
)
{
    /// <summary>
    /// Gets a value indicating whether this file has any untranslated keys.
    /// </summary>
    public bool HasMissingKeys => MissingKeys.Count > 0;
}