using BacapGenerator.Models.Interfaces;

namespace BacapGenerator.Validation.Models;


/// <summary>
/// Encapsulates the execution context and state required for pack-level and item-level validations.
/// </summary>
public sealed class ValidationContext
{
    /// <summary>
    /// Gets the target datapack under validation.
    /// </summary>
    public required IReadOnlyDatapack Datapack { get; init; }

    /// <summary>
    /// Collects reported diagnostics during the validation lifecycle.
    /// </summary>
    public List<ValidationIssue> Diagnostics { get; } = [];

    /// <summary>
    /// Appends a new diagnostic message into the context.
    /// </summary>
    /// <param name="diagnostic">The diagnostic entry to report.</param>
    public void Report(ValidationIssue diagnostic) => Diagnostics.Add(diagnostic);
}