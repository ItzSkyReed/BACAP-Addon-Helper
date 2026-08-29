using Core.SNBT;
using Core.SNBT.Nodes;

using Core.SNBT.Interfaces;
using JetBrains.Annotations;

using Core.DataComponents.Interfaces;

namespace Core.DataComponents.Components;

/// <summary>
/// Contains a map of inactive enchantments and their levels, typically used by enchanted books (<c>minecraft:stored_enchantments</c>).
/// </summary>
/// <param name="Levels">A dictionary mapping enchantment resource locations to their respective levels.</param>
/// <param name="ShowInTooltip">Whether the stored enchantments are displayed in the item tooltip. Defaults to <see langword="true"/>.</param>
[UsedImplicitly]
public record StoredEnchantmentsComponent(
    Dictionary<string, int> Levels,
    bool ShowInTooltip = true
) : ICompoundComponent<StoredEnchantmentsComponent>
{
    /// <inheritdoc/>
    public static string ComponentId => "minecraft:stored_enchantments";

    /// <summary>
    /// Initializes an empty instance of the <see cref="StoredEnchantmentsComponent"/> record.
    /// </summary>
    public StoredEnchantmentsComponent() : this(new Dictionary<string, int>())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StoredEnchantmentsComponent"/> record with a single stored enchantment.
    /// </summary>
    /// <param name="enchantmentId">The resource identifier of the enchantment.</param>
    /// <param name="level">The enchantment level.</param>
    public StoredEnchantmentsComponent(string enchantmentId, int level) : this(new Dictionary<string, int> { [enchantmentId] = level })
    {
    }

    /// <summary>
    /// Parses a <see cref="StoredEnchantmentsComponent"/> from an SNBT node representation.
    /// Supports both canonical object format (<c>{ levels: { ... }, show_in_tooltip: true }</c>) and direct key-value mapping.
    /// </summary>
    /// <param name="compound">The SNBT node to parse, which must be an <see cref="SnbtCompound"/>.</param>
    /// <returns>A populated <see cref="StoredEnchantmentsComponent"/> instance.</returns>
    /// <example>
    /// <code>
    /// // From shorthand key-values:
    /// var node1 = SnbtParser.Parse("{\"minecraft:sharpness\": 5, \"minecraft:unbreaking\": 3}");
    /// var comp1 = StoredEnchantmentsComponent.Parse(node1);
    ///
    /// // From canonical object:
    /// var node2 = SnbtParser.Parse("{levels: {\"minecraft:mending\": 1}, show_in_tooltip: 1b}");
    /// var comp2 = StoredEnchantmentsComponent.Parse(node2);
    /// </code>
    /// </example>
    public static StoredEnchantmentsComponent Parse(SnbtCompound compound)
    {

        var levels = new Dictionary<string, int>();
        var showInTooltip = true;

        if (compound.GetNode("levels") is SnbtCompound levelsComp)
        {
            showInTooltip = compound.GetBool("show_in_tooltip", true);
            foreach (var (key, valNode) in levelsComp.Tags)
            {
                levels[key] = valNode switch
                {
                    SnbtInt intNode => intNode.Value,
                    SnbtByte byteNode => byteNode.Value,
                    SnbtShort shortNode => shortNode.Value,
                    _ => levels[key]
                };
            }
        }
        else
        {
            foreach (var (key, valNode) in compound.Tags)
            {
                if (key == "show_in_tooltip" && valNode is SnbtByte b)
                {
                    showInTooltip = b.Value != 0;
                    continue;
                }

                levels[key] = valNode switch
                {
                    SnbtInt intVal => intVal.Value,
                    SnbtByte byteVal => byteVal.Value,
                    SnbtShort shortVal => shortVal.Value,
                    _ => levels[key]
                };
            }
        }

        return new StoredEnchantmentsComponent(levels, showInTooltip);
    }

    /// <summary>
    /// Serializes the component into an SNBT compound node.
    /// </summary>
    /// <returns>An <see cref="ISnbtNode"/> representing the stored enchantments compound.</returns>
    public ISnbtNode ToSnbt()
    {
        var levelsCompound = Snbt.Compound();
        foreach (var (enchantment, level) in Levels)
        {
            levelsCompound.Put(enchantment, level);
        }

        if (!ShowInTooltip)
        {
            return Snbt.Compound()
                .Put("levels", levelsCompound.Build())
                .Put("show_in_tooltip", false)
                .Build();
        }

        return levelsCompound.Build();
    }
}