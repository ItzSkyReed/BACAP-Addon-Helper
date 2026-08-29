using Core.DataComponents.Interfaces;
using Core.SNBT;

using Core.SNBT.Interfaces;
using Core.SNBT.Nodes;
using JetBrains.Annotations;

namespace Core.DataComponents.Components;

/// <summary>
/// Configures a procedural loot table and optional RNG seed used to populate a container block item when placed and opened (<c>minecraft:container_loot</c>).
/// </summary>
/// <param name="LootTable">The resource location identifier of the loot table (e.g. <c>minecraft:chests/simple_dungeon</c>).</param>
/// <param name="Seed">Optional predetermined RNG seed value used to generate deterministic loot contents.</param>
[UsedImplicitly]
public record ContainerLootComponent(
    string LootTable,
    long? Seed = null
) : ICompoundComponent<ContainerLootComponent>
{
    /// <inheritdoc/>
    public static string ComponentId => "minecraft:container_loot";

    /// <summary>
    /// Parses a <see cref="ContainerLootComponent"/> directly from an SNBT compound node.
    /// </summary>
    /// <param name="compound">The SNBT compound node containing loot table definition fields.</param>
    /// <returns>A populated <see cref="ContainerLootComponent"/> instance.</returns>
    /// <example>
    /// <code>
    /// var node = SnbtParser.Parse("{loot_table: 'minecraft:chests/desert_pyramid', seed: 42L}");
    /// var component = ContainerLootComponent.Parse((SnbtCompound)node);
    /// </code>
    /// </example>
    public static ContainerLootComponent Parse(SnbtCompound compound)
    {
        return new ContainerLootComponent(
            LootTable: compound.GetString("loot_table"),
            Seed: compound.GetOptionalLong("seed")
        );
    }

    /// <summary>
    /// Serializes the container loot settings into an SNBT compound node.
    /// </summary>
    /// <returns>An <see cref="ISnbtNode"/> representing the container loot compound.</returns>
    public ISnbtNode ToSnbt() => Snbt.Compound()
        .Put("loot_table", LootTable)
        .PutOptional("seed", Seed)
        .Build();

    /// <summary>
    /// Implicitly converts a loot table resource identifier string into a <see cref="ContainerLootComponent"/>.
    /// </summary>
    /// <param name="lootTable">The resource location of the loot table.</param>
    public static implicit operator ContainerLootComponent(string lootTable) => new(lootTable);

    /// <inheritdoc/>
    public override string ToString() => LootTable;
}