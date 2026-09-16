
using BacapGenerator.Validation.Models;

namespace BacapGenerator.Datapacks.Models.Settings.Validation.Rules;

/// <summary>
/// Defines the common contract for rule configuration options.
/// </summary>
public interface IRuleOptions
{
    /// <summary>
    /// Gets a value indicating whether the rule is active.
    /// </summary>
    bool Enabled { get; }

    /// <summary>
    /// Gets the severity level reported for violations.
    /// </summary>
    ValidationSeverity Severity { get; }
}