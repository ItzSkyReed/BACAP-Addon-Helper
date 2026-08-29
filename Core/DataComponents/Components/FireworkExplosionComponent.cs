using Core.DataComponents.Models;
using Core.SNBT.Interfaces;
using Core.SNBT.Nodes;
using JetBrains.Annotations;

using Core.DataComponents.Interfaces;

namespace Core.DataComponents.Components;

/// <summary>
/// Represents the firework explosion effect stored on a firework star item (<c>minecraft:firework_explosion</c>).
/// </summary>
/// <param name="Explosion">The explosion effect definition containing shape, colors, trail, and twinkle settings.</param>
[UsedImplicitly]
public record FireworkExplosionComponent(
    FireworkExplosion Explosion
) : ICompoundComponent<FireworkExplosionComponent>
{
    /// <inheritdoc/>
    public static string ComponentId => "minecraft:firework_explosion";

    /// <summary>
    /// Initializes a new instance of the <see cref="FireworkExplosionComponent"/> record with inline explosion parameters.
    /// </summary>
    /// <param name="shape">The shape of the explosion (<c>small_ball</c>, <c>large_ball</c>, <c>star</c>, <c>creeper</c>, or <c>burst</c>).</param>
    /// <param name="colors">Optional list of initial particle RGB colors.</param>
    /// <param name="fadeColors">Optional list of fading particle RGB colors.</param>
    /// <param name="hasTrail">Whether the explosion leaves a trail.</param>
    /// <param name="hasTwinkle">Whether the explosion twinkles.</param>
    public FireworkExplosionComponent(
        string shape = "small_ball",
        List<int>? colors = null,
        List<int>? fadeColors = null,
        bool hasTrail = false,
        bool hasTwinkle = false
    ) : this(new FireworkExplosion(shape, colors, fadeColors, hasTrail, hasTwinkle))
    {
    }

    /// <summary>
    /// Parses a <see cref="FireworkExplosionComponent"/> from an SNBT node representation.
    /// </summary>
    /// <param name="compound">The SNBT node to parse, which must be an <see cref="SnbtCompound"/>.</param>
    /// <returns>A populated <see cref="FireworkExplosionComponent"/> instance.</returns>
    /// <example>
    /// <code>
    /// var node = SnbtParser.Parse("{shape: \"star\", colors: [I; 16711680, 65280], has_trail: 1b}");
    /// var component = FireworkExplosionComponent.Parse(node);
    /// </code>
    /// </example>
    public static FireworkExplosionComponent Parse(SnbtCompound compound)
    {
        return new FireworkExplosionComponent(FireworkExplosion.Parse(compound));
    }

    /// <summary>
    /// Serializes the component into an SNBT compound node.
    /// </summary>
    /// <returns>An <see cref="ISnbtNode"/> representing the firework explosion compound.</returns>
    public ISnbtNode ToSnbt() => Explosion.ToSnbt();
}