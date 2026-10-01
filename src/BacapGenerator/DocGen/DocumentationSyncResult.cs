namespace BacapGenerator.DocGen;

/// <summary>
/// Result of a web documentation sync pipeline execution.
/// </summary>
public record DocumentationSyncResult(
    bool IsSuccess,
    bool WasSkipped,
    bool StubsGenerated,
    int TotalAdded,
    int TotalInjected,
    int ProcessedAddons,
    string? ErrorMessage)
{
    public static DocumentationSyncResult Skipped(string message) =>
        new(true, true, false, 0, 0, 0, message);

    public static DocumentationSyncResult StubsGeneratedResult(int added, int injected) =>
        new(false, false, true, added, injected, 0, null);

    public static DocumentationSyncResult Success(int processedAddons) =>
        new(true, false, false, 0, 0, processedAddons, null);
}