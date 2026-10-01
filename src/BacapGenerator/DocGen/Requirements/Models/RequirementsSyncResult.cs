namespace BacapGenerator.DocGen.Requirements.Models;

/// <summary>
/// Represents the result of a requirements synchronization operation.
/// </summary>
public record RequirementsSyncResult(
    bool IsSuccess,
    bool WasSkipped,
    int TotalTracked,
    int AddedAdvancements,
    int InjectedSections,
    string? ErrorMessage)
{
    public static RequirementsSyncResult Skipped(string message) =>
        new(true, true, 0, 0, 0, message);

    public static RequirementsSyncResult Success(int totalTracked, int addedAdvancements, int injectedSections) =>
        new(true, false, totalTracked, addedAdvancements, injectedSections, null);

    public static RequirementsSyncResult Failed(string error) =>
        new(false, false, 0, 0, 0, error);
}