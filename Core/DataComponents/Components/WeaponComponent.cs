
using Core.SNBT;
using Core.SNBT.Nodes;

using Core.SNBT.Interfaces;
using JetBrains.Annotations;

using Core.DataComponents.Interfaces;

namespace Core.DataComponents.Components;

/// <summary>
/// Configures an item to act as a weapon with durability damage per attack and shield-disabling capabilities (<c>minecraft:weapon</c>).
/// </summary>
/// <remarks>
/// Attack damage attributes are handled separately via the <c>minecraft:attribute_modifiers</c> component.
/// </remarks>
/// <param name="ItemDamagePerAttack">The durability damage applied to the item on each attack. Defaults to 1.</param>
/// <param name="DisableBlockingForSeconds">The duration in seconds that shield blocking is disabled on hit. Defaults to 0.0 (no disabling).</param>
[UsedImplicitly]
public record WeaponComponent(
    int ItemDamagePerAttack = 1,
    float DisableBlockingForSeconds = 0.0f
) : ICompoundComponent<WeaponComponent>
{
    /// <inheritdoc/>
    public static string ComponentId => "minecraft:weapon";

    /// <summary>
    /// Parses a <see cref="WeaponComponent"/> from an SNBT node representation.
    /// </summary>
    /// <param name="compound">The SNBT node to parse, which must be an <see cref="SnbtCompound"/>.</param>
    /// <returns>A populated <see cref="WeaponComponent"/> instance.</returns>
    /// <example>
    /// <code>
    /// var node = SnbtParser.Parse("{item_damage_per_attack: 10, disable_blocking_for_seconds: 5.0f}");
    /// var component = WeaponComponent.Parse(node);
    /// </code>
    /// </example>
    public static WeaponComponent Parse(SnbtCompound compound)
    {

        return new WeaponComponent(
            ItemDamagePerAttack: compound.GetInt("item_damage_per_attack", 1),
            DisableBlockingForSeconds: compound.GetFloat("disable_blocking_for_seconds")
        );
    }

    /// <summary>
    /// Serializes the component into an SNBT compound node.
    /// </summary>
    /// <returns>An <see cref="ISnbtNode"/> representing the weapon configuration.</returns>
    public ISnbtNode ToSnbt() => Snbt.Compound()
        .PutOptional("item_damage_per_attack", ItemDamagePerAttack, 1)
        .PutOptional("disable_blocking_for_seconds", DisableBlockingForSeconds, 0.0f)
        .Build();
}