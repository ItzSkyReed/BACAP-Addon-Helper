using Core.DataComponents.Interfaces;
using Core.DataComponents.Models;
using Core.DataComponents.Models.Extensions;

using Core.SNBT.Interfaces;
using Core.SNBT.Nodes;
using JetBrains.Annotations;

namespace Core.DataComponents.Components;

/// <summary>
/// Defines block breaking restrictions and matching predicates while in Adventure mode (<c>minecraft:can_break</c>).
/// </summary>
/// <param name="Predicates">The list of block predicates matching blocks the player is permitted to break.</param>
[UsedImplicitly]
public record CanBreakComponent(
    List<BlockPredicate> Predicates
) : IFlexibleComponent<CanBreakComponent>
{
    /// <inheritdoc/>
    public static string ComponentId => "minecraft:can_break";

    /// <summary>
    /// Initializes a new instance of the <see cref="CanBreakComponent"/> record with an empty list of predicates.
    /// </summary>
    public CanBreakComponent() : this(new List<BlockPredicate>())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CanBreakComponent"/> record from an array of block predicates.
    /// </summary>
    /// <param name="predicates">The block predicates to match.</param>
    public CanBreakComponent(params BlockPredicate[] predicates)
        : this(predicates.ToList())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CanBreakComponent"/> record for a single block identifier or tag.
    /// </summary>
    /// <param name="blockOrTag">The block identifier (e.g. <c>minecraft:stone</c>) or tag selector (e.g. <c>#minecraft:mineable/pickaxe</c>).</param>
    public CanBreakComponent(string blockOrTag)
        : this(new BlockPredicate(Blocks: [blockOrTag]))
    {
    }

    /// <summary>
    /// Parses a <see cref="CanBreakComponent"/> from an SNBT node representation.
    /// Supports a list of predicates, a compound containing a <c>predicates</c> list, or a single inline predicate compound.
    /// </summary>
    /// <param name="node">The SNBT node to parse (<see cref="SnbtList"/> or <see cref="SnbtCompound"/>).</param>
    /// <returns>A populated <see cref="CanBreakComponent"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="node"/> is neither a list nor a compound.</exception>
    public static CanBreakComponent Parse(ISnbtNode node)
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
                if (compound.GetCompoundList<BlockPredicate>("predicates") is { } innerList)
                {
                    predicates.AddRange(innerList);
                }
                else
                {
                    predicates.Add(BlockPredicate.Parse(compound));
                }
                break;
            }
            default:
                throw new ArgumentException("CanBreak component must be either a compound or a list of predicates.");
        }

        return new CanBreakComponent(predicates);
    }

    /// <summary>
    /// Serializes the component into an SNBT node.
    /// Single predicates are serialized compactly as an <see cref="SnbtCompound"/>, while multiple predicates are serialized as an <see cref="SnbtList"/>.
    /// </summary>
    /// <returns>An <see cref="ISnbtNode"/> representing the block predicates.</returns>
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
    /// Implicitly converts a block identifier or tag string into a <see cref="CanBreakComponent"/>.
    /// </summary>
    /// <param name="blockOrTag">The block identifier or tag selector.</param>
    public static implicit operator CanBreakComponent(string blockOrTag) => new(blockOrTag);
}