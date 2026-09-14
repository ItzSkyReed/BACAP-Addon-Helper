using BacapGenerator.Validation.Interfaces;
using BacapGenerator.Models.Advancements;
using BacapGenerator.Validation.Context;
using BacapGenerator.Validation.Extensions;
using BacapGenerator.Validation.Models;

namespace BacapGenerator.Validation.Rules;


/// <summary>
/// Ensures advancement titles and descriptions do not contain leading or trailing whitespace.
/// </summary>
public sealed class WhitespaceTrimmingTitlesRule : ISingleAdvancementRule
{
    public string RuleId => "WHITESPACE_TRIMMING_TITLES";
    public string DisplayName => "Whitespace Trimming";
    public ValidationSeverity Severity { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="WhitespaceTrimmingRule"/> class.
    /// </summary>
    /// <param name="severity">The configured diagnostic severity.</param>
    public WhitespaceTrimmingTitlesRule(ValidationSeverity severity = ValidationSeverity.Warning)
    {
        Severity = severity;
    }

    /// <inheritdoc/>
    public void Validate(ManagedAdvancement managedAdvancement, ValidationContext context)
    {
        if (managedAdvancement is not BacapAdvancement bacapAdvancement)
            return;


        var title = bacapAdvancement.TitleText;

        if (!string.IsNullOrEmpty(title) && title.Length != title.Trim().Length)
        {
            this.ReportIssue(
                context,
                "Title contains leading or trailing whitespace.",
                managedAdvancement,
                "Display.Title");
        }
    }
}