using BacapGenerator.Validation.Models;
using Microsoft.Extensions.Configuration;

namespace BacapGenerator.Configuration.Validation.Rules;

/// <summary>
/// Represents generic configuration for simple boolean/severity-based validation rules.
/// </summary>
public class GenericRuleOptions : IRuleOptions
{
    /// <inheritdoc/>
    [ConfigurationKeyName("enabled")]
    public bool Enabled { get; init; } = true;

    /// <inheritdoc/>
    [ConfigurationKeyName("severity")]
    public ValidationSeverity Severity { get; init; } = ValidationSeverity.Warning;
}