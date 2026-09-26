using System.Globalization;
using Core.SNBT.Interfaces;

namespace Core.SNBT.Nodes;

/// <summary>
/// Represents an SNBT 64-bit double-precision floating-point node with the 'd' suffix.
/// </summary>
/// <param name="Value">The double-precision floating-point value.</param>
public sealed record SnbtDouble(double Value) : ISnbtNode
{
    /// <summary>
    /// Converts the double node into its SNBT string representation with the 'd' suffix.
    /// </summary>
    /// <param name="pretty">Indicates whether formatting with indents is enabled.</param>
    /// <param name="indent">The indentation prefix for the current line.</param>
    /// <returns>A string formatted as &lt;number&gt;d using invariant culture.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when <see cref="Value"/> is NaN or Infinity, which cannot be represented in valid SNBT syntax.
    /// </exception>
    /// <example>
    /// <code>
    /// var node = new SnbtDouble(3.1415926);
    /// string snbt = node.ToSnbtString(); // returns "3.1415926d"
    /// </code>
    /// </example>
    public string ToSnbtString(bool pretty = false, string indent = "")
    {
        return !double.IsFinite(Value)
            ? throw new InvalidOperationException($"Cannot serialize non-finite double value '{Value}' to SNBT.")
            : string.Create(CultureInfo.InvariantCulture, $"{Value}d");
    }
}