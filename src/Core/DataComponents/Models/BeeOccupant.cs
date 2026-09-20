using Core.DataComponents.Models.Interfaces;
using Core.SNBT;

using Core.SNBT.Interfaces;
using Core.SNBT.Nodes;

namespace Core.DataComponents.Models;

/// <summary>
/// Represents a single bee stored inside a beehive or bee nest item.
/// </summary>
/// <param name="EntityData">The raw entity NBT data compound representing the bee's state (e.g. variants, custom names, angry status).</param>
/// <param name="MinTicksInHive">The minimum number of ticks the bee must remain in the hive before emerging.</param>
/// <param name="TicksInHive">The total number of ticks the bee has currently occupied the hive.</param>
public record BeeOccupant(
    SnbtCompound EntityData,
    int MinTicksInHive,
    int TicksInHive
) : ICompoundModel<BeeOccupant>
{
    /// <summary>
    /// Parses a <see cref="BeeOccupant"/> instance from an SNBT compound node.
    /// </summary>
    /// <param name="compound">The SNBT compound containing bee occupant data.</param>
    /// <returns>A populated <see cref="BeeOccupant"/> instance.</returns>
    public static BeeOccupant Parse(SnbtCompound compound)
    {
        var entityData = compound.GetNode("entity_data") as SnbtCompound
                         ?? new SnbtCompound(new Dictionary<string, ISnbtNode>());

        return new BeeOccupant(
            EntityData: entityData,
            MinTicksInHive: compound.GetInt("min_ticks_in_hive"),
            TicksInHive: compound.GetInt("ticks_in_hive")
        );
    }

    /// <summary>
    /// Serializes the bee occupant data into an SNBT compound node.
    /// </summary>
    /// <returns>An <see cref="ISnbtNode"/> representing the bee occupant compound.</returns>
    public ISnbtNode ToSnbt() => Snbt.Compound()
        .Put("entity_data", EntityData)
        .Put("min_ticks_in_hive", MinTicksInHive)
        .Put("ticks_in_hive", TicksInHive)
        .Build();
}