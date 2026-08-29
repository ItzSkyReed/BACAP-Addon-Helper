using Core.SNBT;
using Core.SNBT.Interfaces;
using Core.SNBT.Nodes;
using JetBrains.Annotations;

using Core.DataComponents.Interfaces;

namespace Core.DataComponents.Components;

/// <summary>
/// Represents the intangible projectile property of an item (<c>minecraft:intangible_projectile</c>).
/// When present on projectile items (such as arrows and tridents), the projectile cannot be picked up after landing by players in Survival or Adventure mode.
/// </summary>
[UsedImplicitly]
public record IntangibleProjectileComponent : ICompoundComponent<IntangibleProjectileComponent>
{
    /// <inheritdoc/>
    public static string ComponentId => "minecraft:intangible_projectile";

    /// <summary>
    /// Parses an <see cref="IntangibleProjectileComponent"/> from an SNBT node representation.
    /// </summary>
    /// <param name="compound">The SNBT node to parse, which must be an <see cref="SnbtCompound"/>.</param>
    /// <returns>A new <see cref="IntangibleProjectileComponent"/> instance.</returns>
    /// <example>
    /// <code>
    /// var node = SnbtParser.Parse("{}");
    /// var component = IntangibleProjectileComponent.Parse(node);
    /// </code>
    /// </example>
    public static IntangibleProjectileComponent Parse(SnbtCompound compound) => new();

    /// <summary>
    /// Serializes the component into an empty SNBT compound node.
    /// </summary>
    /// <returns>An <see cref="ISnbtNode"/> representing an empty compound.</returns>
    public ISnbtNode ToSnbt() => Snbt.Compound().Build();
}