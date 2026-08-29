using Core.DataComponents.Models;
using Core.SNBT;
using Core.SNBT.Nodes;

using Core.SNBT.Interfaces;
using JetBrains.Annotations;

using Core.DataComponents.Interfaces;

namespace Core.DataComponents.Components;

/// <summary>
/// Configures an item to perform sweeping raycast melee attacks damaging multiple entities in a line (<c>minecraft:piercing_weapon</c>).
/// Also prevents the item from being used to mine blocks.
/// </summary>
/// <param name="DealsKnockback">Whether the attack applies knockback to affected targets. Defaults to <see langword="true"/>.</param>
/// <param name="Dismounts">Whether the attack dismounts affected targets from vehicles or mounts. Defaults to <see langword="false"/>.</param>
/// <param name="Sound">Optional sound event played when the player attacks with the weapon.</param>
/// <param name="HitSound">Optional sound event played when the piercing ray hits an entity.</param>
[UsedImplicitly]
public record PiercingWeaponComponent(
    bool DealsKnockback = true,
    bool Dismounts = false,
    SoundEvent? Sound = null,
    SoundEvent? HitSound = null
) : ICompoundComponent<PiercingWeaponComponent>
{
    /// <inheritdoc/>
    public static string ComponentId => "minecraft:piercing_weapon";

    /// <summary>
    /// Parses a <see cref="PiercingWeaponComponent"/> from an SNBT node representation.
    /// </summary>
    /// <param name="compound">The SNBT node to parse, which must be an <see cref="SnbtCompound"/>.</param>
    /// <returns>A populated <see cref="PiercingWeaponComponent"/> instance.</returns>
    /// <example>
    /// <code>
    /// var node = SnbtParser.Parse("{sound: \"entity.blaze.hurt\", hit_sound: \"entity.lightning_bolt.impact\", dismounts: 1b}");
    /// var component = PiercingWeaponComponent.Parse(node);
    /// </code>
    /// </example>
    public static PiercingWeaponComponent Parse(SnbtCompound compound)
    {

        var soundNode = compound.GetNode("sound");
        var hitSoundNode = compound.GetNode("hit_sound");

        return new PiercingWeaponComponent(
            DealsKnockback: compound.GetBool("deals_knockback", true),
            Dismounts: compound.GetBool("dismounts"),
            Sound: soundNode != null ? SoundEvent.Parse(soundNode) : null,
            HitSound: hitSoundNode != null ? SoundEvent.Parse(hitSoundNode) : null
        );
    }

    /// <summary>
    /// Serializes the component into an SNBT compound node.
    /// </summary>
    /// <returns>An <see cref="ISnbtNode"/> representing the piercing weapon compound.</returns>
    public ISnbtNode ToSnbt()
    {
        var builder = Snbt.Compound()
            .PutOptional("deals_knockback", DealsKnockback, true)
            .PutOptional("dismounts", Dismounts, false);

        if (Sound != null) builder.Put("sound", Sound.ToSnbt());
        if (HitSound != null) builder.Put("hit_sound", HitSound.ToSnbt());

        return builder.Build();
    }
}