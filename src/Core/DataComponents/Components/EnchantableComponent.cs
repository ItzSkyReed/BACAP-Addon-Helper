using Core.SNBT;
using Core.SNBT.Nodes;

using Core.SNBT.Interfaces;
using JetBrains.Annotations;

using Core.DataComponents.Interfaces;

namespace Core.DataComponents.Components;

/// <summary>
/// Represents the enchantability property of an item (<c>minecraft:enchantable</c>).
/// Determines whether and how effectively the item can receive enchantments in an enchanting table.
/// </summary>
/// <param name="Value">A positive integer representing the item's enchantability rating.</param>
[UsedImplicitly]
public record EnchantableComponent(
    int Value
) : ICompoundComponent<EnchantableComponent>
{
    /// <inheritdoc/>
    public static string ComponentId => "minecraft:enchantable";

    /// <summary>
    /// Parses an <see cref="EnchantableComponent"/> from an SNBT node representation.
    /// </summary>
    /// <param name="compound">The SNBT node to parse, which must be an <see cref="SnbtCompound"/>.</param>
    /// <returns>A populated <see cref="EnchantableComponent"/> instance.</returns>
    /// <example>
    /// <code>
    /// var node = SnbtParser.Parse("{value: 15}");
    /// var component = EnchantableComponent.Parse(node);
    /// </code>
    /// </example>
    public static EnchantableComponent Parse(SnbtCompound compound)
    {

        return new EnchantableComponent(
            Value: compound.GetInt("value")
        );
    }

    /// <summary>
    /// Serializes the component into an SNBT compound node.
    /// </summary>
    /// <returns>An <see cref="ISnbtNode"/> containing the enchantability compound.</returns>
    public ISnbtNode ToSnbt() => Snbt.Compound()
        .Put("value", Value)
        .Build();
}