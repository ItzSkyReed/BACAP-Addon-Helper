using Core.SNBT.Interfaces;
using Core.SNBT.Nodes;

namespace Core.DataComponents.Components.Base;

/// <summary>
/// Base record providing common value storage and SNBT string serialization for single-string components.
/// </summary>
/// <param name="Value">The underlying string value.</param>
public abstract record StringComponentBase(string Value) : ISnbtSerializable
{
    /// <summary>
    /// Serializes the component into an SNBT string node.
    /// </summary>
    /// <returns>An <see cref="ISnbtNode"/> containing the string value.</returns>
    public ISnbtNode ToSnbt() => new SnbtString(Value);

    /// <inheritdoc/>
    public override string ToString() => Value;
}