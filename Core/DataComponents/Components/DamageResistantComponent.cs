using Core.SNBT;
using Core.SNBT.Nodes;

using Core.SNBT.Interfaces;
using JetBrains.Annotations;

using Core.DataComponents.Interfaces;

namespace Core.DataComponents.Components;

/// <summary>
/// Represents immunity configuration for an item against specific damage types when dropped as an entity or equipped (<c>minecraft:damage_resistant</c>).
/// </summary>
/// <param name="Types">A damage type tag identifier prefixed with <c>#</c> (e.g. <c>#minecraft:is_fire</c>).</param>
[UsedImplicitly]
public record DamageResistantComponent(
    string Types
) : ICompoundComponent<DamageResistantComponent>
{
    /// <inheritdoc/>
    public static string ComponentId => "minecraft:damage_resistant";

    /// <summary>
    /// Parses a <see cref="DamageResistantComponent"/> from an SNBT node representation.
    /// </summary>
    /// <param name="compound">The SNBT node to parse, which must be an <see cref="SnbtCompound"/>.</param>
    /// <returns>A populated <see cref="DamageResistantComponent"/> instance.</returns>
    /// <example>
    /// <code>
    /// var node = SnbtParser.Parse("{types: \"#minecraft:is_fire\"}");
    /// var component = DamageResistantComponent.Parse(node);
    /// </code>
    /// </example>
    public static DamageResistantComponent Parse(SnbtCompound compound)
    {

        return new DamageResistantComponent(
            Types: compound.GetString("types")
        );
    }

    /// <summary>
    /// Serializes the component into an SNBT compound node.
    /// </summary>
    /// <returns>An <see cref="ISnbtNode"/> representing the compound.</returns>
    public ISnbtNode ToSnbt() => Snbt.Compound()
        .Put("types", Types)
        .Build();
}