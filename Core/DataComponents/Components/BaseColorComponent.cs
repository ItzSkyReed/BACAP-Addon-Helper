using Core.DataComponents.Interfaces;
using Core.SNBT.Interfaces;
using Core.SNBT.Nodes;
using JetBrains.Annotations;

namespace Core.DataComponents.Components;

/// <summary>
/// Defines the base dye color of a shield (<c>minecraft:base_color</c>).
/// </summary>
/// <param name="Color">The dye color identifier (e.g. <c>white</c>, <c>black</c>, <c>light_gray</c>).</param>
[UsedImplicitly]
public record BaseColorComponent(string Color) : IStringComponent<BaseColorComponent>
{
    /// <inheritdoc/>
    public static string ComponentId => "minecraft:base_color";

    /// <summary>
    /// Parses a <see cref="BaseColorComponent"/> from an SNBT node representation.
    /// </summary>
    /// <param name="strNode">The SNBT node to parse, which must be an <see cref="SnbtString"/>.</param>
    /// <returns>A populated <see cref="BaseColorComponent"/> instance.</returns>
    /// <example>
    /// <code>
    /// var node = new SnbtString("cyan");
    /// var baseColor = BaseColorComponent.Parse(node);
    /// </code>
    /// </example>
    public static BaseColorComponent Parse(SnbtString strNode)
    {
        return new BaseColorComponent(strNode.Value);
    }

    /// <summary>
    /// Serializes the base color into an SNBT string node.
    /// </summary>
    /// <returns>An <see cref="ISnbtNode"/> representing the dye color string.</returns>
    public ISnbtNode ToSnbt() => new SnbtString(Color);

    /// <summary>
    /// Implicitly converts a dye color name string into a <see cref="BaseColorComponent"/>.
    /// </summary>
    /// <param name="color">The dye color identifier.</param>
    public static implicit operator BaseColorComponent(string color) => new(color);

    /// <inheritdoc/>
    public override string ToString() => Color;
}