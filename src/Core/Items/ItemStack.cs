using Core.DataComponents;
using Core.DataComponents.Models.Interfaces;
using Core.SNBT;
using Core.SNBT.Interfaces;
using Core.SNBT.Nodes;
using JetBrains.Annotations;

namespace Core.Items;

/// <summary>
/// Represents a Minecraft item stack with an identifier, count, and associated data components.
/// </summary>
/// <param name="Id">The resource location identifier of the item (e.g., "minecraft:diamond_sword").</param>
/// <param name="Count">The stack size.</param>
public sealed record ItemStack(
    string Id,
    int Count = 1
) : ICompoundModel<ItemStack>
{
    [PublicAPI]
    public const string AirId = "minecraft:air";

    /// <summary>
    /// Gets the collection of data components attached to this item stack.
    /// </summary>
    [UsedImplicitly]
    public DataComponentMap Components { get; init; } = new();

    /// <summary>
    /// Gets whether this stack is empty or contains no items.
    /// </summary>
    [PublicAPI]
    public bool IsEmpty => Count <= 0 || string.IsNullOrEmpty(Id) || Id == AirId;

    /// <summary>
    /// Parses an <see cref="ItemStack"/> from an NBT compound (e.g., inventory slot or container NBT).
    /// </summary>
    /// <param name="compound">The SNBT compound node to deserialize.</param>
    /// <returns>A populated <see cref="ItemStack"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="compound"/> is null.</exception>
    public static ItemStack Parse(SnbtCompound compound)
    {
        ArgumentNullException.ThrowIfNull(compound);

        var item = new ItemStack(
            Id: compound.GetString("id", compound.GetString("Id", AirId)),
            Count: compound.GetInt("count", compound.GetInt("Count", 1))
        );

        if (compound.GetNode("components") is not SnbtCompound compNode)
            return item;

        foreach (var (compId, valNode) in compNode.Tags)
            item.Components.Set(ComponentRegistry.Parse(compId, valNode));

        return item;
    }

    /// <summary>
    /// Serializes the item stack into an NBT compound tag.
    /// </summary>
    /// <returns>An <see cref="ISnbtNode"/> representing the compound.</returns>
    public ISnbtNode ToSnbt()
    {
        var builder = Snbt.Compound()
            .Put("id", string.IsNullOrEmpty(Id) ? AirId : Id)
            .PutOptional("count", Count, 1);

        if (!Components.IsEmpty)
            builder.Put("components", Components.ToSnbt());

        return builder.Build();
    }

    /// <summary>
    /// Serializes the item stack into the 1.20.5+ command string format (e.g., "minecraft:stick[damage=10]").
    /// </summary>
    /// <returns>A string representation formatted for Minecraft command arguments.</returns>
    [PublicAPI]
    public string ToCommandString()
    {
        if (Components.IsEmpty || Components.ToSnbt() is not SnbtCompound { Tags.Count: > 0 } compNode)
            return Id;

        var pairs = compNode.Tags.Select(kv => $"{kv.Key}={kv.Value.ToSnbtString()}");
        return $"{Id}[{string.Join(", ", pairs)}]";
    }

    /// <summary>
    /// Implicitly converts an item identifier string into an <see cref="ItemStack"/> with count 1.
    /// </summary>
    /// <param name="id">The item identifier.</param>
    public static implicit operator ItemStack(string id) => new(id);

    /// <inheritdoc/>
    public override string ToString() => IsEmpty ? "Air" : $"{Count}x {Id}";
}