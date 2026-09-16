using BacapGenerator.Configuration.Validation.Rules;
using BacapGenerator.Validation.Models;
using Microsoft.Extensions.Configuration;

namespace BacapGenerator.Configuration.Validation;

/// <summary>
/// Encapsulates all validation rule configurations for a specific datapack.
/// </summary>
public sealed class DatapackValidationSettings
{
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
}