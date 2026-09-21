using System.Globalization;
using System.Text;
using Core.DataComponents.Components;
using Core.Items;
using Core.Registries;
using Core.TextComponents.Components;
using Spectre.Console;
using UI.Styling;

namespace UI.Actions.Common.Components.Wizards;

/// <summary>
/// Wizard for configuring and formatting multiple lines of lore on an item stack (<c>minecraft:lore</c>).
/// Includes smart word-wrapping with Unicode visual cell-width calculation.
/// </summary>
public class LoreWizard : IComponentWizard
{
    private const int DefaultMaxLineWidth = 45;

    /// <inheritdoc/>
    public string ComponentId => LoreComponent.ComponentId;

    /// <inheritdoc/>
    public string DisplayTitle => "Lore Tooltip (minecraft:lore)";

    /// <inheritdoc/>
    public void Execute(ItemStack stack, MinecraftData mcData)
    {
        var existing = stack.Components.Get<LoreComponent>();
        var lines = existing?.Lines.Select(l => (l as PlainTextComponent)?.Text ?? l.ToString()!).ToList() ?? [];

        while (true)
        {
            TuiTheme.RenderHeader("Lore Configuration");

            if (lines.Count == 0)
                AnsiConsole.MarkupLine("[grey]No lore lines configured.[/]\n");
            else
            {
                for (var i = 0; i < lines.Count; i++)
                {
                    var width = MeasureVisualWidth(lines[i]);
                    AnsiConsole.MarkupLine($"[grey]{i + 1,2}:[/] [italic purple]{Markup.Escape(lines[i])}[/] [grey]({width} cols)[/]");
                }
                AnsiConsole.WriteLine();
            }

            var action = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Choose an action:")
                    .AddChoices(
                        $"+ Add Text (Auto-wrap to {DefaultMaxLineWidth} cols)",
                        "+ Add Single Line (Raw)",
                        "- Remove Last Line",
                        "[red]Clear All[/]",
                        "Done (Save)"
                    )
            );

            switch (action)
            {
                case var _ when action.StartsWith("+ Add Text (Auto-wrap", StringComparison.OrdinalIgnoreCase):
                    var rawText = AnsiConsole.Prompt(
                        new TextPrompt<string>("Enter text to wrap (supports long paragraphs):")
                    );

                    var wrapped = WrapText(rawText);
                    lines.AddRange(wrapped);
                    TuiTheme.ShowSuccess($"Added {wrapped.Count} wrapped line(s).");
                    break;

                case "+ Add Single Line (Raw)":
                    var newLine = AnsiConsole.Prompt(new TextPrompt<string>("Enter literal line:"));
                    lines.Add(newLine);
                    break;

                case "- Remove Last Line":
                    if (lines.Count > 0)
                        lines.RemoveAt(lines.Count - 1);
                    break;

                case "[red]Clear All[/]":
                    lines.Clear();
                    break;

                case "Done (Save)":
                    if (lines.Count == 0)
                    {
                        stack.Components.Remove<LoreComponent>();
                        TuiTheme.ShowSuccess("Lore cleared.");
                    }
                    else
                    {
                        stack.Components.Set(new LoreComponent([.. lines]));
                        TuiTheme.ShowSuccess($"Saved {lines.Count} lore lines.");
                    }
                    TuiTheme.WaitForKey();
                    return;
            }
        }
    }

    /// <summary>
    /// Wraps text into multiple lines preserving words and calculating visual cell widths.
    /// </summary>
    /// <param name="text">The raw string to wrap.</param>
    /// <param name="maxWidth">The maximum display width allowed per line.</param>
    /// <returns>A list of cleanly wrapped lines.</returns>
    /// <example>
    /// <code>
    /// var lines = LoreWizard.WrapText("A very long ancient description...", maxWidth: 45);
    /// </code>
    /// </example>
    public static List<string> WrapText(string text, int maxWidth = DefaultMaxLineWidth)
    {
        if (string.IsNullOrEmpty(text))
            return [];

        var result = new List<string>();
        var paragraphs = text.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');

        foreach (var paragraph in paragraphs)
        {
            var trimmed = paragraph.Trim();
            if (string.IsNullOrWhiteSpace(trimmed))
            {
                result.Add(string.Empty);
                continue;
            }

            var words = trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var currentLine = new StringBuilder();
            var currentLineWidth = 0;

            foreach (var word in words)
            {
                var wordWidth = MeasureVisualWidth(word);

                // If a single word exceeds maxWidth, break it into smaller fragments
                if (wordWidth > maxWidth)
                {
                    if (currentLine.Length > 0)
                    {
                        result.Add(currentLine.ToString());
                        currentLine.Clear();
                    }

                    var chunks = BreakLongWord(word, maxWidth);
                    for (var i = 0; i < chunks.Count - 1; i++)
                    {
                        result.Add(chunks[i]);
                    }

                    var tail = chunks[^1];
                    currentLine.Append(tail);
                    currentLineWidth = MeasureVisualWidth(tail);
                    continue;
                }

                var spaceNeeded = currentLine.Length > 0 ? 1 : 0;
                if (currentLineWidth + spaceNeeded + wordWidth <= maxWidth)
                {
                    if (currentLine.Length > 0)
                    {
                        currentLine.Append(' ');
                        currentLineWidth += 1;
                    }

                    currentLine.Append(word);
                    currentLineWidth += wordWidth;
                }
                else
                {
                    result.Add(currentLine.ToString());
                    currentLine.Clear();
                    currentLine.Append(word);
                    currentLineWidth = wordWidth;
                }
            }

            if (currentLine.Length > 0)
            {
                result.Add(currentLine.ToString());
            }
        }

        return result;
    }

    /// <summary>
    /// Calculates the total visual column/cell width of a string.
    /// </summary>
    /// <param name="text">The string to measure.</param>
    /// <returns>The total cell width.</returns>
    private static int MeasureVisualWidth(string text)
    {
        return text.EnumerateRunes().Sum(GetRuneWidth);
    }

    /// <summary>
    /// Forcefully chunks a single word exceeding maximum width into fragments fitting within limits.
    /// </summary>
    /// <param name="word">The word to break.</param>
    /// <param name="maxWidth">Maximum visual width allowed per chunk.</param>
    /// <returns>A list of chunked strings.</returns>
    private static List<string> BreakLongWord(string word, int maxWidth)
    {
        var chunks = new List<string>();
        var currentChunk = new StringBuilder();
        var currentWidth = 0;

        foreach (var rune in word.EnumerateRunes())
        {
            var rw = GetRuneWidth(rune);
            if (currentWidth + rw > maxWidth && currentChunk.Length > 0)
            {
                chunks.Add(currentChunk.ToString());
                currentChunk.Clear();
                currentWidth = 0;
            }

            currentChunk.Append(rune.ToString());
            currentWidth += rw;
        }

        if (currentChunk.Length > 0)
            chunks.Add(currentChunk.ToString());

        return chunks;
    }

    /// <summary>
    /// Evaluates the visual cell width of a single Unicode scalar value.
    /// </summary>
    /// <param name="rune">The Unicode rune to evaluate.</param>
    /// <returns>
    /// <c>0</c> for zero-width/control characters,
    /// <c>2</c> for East Asian Fullwidth/Wide characters and emojis,
    /// otherwise <c>1</c>.
    /// </returns>
    private static int GetRuneWidth(Rune rune)
    {
        if (rune.Value == 0 || Rune.IsControl(rune))
            return 0;

        var category = Rune.GetUnicodeCategory(rune);
        if (category is UnicodeCategory.NonSpacingMark
            or UnicodeCategory.SpacingCombiningMark
            or UnicodeCategory.EnclosingMark
            or UnicodeCategory.Format)
        {
            return 0;
        }

        var value = rune.Value;

        // East Asian Wide / Fullwidth and Emoji ranges
        return value is >= 0x1100 and <= 0x115F or
            0x2329 or 0x232A or
            >= 0x2E80 and <= 0xA4CF or
            >= 0xAC00 and <= 0xD7A3 or
            >= 0xF900 and <= 0xFAFF or
            >= 0xFE10 and <= 0xFE19 or
            >= 0xFE30 and <= 0xFE6F or
            >= 0xFF00 and <= 0xFF60 or
            >= 0xFFE0 and <= 0xFFE6 or
            >= 0x1F300 and <= 0x1F64F or
            >= 0x1F680 and <= 0x1F6FF or
            >= 0x1F900 and <= 0x1F9FF or
            >= 0x20000 and <= 0x3FFFD ? 2 : 1;
    }
}