using System.Text;
using Core.TextComponents.Components;
using JetBrains.Annotations;

namespace Core.TextComponents.Extensions;

/// <summary>
/// Provides extension methods for flattening and extracting plain text from <see cref="TextComponent"/> trees.
/// </summary>
public static class TextComponentExtensions
{
    /// <summary>
    /// Recursively extracts and concatenates all text fragments, translation keys,
    /// and extra/with children into a single flat string.
    /// </summary>
    /// <param name="component">The root text component.</param>
    /// <returns>A concatenated plain text representation of the entire component tree.</returns>
    [PublicAPI]
    public static string ExtractPlainText(this TextComponent? component)
    {
        if (component is null)
            return string.Empty;

        var sb = new StringBuilder();
        ExtractRecursive(component, sb);
        return sb.ToString();
    }

    private static void ExtractRecursive(TextComponent component, StringBuilder sb)
    {
        switch (component)
        {
            case PlainTextComponent plain:
                if (!string.IsNullOrEmpty(plain.Text))
                    sb.Append(plain.Text);
                break;

            case TranslatableComponent trans:
                if (!string.IsNullOrEmpty(trans.Translate))
                    sb.Append(trans.Translate);

                if (trans.With is { Count: > 0 })
                {
                    foreach (var withItem in trans.With)
                    {
                        ExtractRecursive(withItem, sb);
                    }
                }

                break;
        }

        // Every TextComponent can have an Extra trailing list
        if (component.Extra is not { Count: > 0 })
            return;

        foreach (var extraItem in component.Extra)
        {
            ExtractRecursive(extraItem, sb);
        }
    }

    /// <summary>
    /// Recursively extracts plain text up to the first double newline sequence (`\n\n`),
    /// stopping traversal immediately once found.
    /// </summary>
    /// <param name="component">The root text component.</param>
    /// <returns>The text preceding the first double newline.</returns>
    [PublicAPI]
    public static string ExtractFirstParagraph(this TextComponent? component)
    {
        if (component is null)
            return string.Empty;

        var sb = new StringBuilder();
        ExtractFirstParagraphRecursive(component, sb);
        return sb.ToString().Trim();
    }

    private static bool ExtractFirstParagraphRecursive(TextComponent component, StringBuilder sb)
    {
        switch (component)
        {
            case PlainTextComponent plain:
                if (!string.IsNullOrEmpty(plain.Text))
                {
                    var index = plain.Text.IndexOf("\n\n", StringComparison.Ordinal);
                    if (index >= 0)
                    {
                        sb.Append(plain.Text[..index]);
                        return true; // Stop traversal at double newline
                    }

                    sb.Append(plain.Text);
                }
                break;

            case TranslatableComponent trans:
                if (!string.IsNullOrEmpty(trans.Translate))
                {
                    var index = trans.Translate.IndexOf("\n\n", StringComparison.Ordinal);
                    if (index >= 0)
                    {
                        sb.Append(trans.Translate[..index]);
                        return true;
                    }

                    sb.Append(trans.Translate);
                }

                if (trans.With is { Count: > 0 })
                {
                    foreach (var withItem in trans.With)
                    {
                        if (ExtractFirstParagraphRecursive(withItem, sb))
                            return true;
                    }
                }
                break;
        }

        if (component.Extra is not { Count: > 0 })
            return false;

        foreach (var extraItem in component.Extra)
        {
            if (ExtractFirstParagraphRecursive(extraItem, sb))
                return true;
        }

        return false;
    }
}