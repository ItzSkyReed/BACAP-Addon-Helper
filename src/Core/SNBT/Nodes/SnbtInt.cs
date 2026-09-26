using System.Globalization;
using Core.SNBT.Interfaces;

namespace Core.SNBT.Nodes;

/// <summary>
/// Represents an SNBT signed 32-bit integer node.
/// </summary>
/// <param name="Value">The signed 32-bit integer value ranging from -2,147,483,648 to 2,147,483,647.</param>
public sealed record SnbtInt(int Value) : ISnbtNode
{
    /// <summary>
    /// Converts the int node into its canonical SNBT string representation without a type suffix.
    /// </summary>
    /// <param name="pretty">Indicates whether formatting with indents is enabled.</param>
    /// <param name="indent">The indentation prefix for the current line.</param>
    /// <returns>A string containing the invariant numeric value.</returns>
    /// <example>
    /// <code>
    /// var node = new SnbtInt(31415926);
    /// string snbt = node.ToSnbtString(); // returns "31415926"
    /// </code>
    /// </example>
    public string ToSnbtString(bool pretty = false, string indent = "") =>
        Value.ToString(CultureInfo.InvariantCulture);
}