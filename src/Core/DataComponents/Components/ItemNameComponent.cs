using Core.SNBT.Interfaces;
using Core.SNBT.Nodes;
using JetBrains.Annotations;

using Core.DataComponents.Interfaces;

namespace Core.DataComponents.Components;

/// <summary>
/// Represents the base default display name of an item (<c>minecraft:item_name</c>).
/// </summary>
/// <remarks>
/// Unlike <c>minecraft:custom_name</c>, this name is not italicized by default, cannot be cleared
/// using an anvil, and does not propagate to entity nametags or banner map markers.
/// </remarks>
/// <param name="Value">The underlying SNBT node representing the text component.</param>
[UsedImplicitly]
public record ItemNameComponent(
    ISnbtNode Value
) : IParsableComponent<ItemNameComponent>
{
    /// <inheritdoc/>
    public static string ComponentId => "minecraft:item_name";

    /// <summary>
    /// Initializes a new instance of the <see cref="ItemNameComponent"/> record with a literal string value.
    /// </summary>
    /// <param name="plainText">The literal unformatted string name.</param>
    public ItemNameComponent(string plainText) : this(new SnbtString(plainText))
    {
    }

    /// <summary>
    /// Parses an <see cref="ItemNameComponent"/> from an SNBT node representation.
    /// </summary>
    /// <param name="node">The SNBT node (string, compound, or list) representing the text component.</param>
    /// <returns>A populated <see cref="ItemNameComponent"/> instance.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="node"/> is not an <see cref="SnbtString"/>, <see cref="SnbtCompound"/>, or <see cref="SnbtList"/>.
    /// </exception>
    /// <example>
    /// <code>
    /// var node = SnbtParser.Parse("{text: \"Legendary Orb\", color: \"gold\"}");
    /// var component = ItemNameComponent.Parse(node);
    /// </code>
    /// </example>
    public static ItemNameComponent Parse(ISnbtNode node)
    {
        return node is not (SnbtString or SnbtCompound or SnbtList)
            ? throw new ArgumentException("Item name component must be a string, compound, or list representing a text component.")
            : new ItemNameComponent(node);
    }

    /// <summary>
    /// Serializes the text component into its corresponding SNBT node.
    /// </summary>
    /// <returns>The <see cref="ISnbtNode"/> containing the text component representation.</returns>
    public ISnbtNode ToSnbt() => Value;

    /// <summary>
    /// Implicitly converts a plain text string into an <see cref="ItemNameComponent"/>.
    /// </summary>
    /// <param name="plainText">The string literal name.</param>
    public static implicit operator ItemNameComponent(string plainText) => new(plainText);
}