using System.Globalization;
using Core.SNBT.Interfaces;

namespace Core.SNBT.Nodes;

/// <summary>
/// Represents an SNBT 32-bit single-precision floating-point node with the 'f' suffix.
/// </summary>
/// <param name="Value">The single-precision floating-point value.</param>
public sealed record SnbtFloat(float Value) : ISnbtNode
{
    /// <summary>
    /// Converts the float node into its SNBT string representation with the 'f' suffix.
    /// </summary>
    /// <param name="pretty">Indicates whether formatting with indents is enabled.</param>
    /// <param name="indent">The indentation prefix for the current line.</param>
    /// <returns>A string formatted as &lt;number&gt;f using invariant culture.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when <see cref="Value"/> is NaN or Infinity, which cannot be represented in valid SNBT syntax.
    /// </exception>
    /// <example>
    /// <code>
    /// var node = new SnbtFloat(3.14f);
    /// string snbt = node.ToSnbtString(); // returns "3.14f"
    /// </code>
    /// </example>
    public string ToSnbtString(bool pretty = false, string indent = "")
    {
        return !float.IsFinite(Value)
            ? throw new InvalidOperationException($"Cannot serialize non-finite float value '{Value}' to SNBT.")
            : string.Create(CultureInfo.InvariantCulture, $"{Value}f");
    }
}