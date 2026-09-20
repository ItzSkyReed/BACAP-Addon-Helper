using Core.DataComponents.Interfaces;
using Core.SNBT;
using Core.SNBT.Interfaces;
using Core.SNBT.Nodes;
using JetBrains.Annotations;

namespace Core.DataComponents.Components;

/// <summary>
/// Marks a block's contents or text as waxed, preventing further modification in-place (<c>minecraft:waxed</c>).
/// Applies to blocks without dedicated waxed variants (such as signs and hanging signs).
/// </summary>
[UsedImplicitly]
public record WaxedComponent : ICompoundComponent<WaxedComponent>
{
    /// <summary>
    /// Shared singleton instance of <see cref="WaxedComponent"/>.
    /// </summary>
    [PublicAPI]
    public static WaxedComponent Instance { get; } = new();

    /// <inheritdoc/>
    public static string ComponentId => "minecraft:waxed";

    /// <summary>
    /// Parses a <see cref="WaxedComponent"/> from an SNBT compound node representation.
    /// </summary>
    /// <param name="compound">The SNBT compound node.</param>
    /// <returns>The shared <see cref="WaxedComponent"/> instance.</returns>
    /// <example>
    /// <code>
    /// var node = SnbtParser.Parse("{}");
    /// var waxed = WaxedComponent.Parse(node);
    /// </code>
    /// </example>
    [PublicAPI]
    public static WaxedComponent Parse(SnbtCompound compound) => Instance;

    /// <summary>
    /// Serializes the marker component into an empty SNBT compound node.
    /// </summary>
    /// <returns>An <see cref="ISnbtNode"/> representing the empty compound tag.</returns>
    public ISnbtNode ToSnbt() => Snbt.Compound().Build();
}