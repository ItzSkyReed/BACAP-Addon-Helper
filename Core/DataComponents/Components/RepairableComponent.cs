using Core.SNBT;

using Core.SNBT.Interfaces;
using Core.SNBT.Nodes;
using JetBrains.Annotations;

using Core.DataComponents.Interfaces;

namespace Core.DataComponents.Components;

/// <summary>
/// Specifies the ingredient item(s) or item tag required to repair this item in an anvil (<c>minecraft:repairable</c>).
/// Also repairs wolf armor equipped on tamed wolves when fed matching items.
/// </summary>
/// <param name="Items">The list of valid item identifiers or a single hash-prefixed item tag (e.g. <c>#minecraft:planks</c>).</param>
[UsedImplicitly]
public record RepairableComponent(
    List<string> Items
) : ICompoundComponent<RepairableComponent>
{
    /// <inheritdoc/>
    public static string ComponentId => "minecraft:repairable";

    /// <summary>
    /// Initializes a new instance of the <see cref="RepairableComponent"/> record with a single item identifier or tag.
    /// </summary>
    /// <param name="itemOrTag">The item resource identifier or item tag (e.g., <c>minecraft:stick</c> or <c>#minecraft:planks</c>).</param>
    public RepairableComponent(string itemOrTag) : this(new List<string> { itemOrTag })
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RepairableComponent"/> record with an array of item identifiers.
    /// </summary>
    /// <param name="items">The item resource identifiers.</param>
    public RepairableComponent(params string[] items) : this(items.ToList())
    {
    }

    /// <summary>
    /// Parses a <see cref="RepairableComponent"/> from an SNBT node representation.
    /// </summary>
    /// <param name="compound">The SNBT node to parse, which must be an <see cref="SnbtCompound"/> containing an <c>items</c> tag.</param>
    /// <returns>A populated <see cref="RepairableComponent"/> instance.</returns>
    /// <example>
    /// <code>
    /// // From single item tag:
    /// var node1 = SnbtParser.Parse("{items: \"#minecraft:planks\"}");
    /// var comp1 = RepairableComponent.Parse(node1);
    ///
    /// // From multiple items list:
    /// var node2 = SnbtParser.Parse("{items: [\"minecraft:stick\", \"minecraft:blaze_rod\"]}");
    /// var comp2 = RepairableComponent.Parse(node2);
    /// </code>
    /// </example>
    public static RepairableComponent Parse(SnbtCompound compound)
    {

        var itemsNode = compound.GetNode("items")
            ?? throw new ArgumentException("Repairable compound is missing required 'items' tag.");

        return itemsNode switch
        {
            SnbtString str => new RepairableComponent(str.Value),
            SnbtList list => new RepairableComponent(ExtractItemsList(list)),
            _ => throw new ArgumentException("The 'items' tag in repairable component must be a string or list of strings.")
        };
    }

    /// <summary>
    /// Serializes the component into an SNBT compound node.
    /// </summary>
    /// <returns>An <see cref="ISnbtNode"/> representing the repairable compound.</returns>
    public ISnbtNode ToSnbt()
    {
        if (Items.Count == 1)
        {
            return Snbt.Compound()
                .Put("items", Items[0])
                .Build();
        }

        return Snbt.Compound()
            .PutList("items", list =>
            {
                foreach (var item in Items)
                    list.Add(item);
            })
            .Build();
    }

    /// <summary>
    /// Implicitly converts a single item ID or tag string into a <see cref="RepairableComponent"/>.
    /// </summary>
    /// <param name="itemOrTag">The item resource identifier or tag selector.</param>
    public static implicit operator RepairableComponent(string itemOrTag) => new(itemOrTag);

    private static List<string> ExtractItemsList(SnbtList list)
    {
        var items = new List<string>(list.Items.Count);
        foreach (var entry in list.Items)
        {
            if (entry is SnbtString str)
                items.Add(str.Value);
        }
        return items;
    }
}