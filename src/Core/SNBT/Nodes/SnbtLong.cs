using System.Globalization;
using Core.SNBT.Interfaces;

namespace Core.SNBT.Nodes;

/// <summary>
/// Represents an SNBT signed 64-bit integer (long) node with the 'L' suffix.
/// </summary>
/// <param name="Value">The signed 64-bit integer value ranging from -9,223,372,036,854,775,808 to 9,223,372,036,854,775,807.</param>
public sealed record SnbtLong(long Value) : ISnbtNode
{
    /// <summary>
    /// Converts the long node into its SNBT string representation with the uppercase 'L' suffix.
    /// </summary>
    /// <param name="pretty">Indicates whether formatting with indents is enabled.</param>
    /// <param name="indent">The indentation prefix for the current line.</param>
    /// <returns>A string formatted as &lt;value&gt;L using invariant culture.</returns>
    /// <example>
    /// <code>
    /// var node = new SnbtLong(31415926L);
    /// string snbt = node.ToSnbtString(); // returns "31415926L"
    /// </code>
    /// </example>
    public string ToSnbtString(bool pretty = false, string indent = "") =>
        string.Create(CultureInfo.InvariantCulture, $"{Value}L");
}