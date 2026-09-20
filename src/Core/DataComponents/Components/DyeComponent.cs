using Core.SNBT.Interfaces;
using Core.SNBT.Nodes;
using JetBrains.Annotations;

using Core.DataComponents.Interfaces;

namespace Core.DataComponents.Components;

/// <summary>
/// Stores the dye color that this item can be used as for crafting recipes and mob or block interactions (<c>minecraft:dye</c>).
/// </summary>
/// <param name="Color">The dye color identifier (e.g. <c>red</c>, <c>white</c>, <c>light_blue</c>).</param>
[UsedImplicitly]
public record DyeComponent(
    string Color
) : IStringComponent<DyeComponent>
{
    /// <inheritdoc/>
    public static string ComponentId => "minecraft:dye";

    /// <summary>
    /// Parses a <see cref="DyeComponent"/> from an SNBT node representation.
    /// </summary>
    /// <param name="strNode">The SNBT node to parse, which must be an <see cref="SnbtString"/>.</param>
    /// <returns>A populated <see cref="DyeComponent"/> instance.</returns>
    /// <example>
    /// <code>
    /// var node = SnbtParser.Parse("\"red\"");
    /// var component = DyeComponent.Parse(node);
    /// </code>
    /// </example>
    public static DyeComponent Parse(SnbtString strNode)
    {
        return new DyeComponent(strNode.Value);
    }

    /// <summary>
    /// Serializes the component into an SNBT string node.
    /// </summary>
    /// <returns>An <see cref="ISnbtNode"/> containing the dye color string.</returns>
    public ISnbtNode ToSnbt() => new SnbtString(Color);
}