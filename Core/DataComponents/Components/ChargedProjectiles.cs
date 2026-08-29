using Core.DataComponents.Interfaces;
using Core.Items;
using Core.SNBT.Interfaces;
using Core.SNBT.Nodes;
using JetBrains.Annotations;

namespace Core.DataComponents.Components;

/// <summary>
/// Stores the projectile items currently loaded and ready to fire in a crossbow (<c>minecraft:charged_projectiles</c>).
/// </summary>
/// <param name="Projectiles">The list of charged projectile items (e.g. arrows, tipped arrows, firework rockets).</param>
[UsedImplicitly]
public record ChargedProjectilesComponent(
    List<ItemStack> Projectiles
) : IListComponent<ChargedProjectilesComponent>
{
    /// <inheritdoc/>
    public static string ComponentId => "minecraft:charged_projectiles";

    /// <summary>
    /// Initializes a new instance of the <see cref="ChargedProjectilesComponent"/> record with no charged projectiles.
    /// </summary>
    public ChargedProjectilesComponent() : this(new List<ItemStack>())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ChargedProjectilesComponent"/> record from an array of projectile items.
    /// </summary>
    /// <param name="projectiles">The projectile items loaded into the weapon.</param>
    public ChargedProjectilesComponent(params ItemStack[] projectiles)
        : this(projectiles.ToList())
    {
    }

    /// <summary>
    /// Parses a <see cref="ChargedProjectilesComponent"/> directly from an SNBT list node.
    /// Empty item stacks and air entries are automatically ignored.
    /// </summary>
    /// <param name="list">The SNBT list containing serialized item compounds.</param>
    /// <returns>A populated <see cref="ChargedProjectilesComponent"/> instance.</returns>
    /// <example>
    /// <code>
    /// var node = SnbtParser.Parse("[{id: 'minecraft:spectral_arrow', count: 1}]");
    /// var charged = ChargedProjectilesComponent.Parse((SnbtList)node);
    /// </code>
    /// </example>
    public static ChargedProjectilesComponent Parse(SnbtList list)
    {
        var projectiles = new List<ItemStack>(list.Items.Count);

        foreach (var entry in list.Items)
        {
            if (entry is not SnbtCompound compound)
                continue;

            var itemStack = ItemStack.Parse(compound);
            if (!itemStack.IsEmpty)
            {
                projectiles.Add(itemStack);
            }
        }

        return new ChargedProjectilesComponent(projectiles);
    }

    /// <summary>
    /// Serializes all non-empty charged projectiles into an SNBT list node.
    /// </summary>
    /// <returns>An <see cref="ISnbtNode"/> representing the list of charged projectile items.</returns>
    public ISnbtNode ToSnbt()
    {
        var list = new SnbtList();

        foreach (var item in Projectiles)
        {
            if (!item.IsEmpty)
            {
                list.Items.Add(item.ToSnbt());
            }
        }

        return list;
    }
}