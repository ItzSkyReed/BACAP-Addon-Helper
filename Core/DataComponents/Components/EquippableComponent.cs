 using Core.DataComponents.Models;
using Core.SNBT;
using Core.SNBT.Nodes;

using Core.SNBT.Interfaces;
using JetBrains.Annotations;

using Core.DataComponents.Interfaces;

namespace Core.DataComponents.Components;

/// <summary>
/// Configures equipment properties for an item, defining its equip slot, model, interactions, and sounds (<c>minecraft:equippable</c>).
/// </summary>
/// <param name="Slot">The equipment slot where the item can be worn (<c>head</c>, <c>chest</c>, <c>legs</c>, <c>feet</c>, <c>body</c>, <c>mainhand</c>, <c>offhand</c>, or <c>saddle</c>).</param>
/// <param name="EquipSound">Optional sound event played when equipping.</param>
/// <param name="AssetId">Optional equipment asset identifier referring to <c>assets/&lt;namespace&gt;/equipment/&lt;id&gt;.json</c>.</param>
/// <param name="AllowedEntities">Optional list of entity identifiers or tag selectors (prefixed with <c>#</c>) permitted to equip this item.</param>
/// <param name="Dispensable">Whether this item can be equipped onto entities via a dispenser. Defaults to <see langword="true"/>.</param>
/// <param name="Swappable">Whether right-clicking the item swaps it into the target slot. Defaults to <see langword="true"/>.</param>
/// <param name="DamageOnHurt">Whether the item loses durability when the wearing entity takes damage. Defaults to <see langword="true"/>.</param>
/// <param name="EquipOnInteract">Whether right-clicking a target entity directly equips this item onto it. Defaults to <see langword="false"/>.</param>
/// <param name="CameraOverlay">Optional texture identifier for a HUD overlay applied when equipped.</param>
/// <param name="CanBeSheared">Whether the item can be sheared off a wearing entity using shears. Defaults to <see langword="false"/>.</param>
/// <param name="ShearingSound">Optional sound event played when shearing this item off an entity.</param>
[UsedImplicitly]
public record EquippableComponent(
    string Slot,
    SoundEvent? EquipSound = null,
    string? AssetId = null,
    List<string>? AllowedEntities = null,
    bool Dispensable = true,
    bool Swappable = true,
    bool DamageOnHurt = true,
    bool EquipOnInteract = false,
    string? CameraOverlay = null,
    bool CanBeSheared = false,
    SoundEvent? ShearingSound = null
) : ICompoundComponent<EquippableComponent>
{
    /// <inheritdoc/>
    public static string ComponentId => "minecraft:equippable";

    /// <summary>
    /// Parses an <see cref="EquippableComponent"/> from an SNBT node representation.
    /// </summary>
    /// <param name="compound">The SNBT node to parse, which must be an <see cref="SnbtCompound"/>.</param>
    /// <returns>A populated <see cref="EquippableComponent"/> instance.</returns>
    public static EquippableComponent Parse(SnbtCompound compound)
    {

        List<string>? allowedEntities = null;
        var allowedNode = compound.GetNode("allowed_entities");

        switch (allowedNode)
        {
            case SnbtString singleEntity:
                allowedEntities = [singleEntity.Value];
                break;

            case SnbtList entityList:
            {
                allowedEntities = new List<string>(entityList.Items.Count);
                foreach (var item in entityList.Items)
                    if (item is SnbtString s) allowedEntities.Add(s.Value);

                break;
            }
        }

        var equipSoundNode = compound.GetNode("equip_sound");
        var shearingSoundNode = compound.GetNode("shearing_sound");

        return new EquippableComponent(
            Slot: compound.GetString("slot"),
            EquipSound: equipSoundNode != null ? SoundEvent.Parse(equipSoundNode) : null,
            AssetId: compound.GetOptionalString("asset_id"),
            AllowedEntities: allowedEntities,
            Dispensable: compound.GetBool("dispensable", true),
            Swappable: compound.GetBool("swappable", true),
            DamageOnHurt: compound.GetBool("damage_on_hurt", true),
            EquipOnInteract: compound.GetBool("equip_on_interact"),
            CameraOverlay: compound.GetOptionalString("camera_overlay"),
            CanBeSheared: compound.GetBool("can_be_sheared"),
            ShearingSound: shearingSoundNode != null ? SoundEvent.Parse(shearingSoundNode) : null
        );
    }

    /// <summary>
    /// Serializes the component into an SNBT compound node.
    /// </summary>
    /// <returns>An <see cref="ISnbtNode"/> representing the equippable component compound.</returns>
    public ISnbtNode ToSnbt()
    {
        var builder = Snbt.Compound()
            .Put("slot", Slot)
            .PutOptional("dispensable", Dispensable, true)
            .PutOptional("swappable", Swappable, true)
            .PutOptional("damage_on_hurt", DamageOnHurt, true)
            .PutOptional("equip_on_interact", EquipOnInteract, false)
            .PutOptional("can_be_sheared", CanBeSheared, false)
            .PutOptional("asset_id", AssetId)
            .PutOptional("camera_overlay", CameraOverlay);

        if (EquipSound != null)
            builder.Put("equip_sound", EquipSound.ToSnbt());
        if (ShearingSound != null)
            builder.Put("shearing_sound", ShearingSound.ToSnbt());

        if (AllowedEntities is not { Count: > 0 })
            return builder.Build();

        if (AllowedEntities.Count == 1)
            builder.Put("allowed_entities", AllowedEntities[0]);
        else
            builder.PutList("allowed_entities", list => list.AddRange(AllowedEntities));

        return builder.Build();
    }
}