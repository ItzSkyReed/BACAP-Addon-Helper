using Core.SNBT;
using Core.SNBT.Interfaces;
using Core.SNBT.Nodes;

using JetBrains.Annotations;

using Core.DataComponents.Interfaces;

namespace Core.DataComponents.Components;

/// <summary>
/// Represents the active enchantments applied to an item (<c>minecraft:enchantments</c>).
/// </summary>
/// <remarks>
/// Maps enchantment resource identifiers to their respective levels.
/// These enchantments actively apply their effects during gameplay (unlike <c>minecraft:stored_enchantments</c>).
/// </remarks>
/// <param name="Levels">A dictionary mapping enchantment resource locations to their level integers.</param>
[UsedImplicitly]
public record EnchantmentsComponent(
    Dictionary<string, int> Levels
) : ICompoundComponent<EnchantmentsComponent>
{
    /// <inheritdoc/>
    public static string ComponentId => "minecraft:enchantments";

    /// <summary>
    /// Initializes an empty <see cref="EnchantmentsComponent"/> instance.
    /// </summary>
    public EnchantmentsComponent() : this(new Dictionary<string, int>())
    {
    }

    /// <summary>
    /// Parses an <see cref="EnchantmentsComponent"/> from an SNBT node representation.
    /// </summary>
    /// <param name="compound">The SNBT node to parse, which must be an <see cref="SnbtCompound"/>.</param>
    /// <returns>A populated <see cref="EnchantmentsComponent"/> instance.</returns>
    /// <example>
    /// <code>
    /// var node = SnbtParser.Parse("{sharpness: 3, knockback: 2}");
    /// var component = EnchantmentsComponent.Parse(node);
    /// </code>
    /// </example>
    public static EnchantmentsComponent Parse(SnbtCompound compound)
    {

        var levels = new Dictionary<string, int>(compound.Tags.Count);

        foreach (var (enchantmentId, levelNode) in compound.Tags)
        {
            levels[enchantmentId] = levelNode switch
            {
                SnbtInt intNode => intNode.Value,
                SnbtByte byteNode => byteNode.Value,
                SnbtShort shortNode => shortNode.Value,
                _ => levels[enchantmentId]
            };
        }

        return new EnchantmentsComponent(levels);
    }

    /// <summary>
    /// Serializes the component into an SNBT compound node.
    /// </summary>
    /// <returns>An <see cref="ISnbtNode"/> containing the enchantment level map.</returns>
    public ISnbtNode ToSnbt()
    {
        var builder = Snbt.Compound();

        foreach (var (enchantmentId, level) in Levels)
        {
            builder.Put(enchantmentId, level);
        }

        return builder.Build();
    }
}