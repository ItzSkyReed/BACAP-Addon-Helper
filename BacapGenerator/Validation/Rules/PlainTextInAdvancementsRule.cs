using BacapGenerator.Advancements.Models;
using BacapGenerator.Datapacks.Models.Settings.Validation.Rules;
using BacapGenerator.Validation.Extensions;
using BacapGenerator.Validation.Models;
using Core.TextComponents.Components;

namespace BacapGenerator.Validation.Rules;

/// <summary>
/// Validates that advancement titles and descriptions use translatable components instead of plain text literals,
/// while ignoring symbols, digits, and whitelisted tokens.
/// </summary>
/// <param name="options">The configuration options specifying allowed strings and diagnostic severity.</param>
/// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is <see langword="null"/>.</exception>
public sealed class PlainTextInAdvancementsRule(PlainTextRuleOptions options)
    : SingleAdvancementRuleBase<PlainTextRuleOptions>(options)
{
    /// <inheritdoc/>
    public override string RuleId => "PLAIN_TEXT_IN_ADVANCEMENTS";

    /// <inheritdoc/>
    public override string DisplayName => "Translation Key Enforcement";

    /// <inheritdoc/>
    public override void Validate(ManagedAdvancement advancement, ValidationContext context)
    {
        ArgumentNullException.ThrowIfNull(advancement);
        ArgumentNullException.ThrowIfNull(context);

        if (advancement is not BacapAdvancement bacapAdvancement)
            return;

        if (bacapAdvancement.TitleComponent is { } titleComponent)
            InspectComponentTree(titleComponent, advancement, "Display.Title", context);

        if (bacapAdvancement.DescriptionComponent is { } descriptionComponent)
            InspectComponentTree(descriptionComponent, advancement, "Display.Description", context);
    }

    /// <summary>
    /// Recursively traverses a component tree, reporting any plain text literals that violate localization rules.
    /// </summary>
    private void InspectComponentTree(
        TextComponent component,
        ManagedAdvancement advancement,
        string propertyPath,
        ValidationContext context)
    {
        switch (component)
        {
            case PlainTextComponent plainText:
            {
                if (IsDisallowedPlainText(plainText.Text))
                {
                    // Escape newlines and returns so they don't break the TUI console layout
                    var displayString = plainText.Text
                        .Replace("\n", "\\n")
                        .Replace("\r", "\\r")
                        .Replace("\t", "\\t");

                    this.ReportIssue(
                        context,
                        $"Literal plain text '{displayString}' is used in '{propertyPath}' instead of a translation component.",
                        advancement,
                        propertyPath);
                }

                break;
            }

            case TranslatableComponent translatable:
            {
                if (translatable.With is { Count: > 0 })
                {
                    foreach (var withComponent in translatable.With)
                    {
                        InspectComponentTree(withComponent, advancement, propertyPath, context);
                    }
                }

                break;
            }
        }

        if (component.Extra is not { Count: > 0 })
            return;

        foreach (var extraComponent in component.Extra)
        {
            InspectComponentTree(extraComponent, advancement, propertyPath, context);
        }
    }

    /// <summary>
    /// Evaluates whether a raw text literal constitutes a validation failure.
    /// </summary>
    private bool IsDisallowedPlainText(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return false;

        // Remove leading/trailing whitespaces, including \n, \r, and \t
        var cleanText = text.Trim();

        // Skip tokens that contain no alphabetic letters (e.g., "[1/5]", "-", "->", "100%")
        if (!ContainsAnyLetter(cleanText.AsSpan()))
            return false;

        // 3. Skip if explicitly permitted by configuration
        return !Options.AllowedStringsSet.Contains(cleanText);
    }

    /// <summary>
    /// Checks whether the character span contains at least one alphabetic letter without allocating.
    /// </summary>
    private static bool ContainsAnyLetter(ReadOnlySpan<char> span)
    {
        foreach (var c in span)
        {
            if (char.IsLetter(c))
                return true;
        }

        return false;
    }
}