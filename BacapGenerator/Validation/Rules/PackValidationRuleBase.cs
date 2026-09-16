using BacapGenerator.Datapacks.Models.Settings.Validation.Rules;
using BacapGenerator.Validation.Interfaces;
using BacapGenerator.Validation.Models;

namespace BacapGenerator.Validation.Rules;

/// <summary>
/// Base class for pack-wide validation rules configured with strongly-typed options.
/// </summary>
/// <typeparam name="TOptions">The type of options configuring this rule.</typeparam>
/// <param name="options">The configuration options instance.</param>
public abstract class PackValidationRuleBase<TOptions>(TOptions options) : IPackValidationRule
    where TOptions : IRuleOptions
{
    /// <summary>
    /// Gets the options configuring this rule.
    /// </summary>
    protected TOptions Options { get; } = options ?? throw new ArgumentNullException(nameof(options));

    /// <inheritdoc/>
    public abstract string RuleId { get; }

    /// <inheritdoc/>
    public abstract string DisplayName { get; }

    /// <inheritdoc/>
    public ValidationSeverity Severity => Options.Severity;

    /// <inheritdoc/>
    public abstract void Validate(ValidationContext context);
}