using BacapGenerator.Datapacks.Extensions;

namespace BacapGenerator.LanguagePacks.Models;
/// <summary>
/// Diagnostic summary for a synchronized datapack and its associated language pack.
/// </summary>
/// <param name="Group">The primary addon and its compatibility addons.</param>
/// <param name="BaseTranslationFile">The generated or updated <c>base_translation.json</c> file.</param>
/// <param name="TotalRequiredKeys">Total unique translation keys discovered across the datapack group.</param>
/// <param name="FileSummaries">Individual file summaries for each language file in the pack.</param>
/// <param name="ErrorMessage">Error message if synchronization failed.</param>
public record TranslationPackSyncResult(
    DatapackRegistryExtensions.AddonGroup Group,
    FileInfo? BaseTranslationFile,
    int TotalRequiredKeys,
    IReadOnlyList<LanguageFileSyncSummary> FileSummaries,
    string? ErrorMessage = null)
{
    /// <summary>
    /// Gets a value indicating whether the synchronization finished without errors.
    /// </summary>
    public bool IsSuccess => BaseTranslationFile is not null && ErrorMessage is null;
}