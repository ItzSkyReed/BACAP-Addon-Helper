using Core.SNBT.Interfaces;
using Core.SNBT.Nodes;
using JetBrains.Annotations;

using Core.DataComponents.Interfaces;

namespace Core.DataComponents.Components;

/// <summary>
/// Specifies the damage type that this item deals when attacking (<c>minecraft:damage_type</c>).
/// </summary>
/// <param name="DamageType">The identifier of the damage type resource (e.g. <c>minecraft:campfire</c>).</param>
[UsedImplicitly]
public record DamageTypeComponent(
    string DamageType
) : IStringComponent<DamageTypeComponent>
{
    /// <inheritdoc/>
    public static string ComponentId => "minecraft:damage_type";

    /// <summary>
    /// Parses a <see cref="DamageTypeComponent"/> from an SNBT node representation.
    /// </summary>
    /// <param name="strNode">The SNBT node to parse, which must be an <see cref="SnbtString"/>.</param>
    /// <returns>A populated <see cref="DamageTypeComponent"/> instance.</returns>
    /// <example>
    /// <code>
    /// var node = SnbtParser.Parse("\"minecraft:campfire\"");
    /// var component = DamageTypeComponent.Parse(node);
    /// </code>
    /// </example>
    public static DamageTypeComponent Parse(SnbtString strNode)
    {
        return new DamageTypeComponent(strNode.Value);
    }

    /// <summary>
    /// Serializes the component into an SNBT string node.
    /// </summary>
    /// <returns>An <see cref="ISnbtNode"/> containing the damage type identifier.</returns>
    public ISnbtNode ToSnbt() => new SnbtString(DamageType);
}