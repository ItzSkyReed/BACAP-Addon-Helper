using BacapGenerator.Configuration.Validation.Rules;
using BacapGenerator.Models.Advancements;
using BacapGenerator.Utils;
using BacapGenerator.Validation.Extensions;
using BacapGenerator.Validation.Models;


namespace BacapGenerator.Validation.Rules;

/// <summary>
/// Validates that advancement titles adhere to Title Case formatting conventions.
/// </summary>
/// <param name="options">The configuration options controlling formatting rules and exact exceptions.</param>
public sealed class TitleCaseRule(TitleFormatRuleOptions options)
    : SingleAdvancementRuleBase<TitleFormatRuleOptions>(options)
{
    /// <inheritdoc/>
    public override string RuleId => "TITLE_FORMAT_TITLES";

    /// <inheritdoc/>
    public override string DisplayName => "Title Case Formatting";

    /// <inheritdoc/>
    public override void Validate(ManagedAdvancement advancement, ValidationContext context)
    {
        if (advancement is not BacapAdvancement bacapAdvancement)
            return;

        var title = bacapAdvancement.TitleText;
        if (string.IsNullOrWhiteSpace(title))
            return;

        // Skip validation entirely if the exact title phrase is explicitly allowed in configuration
        if (Options.AllowedTitlesSet.Contains(title))
            return;

        var expectedTitle = title.ToTitleCase(Options.MinorWordsSet, preserveContractions: true);

        if (!string.Equals(title, expectedTitle, StringComparison.Ordinal))
        {
            this.ReportIssue(
                context,
                $"Title '{title}' does not match Title Case formatting. Expected: '{expectedTitle}'.",
                advancement,
                "Display.Title");
        }
    }
}