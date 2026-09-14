using BacapGenerator.Validation.Models;

namespace BacapGenerator.Validation.Interfaces;

/// <summary>
/// Defines the contract for all validation rules.
/// </summary>
public interface IValidationRule
{
    /// <summary>
    /// Gets the unique code identifying the rule (e.g., "BACAP_TXT_01").
    /// </summary>
    string RuleId { get; }

    /// <summary>
    /// Gets the descriptive name of the rule.
    /// </summary>
    string DisplayName { get; }

    /// <summary>
    /// Gets the configured severity level for issues emitted by this rule.
    /// </summary>
    ValidationSeverity Severity { get; }
}