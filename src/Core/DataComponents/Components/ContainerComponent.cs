using Core.DataComponents.Interfaces;
using Core.DataComponents.Models;
using Core.SNBT.Interfaces;
using Core.SNBT.Nodes;
using JetBrains.Annotations;

namespace Core.DataComponents.Components;

/// <summary>
/// Stores the indexed contents of a container block item (e.g. chest, dispenser, shulker box) (<c>minecraft:container</c>).
/// </summary>
/// <param name="Slots">The list of occupied container slots and their item stacks.</param>
[UsedImplicitly]
public record ContainerComponent(
    List<ContainerSlot> Slots
) : IListComponent<ContainerComponent>
{
    /// <inheritdoc/>
    public static string ComponentId => "minecraft:container";

    /// <summary>
    /// Initializes a new instance of the <see cref="ContainerComponent"/> record with an empty container.
    /// </summary>
    public ContainerComponent() : this(new List<ContainerSlot>())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ContainerComponent"/> record from an array of container slots.
    /// </summary>
    /// <param name="slots">The container slot entries.</param>
    public ContainerComponent(params ContainerSlot[] slots)
        : this(slots.ToList())
    {
    }

    /// <summary>
    /// Parses a <see cref="ContainerComponent"/> directly from an SNBT list node.
    /// Empty item stacks and invalid slots are automatically filtered out.
    /// </summary>
    /// <param name="list">The SNBT list containing serialized slot compounds.</param>
    /// <returns>A populated <see cref="ContainerComponent"/> instance.</returns>
    /// <example>
    /// <code>
    /// var node = SnbtParser.Parse("[{slot: 0, item: {id: 'minecraft:diamond', count: 64}}]");
    /// var container = ContainerComponent.Parse((SnbtList)node);
    /// </code>
    /// </example>
    public static ContainerComponent Parse(SnbtList list)
    {
        var slots = new List<ContainerSlot>(list.Items.Count);

        foreach (var entry in list.Items)
        {
            if (entry is not SnbtCompound entryComp)
                continue;

            var slotEntry = ContainerSlot.Parse(entryComp);
            if (!slotEntry.Item.IsEmpty)
                slots.Add(slotEntry);
        }

        return new ContainerComponent(slots);
    }

    /// <summary>
    /// Serializes all non-empty container slots into an SNBT list node.
    /// </summary>
    /// <returns>An <see cref="ISnbtNode"/> representing the container slots list.</returns>
    public ISnbtNode ToSnbt()
    {
        var list = new SnbtList();

        foreach (var slot in Slots)
        {
            if (!slot.Item.IsEmpty)
                list.Items.Add(slot.ToSnbt());
        }

        return list;
    }
}