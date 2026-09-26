using System.Globalization;
using Core.SNBT.Interfaces;

namespace Core.SNBT.Nodes;

/// <summary>
/// Represents an SNBT signed 8-bit integer (byte) node.
/// </summary>
/// <param name="Value">The signed 8-bit integer value ranging from -128 to 127.</param>
public sealed record SnbtByte(sbyte Value) : ISnbtNode
{
    /// <summary>
    /// Converts the byte node into its SNBT string representation with the 'b' suffix.
    /// </summary>
    /// <param name="pretty">Indicates whether formatting with indents is enabled.</param>
    /// <param name="indent">The indentation prefix for the current line.</param>
    /// <returns>A string formatted as &lt;value&gt;b using invariant culture.</returns>
    /// <example>
    /// <code>
    /// var node = new SnbtByte(-16);
    /// string snbt = node.ToSnbtString(); // returns "-16b"
    /// </code>
    /// </example>
    public string ToSnbtString(bool pretty = false, string indent = "") =>
        string.Create(CultureInfo.InvariantCulture, $"{Value}b");
}