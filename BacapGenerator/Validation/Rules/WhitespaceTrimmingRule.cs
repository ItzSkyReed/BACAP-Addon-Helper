using BacapGenerator.Configuration.Validation.Rules;
using BacapGenerator.Models.Advancements;
using BacapGenerator.Validation.Extensions;
using BacapGenerator.Validation.Interfaces;
using BacapGenerator.Validation.Models;

namespace BacapGenerator.Validation.Rules;

/// <summary>
/// Validates that advancement titles do not contain leading or trailing whitespace characters.
/// </summary>
public sealed class WhitespaceTrimmingRule : ISingleAdvancementRule
{
    /// <summary>
    /// Gets the configuration options controlling this rule.
    /// </summary>
    public GenericRuleOptions RuleOptions { get; }

    /// <inheritdoc/>
    public string RuleId => "WHITESPACE_TRIMMING_TITLES";

    /// <inheritdoc/>
    public string DisplayName => "Title Whitespace Trimming";

    /// <inheritdoc/>
    public ValidationSeverity Severity => RuleOptions.Severity;

    /// <summary>
    /// Initializes a new instance of the <see cref="WhitespaceTrimmingRule"/> class.
    /// </summary>
    /// <param name="ruleOptions">The configuration options specifying rule status and severity.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="ruleOptions"/> is <see langword="null"/>.</exception>
    public WhitespaceTrimmingRule(GenericRuleOptions ruleOptions)
    {
        ArgumentNullException.ThrowIfNull(ruleOptions);
        RuleOptions = ruleOptions;
    }

    /// <summary>
    /// Evaluates the advancement title for leading or trailing whitespace.
    /// </summary>
    /// <param name="managedAdvancement">The advancement to evaluate.</param>
    /// <param name="context">The shared validation execution context where issues are reported.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is <see langword="null"/>.</exception>
    public void Validate(ManagedAdvancement managedAdvancement, ValidationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (managedAdvancement is not BacapAdvancement bacapAdvancement)
            return;

        var title = bacapAdvancement.TitleText;
        if (string.IsNullOrEmpty(title))
            return;

        if (char.IsWhiteSpace(title[0]) || char.IsWhiteSpace(title[^1]))
        {
            this.ReportIssue(
                context,
                $"Title '{title}' contains leading or trailing whitespace.",
                managedAdvancement,
                "Display.Title");
        }
    }
}