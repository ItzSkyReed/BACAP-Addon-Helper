using JetBrains.Annotations;

namespace BacapGenerator.LanguagePacks.Models;

/// <summary>
/// Diagnostics summary representing the outcome of synchronizing an individual language file.
/// </summary>
/// <param name="File">The associated language file model.</param>
/// <param name="ExistingCount">Count of valid translation keys parsed from the file.</param>
/// <param name="MissedCount">Count of discovered missing translation keys appended.</param>
/// <param name="RemovedCount">Count of obsolete keys removed from the file.</param>
/// <param name="WasPatched">Indicates whether physical changes were written to disk.</param>
/// <param name="ErrorMessage">The diagnostic error message if the file was corrupted or failed to process.</param>
public record LanguageFileSyncSummary(
    LanguageFile File,
    int ExistingCount,
    int MissedCount,
    int RemovedCount,
    bool WasPatched,
    string? ErrorMessage = null
)
{
    /// <summary>
    /// Gets a value indicating whether this file encountered errors preventing synchronization.
    /// </summary>
    [PublicAPI]
    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

    /// <summary>
    /// Creates a summary representing a successfully parsed and synchronized language file.
    /// </summary>
    public static LanguageFileSyncSummary Success(
        LanguageFile file,
        int existingCount,
        int missedCount,
        int removedCount,
        bool wasPatched) =>
        new(file, existingCount, missedCount, removedCount, wasPatched);

    /// <summary>
    /// Creates a summary for a quarantined file that could not be parsed safely.
    /// </summary>
    public static LanguageFileSyncSummary Failed(FileInfo file, string errorMessage)
    {
        var nameParts = Path.GetFileNameWithoutExtension(file.Name).Split('_', 2);

        return new LanguageFileSyncSummary(
            File: new LanguageFile
            {
                File = file,
                MajorLanguageGroup = nameParts.Length > 0 ? nameParts[0].ToLowerInvariant() : "unknown",
                MinorLanguageGroup = nameParts.Length > 1 ? nameParts[1].ToLowerInvariant() : "unknown",
                Translations = []
            },
            ExistingCount: 0,
            MissedCount: 0,
            RemovedCount: 0,
            WasPatched: false,
            ErrorMessage: errorMessage);
    }
}