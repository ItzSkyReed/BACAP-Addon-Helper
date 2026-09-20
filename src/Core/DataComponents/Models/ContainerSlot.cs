using Core.DataComponents.Models.Interfaces;
using Core.Items;
using Core.SNBT;

using Core.SNBT.Interfaces;
using Core.SNBT.Nodes;

namespace Core.DataComponents.Models;

public record ContainerSlot(
    int Slot,
    ItemStack Item
) : ICompoundModel<ContainerSlot>
{
    public static ContainerSlot Parse(SnbtCompound compound)
    {
        var itemCompound = compound.GetNode("item") as SnbtCompound ?? compound;

        return new ContainerSlot(
            Slot: compound.GetInt("slot"),
            Item: ItemStack.Parse(itemCompound)
        );
    }

    public ISnbtNode ToSnbt() => Snbt.Compound()
        .Put("slot", Slot)
        .Put("item", Item.ToSnbt())
        .Build();
}