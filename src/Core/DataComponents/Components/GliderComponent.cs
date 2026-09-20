using Core.DataComponents.Interfaces;
using Core.SNBT;
using Core.SNBT.Interfaces;
using Core.SNBT.Nodes;
using JetBrains.Annotations;

namespace Core.DataComponents.Components;

/// <summary>
/// Marks an item as a glider, enabling living entities to glide through the air when equipped in the chest slot (<c>minecraft:glider</c>).
/// </summary>
[UsedImplicitly]
public record GliderComponent : ICompoundComponent<GliderComponent>
{
    /// <inheritdoc/>
    public static string ComponentId => "minecraft:glider";

    /// <summary>
    /// Parses a <see cref="GliderComponent"/> directly from an SNBT compound node.
    /// </summary>
    /// <param name="compound">The SNBT compound node representing the glider tag.</param>
    /// <returns>A new <see cref="GliderComponent"/> instance.</returns>
    /// <example>
    /// <code>
    /// var node = SnbtParser.Parse("{}");
    /// var component = GliderComponent.Parse((SnbtCompound)node);
    /// </code>
    /// </example>
    public static GliderComponent Parse(SnbtCompound compound) => new();

    /// <summary>
    /// Serializes the glider marker component into an empty SNBT compound node.
    /// </summary>
    /// <returns>An <see cref="ISnbtNode"/> representing an empty compound.</returns>
    public ISnbtNode ToSnbt() => Snbt.Compound().Build();
}