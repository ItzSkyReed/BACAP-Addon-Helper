using BacapGenerator.Advancements.Models;

namespace BacapGenerator.Validation.Models;

/// <summary>
/// Specifies the severity level of a validation diagnostic.
/// </summary>
public enum ValidationSeverity
{
    Ignore,
    Info,
    Warning,
    Error
}


/// <summary>
/// Represents an issue identified during the advancement validation process.
/// </summary>
/// <param name="RuleId">The unique identifier of the triggered rule (e.g., "BACAP001").</param>
/// <param name="Severity">The severity level of this diagnostic.</param>
/// <param name="Message">Human-readable explanation of the issue.</param>
/// <param name="Advancement">The object of the affected advancement.</param>
/// <param name="PropertyPath">Optional dot-separated path to the problematic property (e.g., "Display.Title").</param>
public sealed record ValidationIssue(
    string RuleId,
    ValidationSeverity Severity,
    string Message,
    ManagedAdvancement? Advancement = null,
    string? PropertyPath = null);