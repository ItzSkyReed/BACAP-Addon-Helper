using System.Buffers;
using System.Text;
using Core.SNBT.Interfaces;

namespace Core.SNBT.Nodes;

/// <summary>
/// Represents an SNBT string value node enclosed in double quotes with escape sequences.
/// </summary>
public sealed record SnbtString : ISnbtNode
{
    private static readonly SearchValues<char> EscapableChars =
        SearchValues.Create('\\', '"', '\b', '\f', '\n', '\r', '\t');

    /// <summary>
    /// Gets the unescaped underlying string value.
    /// </summary>
    public string Value { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SnbtString"/> record.
    /// </summary>
    /// <param name="value">The string value to encapsulate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public SnbtString(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        Value = value;
    }

    /// <summary>
    /// Converts the string node into an SNBT-compatible double-quoted string with escaped characters.
    /// </summary>
    /// <param name="pretty">Indicates whether formatting with indents is enabled.</param>
    /// <param name="indent">The indentation prefix for the current line.</param>
    /// <returns>A double-quoted string containing escaped characters where necessary.</returns>
    /// <example>
    /// <code>
    /// var node = new SnbtString("Hello \"World\"!");
    /// string snbt = node.ToSnbtString(); // returns "\"Hello \\\"World\\\"!\""
    /// </code>
    /// </example>
    public string ToSnbtString(bool pretty = false, string indent = "")
    {
        if (Value.Length == 0)
            return "\"\"";

        if (!Value.AsSpan().ContainsAny(EscapableChars))
            return $"\"{Value}\"";

        var sb = new StringBuilder(Value.Length + 16);
        sb.Append('"');

        foreach (var c in Value)
        {
            switch (c)
            {
                case '\\':
                    sb.Append(@"\\");
                    break;
                case '"':
                    sb.Append("\\\"");
                    break;
                case '\b':
                    sb.Append("\\b");
                    break;
                case '\f':
                    sb.Append("\\f");
                    break;
                case '\n':
                    sb.Append("\\n");
                    break;
                case '\r':
                    sb.Append("\\r");
                    break;
                case '\t':
                    sb.Append("\\t");
                    break;
                default:
                    sb.Append(c);
                    break;
            }
        }

        sb.Append('"');
        return sb.ToString();
    }
}