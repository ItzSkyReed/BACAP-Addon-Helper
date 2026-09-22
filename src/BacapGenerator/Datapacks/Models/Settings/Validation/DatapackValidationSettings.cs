using BacapGenerator.Datapacks.Models.Settings.Validation.Rules;
using BacapGenerator.Validation.Models;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;

namespace BacapGenerator.Datapacks.Models.Settings.Validation;

/// <summary>
/// Encapsulates all validation rule configurations for a specific datapack.
/// </summary>
public sealed class DatapackValidationSettings
{
    /// <summary>
    /// Gets the optional name of the validation template to inherit settings from.
    /// </summary>
    [PublicAPI]
    [ConfigurationKeyName("template")]
    public string? Template { get; init; }

    /// <summary>
    /// Gets a value indicating whether validation is active for this datapack.
    /// </summary>
    [ConfigurationKeyName("enabled")]
    public bool Enabled { get; init; } = true;

    /// <summary>
    /// Gets the configuration options for Title Case formatting validation.
    /// </summary>
    [ConfigurationKeyName("title_case_formatting")]
    public TitleFormatRuleOptions TitleCase { get; init; } = new() { Severity = ValidationSeverity.Info };

    /// <summary>
    /// Gets the configuration options for detecting leading or trailing whitespaces.
    /// </summary>
    [ConfigurationKeyName("whitespace_trimming")]
    public GenericRuleOptions WhitespaceTrimming { get; init; } = new() { Severity = ValidationSeverity.Info };

    /// <summary>
    /// Gets the configuration options for enforcing translation keys and prohibiting raw plain text.
    /// </summary>
    [ConfigurationKeyName("plain_text_usage")]
    public PlainTextRuleOptions PlainTextUsage { get; init; } = new() { Severity = ValidationSeverity.Warning };

    /// <summary>
    /// Gets the configuration options for reward functions path validation and file existence.
    /// </summary>
    [ConfigurationKeyName("reward_paths")]
    public GenericRuleOptions RewardPaths { get; init; } = new() { Severity = ValidationSeverity.Error };

    /// <summary>
    /// Gets the configuration options for parent advancement validation (missing or circular references).
    /// </summary>
    [ConfigurationKeyName("parent_references")]
    public GenericRuleOptions ParentReferences { get; init; } = new() { Severity = ValidationSeverity.Error };

    /// <summary>
    /// Validates all configured validation rules within this datapack settings set.
    /// </summary>
    /// <param name="datapackId">The parent datapack identifier.</param>
    /// <exception cref="BacapGenerator.Configuration.Exceptions.DatapackConfigurationException">
    /// Thrown when any underlying rule configuration is invalid.
    /// </exception>
    /// <example>
    /// <code>
    /// validationSettings.Validate("bacaped");
    /// </code>
    /// </example>
    public void Validate(string datapackId)
    {
        // Even if Enabled is false, validate rule declarations to prevent hidden syntax/config bugs
        TitleCase.Validate(datapackId, "title_case_formatting");
        WhitespaceTrimming.Validate(datapackId, "whitespace_trimming");
        PlainTextUsage.Validate(datapackId, "plain_text_usage");
        RewardPaths.Validate(datapackId, "reward_paths");
        ParentReferences.Validate(datapackId, "parent_references");
    }
}