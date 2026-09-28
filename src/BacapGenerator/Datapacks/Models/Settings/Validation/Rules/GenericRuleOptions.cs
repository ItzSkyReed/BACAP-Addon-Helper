using BacapGenerator.Configuration;
using BacapGenerator.Configuration.Exceptions;
using BacapGenerator.Validation.Models;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;

namespace BacapGenerator.Datapacks.Models.Settings.Validation.Rules;

/// <summary>
/// Represents generic configuration for simple boolean/severity-based validation rules.
/// </summary>
public class GenericRuleOptions : IRuleOptions
{
    /// <summary>
    /// Gets the raw string representation indicating whether the rule is enabled.
    /// </summary>
    [PublicAPI]
    [ConfigurationKeyName("enabled")]
    public string? RawEnabled { get; init; }

    /// <summary>
    /// Gets the raw string representation of the rule severity level.
    /// </summary>
    [PublicAPI]
    [ConfigurationKeyName("severity")]
    public string? RawSeverity { get; init; }

    /// <inheritdoc/>
    [PublicAPI]
    public bool IsEnabled { get; set; } = true;

    /// <inheritdoc/>
    [PublicAPI]
    public ValidationSeverity SeverityLevel { get; set; } = ValidationSeverity.Warning;

    /// <summary>
    /// Validates and parses raw rule configuration options such as rule enablement and severity level.
    /// </summary>
    /// <param name="datapackId">The parent datapack identifier.</param>
    /// <param name="ruleName">The display name or configuration key of the rule.</param>
    /// <exception cref="DatapackConfigurationException">
    /// Thrown when <see cref="RawEnabled"/> is not a valid boolean, <see cref="RawSeverity"/> cannot be parsed into a defined <see cref="ValidationSeverity"/>, or the resulting severity level is undefined.
    /// </exception>
    /// <example>
    /// <code>
    /// var options = new GenericRuleOptions
    /// {
    ///     RawEnabled = "true",
    ///     RawSeverity = "error"
    /// };
    /// options.Validate("bacaped", "reward_paths");
    /// </code>
    /// </example>
    public virtual void Validate(string datapackId, string ruleName)
    {
        IsEnabled = ConfigParser.ParseBool(
            RawEnabled,
            defaultValue: IsEnabled,
            datapackId,
            DatapackErrorKind.InvalidValidationRuleConfiguration,
            $"Invalid boolean value '{RawEnabled}' for 'enabled' in rule '{ruleName}' of datapack '{datapackId}'.");

        SeverityLevel = ConfigParser.ParseOptionalEnum(
            RawSeverity,
            defaultValue: SeverityLevel,
            datapackId,
            DatapackErrorKind.InvalidValidationRuleConfiguration,
            $"Invalid severity level '{RawSeverity}' in rule '{ruleName}' of datapack '{datapackId}'. " +
            $"Allowed values: {string.Join(", ", Enum.GetNames<ValidationSeverity>())}.");
    }
}