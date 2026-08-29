using Core.DataComponents.Interfaces;
using Core.DataComponents.Models;
using Core.DataComponents.Models.Extensions;

using Core.SNBT.Interfaces;
using Core.SNBT.Nodes;
using JetBrains.Annotations;

namespace Core.DataComponents.Components;

/// <summary>
/// Defines block placement restrictions and matching predicates while in Adventure mode (<c>minecraft:can_place_on</c>).
/// </summary>
/// <param name="Predicates">The list of block predicates matching blocks upon which this item can be placed.</param>
[UsedImplicitly]
public record CanPlaceOnComponent(
    List<BlockPredicate> Predicates
) : IFlexibleComponent<CanPlaceOnComponent>
{
    /// <inheritdoc/>
    public static string ComponentId => "minecraft:can_place_on";

    /// <summary>
    /// Initializes a new instance of the <see cref="CanPlaceOnComponent"/> record with an empty list of predicates.
    /// </summary>
    public CanPlaceOnComponent() : this(new List<BlockPredicate>())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CanPlaceOnComponent"/> record from an array of block predicates.
    /// </summary>
    /// <param name="predicates">The block predicates to match.</param>
    public CanPlaceOnComponent(params BlockPredicate[] predicates)
        : this(predicates.ToList())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CanPlaceOnComponent"/> record for a single block identifier or tag.
    /// </summary>
    /// <param name="blockOrTag">The block identifier (e.g. <c>minecraft:grass_block</c>) or tag selector (e.g. <c>#minecraft:dirt</c>).</param>
    public CanPlaceOnComponent(string blockOrTag)
        : this(new BlockPredicate(Blocks: [blockOrTag]))
    {
    }

    /// <summary>
    /// Parses a <see cref="CanPlaceOnComponent"/> from an SNBT node representation.
    /// Supports a list of predicates, a compound containing a <c>predicates</c> list, or a single inline predicate compound.
    /// </summary>
    /// <param name="node">The SNBT node to parse (<see cref="SnbtList"/> or <see cref="SnbtCompound"/>).</param>
    /// <returns>A populated <see cref="CanPlaceOnComponent"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="node"/> is neither a list nor a compound.</exception>
    /// <example>
    /// <code>
    /// // From a list of predicates:
    /// var listNode = SnbtParser.Parse("[{blocks: 'minecraft:dirt'}, {blocks: '#minecraft:planks'}]");
    /// var canPlace1 = CanPlaceOnComponent.Parse(listNode);
    ///
    /// // From a single block predicate compound:
    /// var compNode = SnbtParser.Parse("{blocks: 'minecraft:sand'}");
    /// var canPlace2 = CanPlaceOnComponent.Parse(compNode);
    /// </code>
    /// </example>
    public static CanPlaceOnComponent Parse(ISnbtNode node)
    {
        var predicates = new List<BlockPredicate>();

        switch (node)
        {
            case SnbtList list:
            {
                foreach (var item in list.Items)
                {
                    if (item is SnbtCompound comp)
                        predicates.Add(BlockPredicate.Parse(comp));
                }
                break;
            }
            case SnbtCompound compound:
            {
                // Handles wrapped predicates list format: {predicates: [...]}
                if (compound.GetCompoundList<BlockPredicate>("predicates") is { } innerList)
                {
                    predicates.AddRange(innerList);
                }
                else
                {
                    // Handles single inline predicate compound format: {blocks: "..."}
                    predicates.Add(BlockPredicate.Parse(compound));
                }
                break;
            }
            default:
                throw new ArgumentException("CanPlaceOn component must be either a compound or a list of predicates.");
        }

        return new CanPlaceOnComponent(predicates);
    }

    /// <summary>
    /// Serializes the component into an SNBT node.
    /// Single predicates are serialized compactly as an <see cref="SnbtCompound"/>, while multiple predicates are serialized as an <see cref="SnbtList"/>.
    /// </summary>
    /// <returns>An <see cref="ISnbtNode"/> representing the target placement block predicates.</returns>
    public ISnbtNode ToSnbt()
    {
        if (Predicates.Count == 1)
            return Predicates[0].ToSnbt();

        var list = new SnbtList();
        foreach (var predicate in Predicates)
            list.Items.Add(predicate.ToSnbt());

        return list;
    }

    /// <summary>
    /// Implicitly converts a block identifier or tag string into a <see cref="CanPlaceOnComponent"/>.
    /// </summary>
    /// <param name="blockOrTag">The block identifier or tag selector.</param>
    public static implicit operator CanPlaceOnComponent(string blockOrTag) => new(blockOrTag);
}