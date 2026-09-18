namespace BacapGenerator.LanguagePacks.Models;

/// <summary>
/// Diagnostic summary for a single processed language file.
/// </summary>
/// <param name="File">The processed language file model.</param>
/// <param name="ExistingCount">The count of already translated entries present in the file.</param>
/// <param name="MissingKeysCount">The count of missing translation keys detected and appended.</param>
/// <param name="WasPatched">Indicates whether missing keys were appended to the file on disk.</param>
public record LanguageFileSyncSummary(
    LanguageFile File,
    int ExistingCount,
    int MissingKeysCount,
    int UnusedKeysCount,
    bool WasPatched);