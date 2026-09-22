using BacapGenerator.Configuration.Exceptions;
using BacapGenerator.Validation.Models;
using Microsoft.Extensions.Configuration;

namespace BacapGenerator.Datapacks.Models.Settings.Validation.Rules;

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

    /// <summary>
    /// Validates common rule parameters such as defined severity levels.
    /// </summary>
    /// <param name="datapackId">The parent datapack identifier.</param>
    /// <param name="ruleName">The display name or configuration key of the rule.</param>
    /// <exception cref="DatapackConfigurationException">Thrown when the rule severity is undefined or out of range.</exception>
    /// <example>
    /// <code>
    /// options.Validate("bacaped", "reward_paths");
    /// </code>
    /// </example>
    public virtual void Validate(string datapackId, string ruleName)
    {
        if (!Enum.IsDefined(Severity))
        {
            throw new DatapackConfigurationException(
                datapackId,
                DatapackErrorKind.InvalidValidationRuleConfiguration,
                $"Rule '{ruleName}' in datapack '{datapackId}' has an invalid or undefined severity level: {(int)Severity}.",
                nameof(Severity));
        }
    }
}