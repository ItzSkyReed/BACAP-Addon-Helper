
using Core.DataComponents.Models;
using Core.SNBT;
using Core.SNBT.Nodes;

using Core.SNBT.Interfaces;
using JetBrains.Annotations;

using Core.DataComponents.Interfaces;

namespace Core.DataComponents.Components;

/// <summary>
/// Enables a charge-type attack where damage, dismount, and knockback are calculated along a ray per tick based on relative entity velocity (<c>minecraft:kinetic_weapon</c>).
/// </summary>
/// <param name="DelayTicks">The time in ticks required before the weapon becomes effective. Defaults to 0.</param>
/// <param name="DamageConditions">Optional conditions under which the charge attack deals damage.</param>
/// <param name="DismountConditions">Optional conditions under which the charge attack dismounts target entities.</param>
/// <param name="KnockbackConditions">Optional conditions under which the charge attack applies knockback.</param>
/// <param name="ForwardMovement">The distance in blocks the item moves forward during the charge animation. Defaults to 0.0.</param>
/// <param name="DamageMultiplier">The damage scaling multiplier applied to the relative velocity calculation. Defaults to 1.0.</param>
/// <param name="Sound">Optional sound event played when the charge attack is engaged.</param>
/// <param name="HitSound">Optional sound event played when the charge attack successfully impacts an entity.</param>
[UsedImplicitly]
public record KineticWeaponComponent(
    int DelayTicks = 0,
    KineticWeaponConditions? DamageConditions = null,
    KineticWeaponConditions? DismountConditions = null,
    KineticWeaponConditions? KnockbackConditions = null,
    float ForwardMovement = 0.0f,
    float DamageMultiplier = 1.0f,
    SoundEvent? Sound = null,
    SoundEvent? HitSound = null
) : ICompoundComponent<KineticWeaponComponent>
{
    /// <inheritdoc/>
    public static string ComponentId => "minecraft:kinetic_weapon";

    /// <summary>
    /// Parses a <see cref="KineticWeaponComponent"/> from an SNBT node representation.
    /// </summary>
    /// <param name="compound">The SNBT node to parse, which must be an <see cref="SnbtCompound"/>.</param>
    /// <returns>A populated <see cref="KineticWeaponComponent"/> instance.</returns>
    /// <example>
    /// <code>
    /// var node = SnbtParser.Parse("{delay_ticks: 20, damage_conditions: {max_duration_ticks: 60}, hit_sound: \"block.amethyst_cluster.step\"}");
    /// var component = KineticWeaponComponent.Parse(node);
    /// </code>
    /// </example>
    public static KineticWeaponComponent Parse(SnbtCompound compound)
    {

        var soundNode = compound.GetNode("sound");
        var hitSoundNode = compound.GetNode("hit_sound");

        return new KineticWeaponComponent(
            DelayTicks: compound.GetInt("delay_ticks"),
            DamageConditions: compound.GetNode("damage_conditions") is SnbtCompound dmgComp ? KineticWeaponConditions.Parse(dmgComp) : null,
            DismountConditions: compound.GetNode("dismount_conditions") is SnbtCompound dismountComp ? KineticWeaponConditions.Parse(dismountComp) : null,
            KnockbackConditions: compound.GetNode("knockback_conditions") is SnbtCompound knockComp ? KineticWeaponConditions.Parse(knockComp) : null,
            ForwardMovement: compound.GetFloat("forward_movement"),
            DamageMultiplier: compound.GetFloat("damage_multiplier", 1.0f),
            Sound: soundNode != null ? SoundEvent.Parse(soundNode) : null,
            HitSound: hitSoundNode != null ? SoundEvent.Parse(hitSoundNode) : null
        );
    }

    /// <summary>
    /// Serializes the component into an SNBT compound node.
    /// </summary>
    /// <returns>An <see cref="ISnbtNode"/> representing the kinetic weapon configuration.</returns>
    public ISnbtNode ToSnbt()
    {
        var builder = Snbt.Compound()
            .PutOptional("delay_ticks", DelayTicks, 0)
            .PutOptional("forward_movement", ForwardMovement, 0.0f)
            .PutOptional("damage_multiplier", DamageMultiplier, 1.0f);

        if (DamageConditions != null) builder.Put("damage_conditions", DamageConditions.ToSnbt());
        if (DismountConditions != null) builder.Put("dismount_conditions", DismountConditions.ToSnbt());
        if (KnockbackConditions != null) builder.Put("knockback_conditions", KnockbackConditions.ToSnbt());
        if (Sound != null) builder.Put("sound", Sound.ToSnbt());
        if (HitSound != null) builder.Put("hit_sound", HitSound.ToSnbt());

        return builder.Build();
    }
}