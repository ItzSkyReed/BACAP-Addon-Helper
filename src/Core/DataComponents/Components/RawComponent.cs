using Core.DataComponents.Interfaces;
using Core.SNBT.Interfaces;
using JetBrains.Annotations;

namespace Core.DataComponents.Components;

/// <summary>
/// Represents an unregistered, modded, or raw data component preserved as an arbitrary SNBT node.
/// Serves as a lossless fallback for unrecognized or dynamically parsed components.
/// </summary>
/// <param name="Id">The namespaced resource identifier of the component (e.g. <c>custom_mod:energy_storage</c>).</param>
/// <param name="Node">The raw SNBT node payload representing the component data.</param>
[PublicAPI]
public record RawComponent(
    string Id,
    ISnbtNode Node
) : IDataComponent
{
    /// <summary>
    /// Serializes the raw component by returning its underlying SNBT node.
    /// </summary>
    /// <returns>The preserved <see cref="ISnbtNode"/>.</returns>
    public ISnbtNode ToSnbt() => Node;

    /// <inheritdoc/>
    public override string ToString() => $"!{Id}={Node.ToSnbtString()}";
}