using Core.DataComponents.Interfaces;
using Core.Items;
using Core.SNBT.Interfaces;
using Core.SNBT.Nodes;
using JetBrains.Annotations;

namespace Core.DataComponents.Components;

/// <summary>
/// Stores the collection of items held inside a bundle or dyed bundle item (<c>minecraft:bundle_contents</c>).
/// </summary>
/// <param name="Items">The list of nested item stacks stored inside the bundle.</param>
[UsedImplicitly]
public record BundleContentsComponent(
    List<ItemStack> Items
) : IListComponent<BundleContentsComponent>
{
    /// <inheritdoc/>
    public static string ComponentId => "minecraft:bundle_contents";

    /// <summary>
    /// Initializes a new instance of the <see cref="BundleContentsComponent"/> record with an empty item list.
    /// </summary>
    public BundleContentsComponent() : this(new List<ItemStack>())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BundleContentsComponent"/> record from an array of item stacks.
    /// </summary>
    /// <param name="items">The items stored in the bundle.</param>
    public BundleContentsComponent(params ItemStack[] items)
        : this(items.ToList())
    {
    }

    /// <summary>
    /// Parses a <see cref="BundleContentsComponent"/> directly from an SNBT list node.
    /// Empty item stacks and air entries are automatically filtered out.
    /// </summary>
    /// <param name="list">The SNBT list containing serialized item compounds.</param>
    /// <returns>A populated <see cref="BundleContentsComponent"/> instance.</returns>
    /// <example>
    /// <code>
    /// var node = SnbtParser.Parse("[{id: 'minecraft:diamond', count: 3}, {id: 'minecraft:stick', count: 1}]");
    /// var bundle = BundleContentsComponent.Parse((SnbtList)node);
    /// </code>
    /// </example>
    public static BundleContentsComponent Parse(SnbtList list)
    {
        var items = new List<ItemStack>(list.Items.Count);

        foreach (var entry in list.Items)
        {
            if (entry is not SnbtCompound compound)
                continue;

            var itemStack = ItemStack.Parse(compound);
            if (!itemStack.IsEmpty)
            {
                items.Add(itemStack);
            }
        }

        return new BundleContentsComponent(items);
    }

    /// <summary>
    /// Serializes all non-empty bundle items into an SNBT list node.
    /// </summary>
    /// <returns>An <see cref="ISnbtNode"/> representing the bundle contents list.</returns>
    public ISnbtNode ToSnbt()
    {
        var list = new SnbtList();

        foreach (var item in Items)
        {
            if (!item.IsEmpty)
            {
                list.Items.Add(item.ToSnbt());
            }
        }

        return list;
    }
}