using System.Text;
using Core.McFunctions.Models.Interfaces;
using JetBrains.Annotations;

namespace Core.McFunctions.Models;

/// <summary>
/// Represents a parsed Minecraft function (.mcfunction file).
/// </summary>
/// <param name="Lines">The ordered collection of lines (commands, comments, empty) in the function.</param>
public record McFunction(List<IMcFunctionLine> Lines)
{
/// <summary>
    /// Serializes the entire function into a string ready to be written to an .mcfunction file.
    /// </summary>
    /// <param name="maxLineLength">
    /// The maximum allowed length for a single line. If a command exceeds this limit,
    /// it is wrapped using the line continuation character ('\').
    /// If null or less than or equal to 0, line wrapping is disabled.
    /// </param>
    /// <returns>A formatted .mcfunction string.</returns>
    /// <example>
    /// <code>
    /// var mcFunction = new McFunction(lines);
    /// string content = mcFunction.Build(120); // Wraps commands longer than 120 characters
    /// </code>
    /// </example>
    [PublicAPI]
    public string Build(int? maxLineLength = 65536)
    {
        var sb = new StringBuilder();

        foreach (var line in Lines)
        {
            var builtLine = line.Build();

            // Skip wrapping if disabled, line is short enough, or if it's a comment
            // (Minecraft does not support line continuations for comments)
            if (maxLineLength is null or <= 0 || builtLine.Length <= maxLineLength || builtLine.StartsWith('#'))
            {
                sb.AppendLine(builtLine);
                continue;
            }

            // Use Span to slice the string without allocating new string objects for each chunk
            var span = builtLine.AsSpan();

            while (span.Length > 0)
            {
                if (span.Length <= maxLineLength.Value)
                {
                    sb.AppendLine(span.ToString());
                    break;
                }

                // Extract the chunk of max length, append it with '\', and move to the next line
                var chunk = span[..maxLineLength.Value];
                sb.Append(chunk).AppendLine(@"\");

                // Slice the remaining part of the span
                span = span[maxLineLength.Value..];
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Parses the raw text of an .mcfunction file.
    /// </summary>
    [PublicAPI]
    public static McFunction Parse(string rawText)
    {
        return McFunctionParser.Parse(rawText);
    }
}