
using BacapGenerator.Configuration.Validation.Rules;
using BacapGenerator.Models.Advancements;
using BacapGenerator.Utils;
using BacapGenerator.Validation.Context;
using BacapGenerator.Validation.Extensions;
using BacapGenerator.Validation.Interfaces;
using BacapGenerator.Validation.Models;

namespace BacapGenerator.Validation.Rules;

/// <summary>
/// Validates that advancement titles follow Title Case formatting conventions,
/// respecting minor word exceptions.
/// </summary>
public sealed class TitleFormatTitlesRule : ISingleAdvancementRule
{
    public string RuleId => "TITLE_FORMAT_TITLES";
    public string DisplayName => "Title Case Formatting";
    public ValidationSeverity Severity => _options.Severity;

    private readonly TitleFormatRuleOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="TitleFormatTitlesRule"/> class.
    /// </summary>
    /// <param name="options">The configuration options containing severity and custom minor words.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is null.</exception>
    public TitleFormatTitlesRule(TitleFormatRuleOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _options = options;
    }

    /// <inheritdoc/>
    public void Validate(ManagedAdvancement managedAdvancement, ValidationContext context)
    {
        if (managedAdvancement is not BacapAdvancement bacapAdvancement)
            return;

        var title = bacapAdvancement.TitleText;
        if (string.IsNullOrWhiteSpace(title))
            return;

        var expectedTitle = title.ToTitleCase(_options.MinorWordsSet, preserveContractions: true);

        if (!string.Equals(title, expectedTitle, StringComparison.Ordinal))
        {
            this.ReportIssue(
                context,
                $"Title '{title}' does not match Title Case conventions. Expected: '{expectedTitle}'.",
                managedAdvancement,
                "Display.Title");
        }
    }
}