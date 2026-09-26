using System.Globalization;
using Core.SNBT.Interfaces;

namespace Core.SNBT.Nodes;

/// <summary>
/// Represents an SNBT signed 16-bit integer (short) node.
/// </summary>
/// <param name="Value">The signed 16-bit integer value ranging from -32,768 to 32,767.</param>
public sealed record SnbtShort(short Value) : ISnbtNode
{
    /// <summary>
    /// Converts the short node into its SNBT string representation with the 's' suffix.
    /// </summary>
    /// <param name="pretty">Indicates whether formatting with indents is enabled.</param>
    /// <param name="indent">The indentation prefix for the current line.</param>
    /// <returns>A string formatted as &lt;value&gt;s using invariant culture.</returns>
    /// <example>
    /// <code>
    /// var node = new SnbtShort(15);
    /// string snbt = node.ToSnbtString(); // returns "15s"
    /// </code>
    /// </example>
    public string ToSnbtString(bool pretty = false, string indent = "") =>
        string.Create(CultureInfo.InvariantCulture, $"{Value}s");
}