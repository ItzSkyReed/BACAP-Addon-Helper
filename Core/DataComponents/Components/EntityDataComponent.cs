using Core.SNBT.Nodes;

using Core.SNBT.Interfaces;
using JetBrains.Annotations;

using Core.DataComponents.Interfaces;

namespace Core.DataComponents.Components;

/// <summary>
/// Represents entity NBT applied to an entity spawned or placed using this item (<c>minecraft:entity_data</c>).
/// </summary>
/// <param name="Tag">The underlying SNBT compound containing entity definitions and custom tags (such as <c>id</c>, <c>Small</c>, <c>NoGravity</c>).</param>
[UsedImplicitly]
public record EntityDataComponent(
    SnbtCompound Tag
) : ICompoundComponent<EntityDataComponent>
{
    /// <inheritdoc/>
    public static string ComponentId => "minecraft:entity_data";

    /// <summary>
    /// Gets the resource location identifier of the entity to spawn.
    /// </summary>
    public string Id => Tag.GetString("id");

    /// <summary>
    /// Initializes a new instance of the <see cref="EntityDataComponent"/> record with an entity ID and optional custom tag entries.
    /// </summary>
    /// <param name="id">The resource location of the entity (e.g. <c>minecraft:armor_stand</c>).</param>
    /// <param name="customTags">Optional additional NBT tags applied to the entity.</param>
    public EntityDataComponent(string id, Dictionary<string, ISnbtNode>? customTags = null)
        : this(BuildCompound(id, customTags))
    {
    }

    /// <summary>
    /// Parses an <see cref="EntityDataComponent"/> from an SNBT node representation.
    /// </summary>
    /// <param name="compound">The SNBT node to parse, which must be an <see cref="SnbtCompound"/>.</param>
    /// <returns>A populated <see cref="EntityDataComponent"/> instance.</returns>
    /// <example>
    /// <code>
    /// var node = SnbtParser.Parse("{id: \"minecraft:armor_stand\", Small: 1b, Invisible: 1b}");
    /// var component = EntityDataComponent.Parse(node);
    /// </code>
    /// </example>
    public static EntityDataComponent Parse(SnbtCompound compound)
    {
        return new EntityDataComponent(compound);
    }

    /// <summary>
    /// Serializes the component into an SNBT compound node.
    /// </summary>
    /// <returns>An <see cref="ISnbtNode"/> containing the entity data compound.</returns>
    public ISnbtNode ToSnbt() => Tag;

    private static SnbtCompound BuildCompound(string id, Dictionary<string, ISnbtNode>? customTags)
    {
        var dict = customTags != null
            ? new Dictionary<string, ISnbtNode>(customTags)
            : new Dictionary<string, ISnbtNode>();

        dict["id"] = new SnbtString(id);
        return new SnbtCompound(dict);
    }
}