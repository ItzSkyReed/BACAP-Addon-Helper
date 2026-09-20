using Core.DataComponents.Interfaces;
using Core.DataComponents.Models;
using Core.SNBT.Interfaces;
using Core.SNBT.Nodes;
using JetBrains.Annotations;

namespace Core.DataComponents.Components;

/// <summary>
/// Stores the collection of bees occupying a beehive or bee nest item (<c>minecraft:bees</c>).
/// </summary>
/// <param name="Bees">The list of bees housed within the block item.</param>
[UsedImplicitly]
public record BeesComponent(
    List<BeeOccupant> Bees
) : IListComponent<BeesComponent>
{
    /// <inheritdoc/>
    public static string ComponentId => "minecraft:bees";

    /// <summary>
    /// Initializes a new instance of the <see cref="BeesComponent"/> record with an array of bee occupants.
    /// </summary>
    /// <param name="bees">The bee occupants to store in the hive.</param>
    public BeesComponent(params BeeOccupant[] bees)
        : this(bees.ToList())
    {
    }

    /// <summary>
    /// Parses a <see cref="BeesComponent"/> from an SNBT node representation.
    /// </summary>
    /// <param name="list">The SNBT node to parse, which must be an <see cref="SnbtList"/>.</param>
    /// <returns>A populated <see cref="BeesComponent"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="list"/> is not an <see cref="SnbtList"/>.</exception>
    /// <example>
    /// <code>
    /// var node = SnbtParser.Parse("[{entity_data: {id: 'minecraft:bee'}, min_ticks_in_hive: 600, ticks_in_hive: 200}]");
    /// var component = BeesComponent.Parse(node);
    /// </code>
    /// </example>
    public static BeesComponent Parse(SnbtList list)
    {

        var bees = new List<BeeOccupant>(list.Items.Count);

        foreach (var item in list.Items)
        {
            if (item is SnbtCompound compound)
                bees.Add(BeeOccupant.Parse(compound));
        }

        return new BeesComponent(bees);
    }

    /// <summary>
    /// Serializes the list of bee occupants into an SNBT list node.
    /// </summary>
    /// <returns>An <see cref="ISnbtNode"/> representing the list of bee compounds.</returns>
    public ISnbtNode ToSnbt()
    {
        var list = new SnbtList();

        foreach (var bee in Bees)
            list.Items.Add(bee.ToSnbt());

        return list;
    }
}