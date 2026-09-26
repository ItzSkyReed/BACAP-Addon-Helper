using Core.SNBT.Interfaces;

namespace Core.SNBT.Nodes;

/// <summary>
/// Represents an SNBT boolean node, which serializes to "true" or "false".
/// </summary>
/// <param name="Value">The underlying boolean value.</param>
public sealed record SnbtBool(bool Value) : ISnbtNode
{
    /// <summary>
    /// Converts the boolean node into its SNBT string representation.
    /// </summary>
    /// <param name="pretty">Indicates whether formatting with indents is enabled.</param>
    /// <param name="indent">The indentation prefix for the current line.</param>
    /// <returns>A lowercase string representation: "true" or "false".</returns>
    /// <example>
    /// <code>
    /// var node = new SnbtBool(true);
    /// string snbt = node.ToSnbtString(); // returns "true"
    /// </code>
    /// </example>
    public string ToSnbtString(bool pretty = false, string indent = "") => Value ? "true" : "false";
}