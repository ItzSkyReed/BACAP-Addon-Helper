using JetBrains.Annotations;

namespace BacapGenerator.DocGen;

/// <summary>
/// Result of a documentation sync pipeline execution.
/// </summary>
/// <param name="IsSuccess">Indicates whether the entire pipeline completed successfully.</param>
/// <param name="WasSkipped">Indicates whether the execution was skipped due to configuration or missing addons.</param>
/// <param name="StubsGenerated">Indicates whether generation was halted because new requirement stubs were created.</param>
/// <param name="TotalAdded">The number of new advancement requirement stubs added.</param>
/// <param name="TotalInjected">The number of new sections injected into existing requirements.</param>
/// <param name="ProcessedAddons">The total count of primary addons processed.</param>
/// <param name="ErrorMessage">The error description if the operation failed; otherwise, <see langword="null"/>.</param>
/// <param name="OrphanedAdvancements">A dictionary mapping addon IDs to lists of requirement paths that no longer exist in the datapacks.</param>
[PublicAPI]
public record DocumentationSyncResult(
    bool IsSuccess,
    bool WasSkipped,
    bool StubsGenerated,
    int TotalAdded,
    int TotalInjected,
    int ProcessedAddons,
    string? ErrorMessage,
    IReadOnlyDictionary<string, IReadOnlyList<string>> OrphanedAdvancements)
{
    /// <summary>
    /// Gets a value indicating whether any obsolete or orphaned requirement entries were detected.
    /// </summary>
    public bool HasOrphanedAdvancements => OrphanedAdvancements.Values.Any(list => list.Count > 0);

    /// <summary>
    /// Creates a result representing a skipped pipeline execution.
    /// </summary>
    /// <param name="message">The reason why the operation was skipped.</param>
    /// <returns>A new <see cref="DocumentationSyncResult"/> instance marked as skipped.</returns>
    public static DocumentationSyncResult Skipped(string message) =>
        new(true, true, false, 0, 0, 0, message, new Dictionary<string, IReadOnlyList<string>>());

    /// <summary>
    /// Creates a result indicating that new stubs or sections were injected, requiring user completion.
    /// </summary>
    /// <param name="added">The number of newly added advancement entries.</param>
    /// <param name="injected">The number of injected sections.</param>
    /// <param name="orphanedAdvancements">Optional dictionary of orphaned advancement paths discovered during execution.</param>
    /// <returns>A new <see cref="DocumentationSyncResult"/> instance indicating pending stubs.</returns>
    public static DocumentationSyncResult StubsGeneratedResult(
        int added,
        int injected,
        IReadOnlyDictionary<string, IReadOnlyList<string>>? orphanedAdvancements = null) =>
        new(false, false, true, added, injected, 0, null, orphanedAdvancements ?? new Dictionary<string, IReadOnlyList<string>>());

    /// <summary>
    /// Creates a result representing a fully successful pipeline run.
    /// </summary>
    /// <param name="processedAddons">The number of primary addons successfully generated.</param>
    /// <param name="orphanedAdvancements">Optional dictionary of orphaned advancement paths discovered during execution.</param>
    /// <returns>A new <see cref="DocumentationSyncResult"/> instance marked as successful.</returns>
    public static DocumentationSyncResult Success(
        int processedAddons,
        IReadOnlyDictionary<string, IReadOnlyList<string>>? orphanedAdvancements = null) =>
        new(true, false, false, 0, 0, processedAddons, null, orphanedAdvancements ?? new Dictionary<string, IReadOnlyList<string>>());

    /// <summary>
    /// Creates a result representing an execution failure.
    /// </summary>
    /// <param name="error">The diagnostic error message.</param>
    /// <returns>A new <see cref="DocumentationSyncResult"/> instance marked as failed.</returns>
    public static DocumentationSyncResult Failed(string error) =>
        new(false, false, false, 0, 0, 0, error, new Dictionary<string, IReadOnlyList<string>>());
}