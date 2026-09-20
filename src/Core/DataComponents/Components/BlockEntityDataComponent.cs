using Core.DataComponents.Interfaces;
using Core.SNBT.Interfaces;
using Core.SNBT.Nodes;
using JetBrains.Annotations;

namespace Core.DataComponents.Components;

/// <summary>
/// Stores NBT data to be applied to a block entity when the corresponding block is placed in the world (<c>minecraft:block_entity_data</c>).
/// </summary>
/// <param name="Data">The raw SNBT compound containing the block entity tags (e.g. chest inventory, spawner properties, command block commands).</param>
[UsedImplicitly]
public record BlockEntityDataComponent(
    SnbtCompound Data
) : ICompoundComponent<BlockEntityDataComponent>
{
    /// <inheritdoc/>
    public static string ComponentId => "minecraft:block_entity_data";

    /// <summary>
    /// Parses a <see cref="BlockEntityDataComponent"/> directly from an SNBT compound node.
    /// </summary>
    /// <param name="compound">The SNBT compound node containing the block entity data.</param>
    /// <returns>A populated <see cref="BlockEntityDataComponent"/> instance.</returns>
    /// <example>
    /// <code>
    /// var node = SnbtParser.Parse("{Command: \"/say Hello!\"}");
    /// var component = BlockEntityDataComponent.Parse((SnbtCompound)node);
    /// </code>
    /// </example>
    public static BlockEntityDataComponent Parse(SnbtCompound compound)
    {
        return new BlockEntityDataComponent(compound);
    }

    /// <summary>
    /// Serializes the component by returning the underlying SNBT compound directly.
    /// </summary>
    /// <returns>An <see cref="ISnbtNode"/> representing the block entity data.</returns>
    public ISnbtNode ToSnbt() => Data;
}