using Core.DataComponents.Interfaces;
using Core.DataComponents.Models;
using Core.DataComponents.Models.Extensions;
using Core.SNBT;

using Core.SNBT.Interfaces;
using Core.SNBT.Nodes;
using JetBrains.Annotations;

namespace Core.DataComponents.Components;

/// <summary>
/// Configures attack blocking mechanics, damage absorption rules, disable cooldown scaling, and sound events (<c>minecraft:blocks_attacks</c>).
/// </summary>
/// <param name="BlockDelaySeconds">The delay in seconds after raising the item before blocking takes effect. Defaults to 0.0.</param>
/// <param name="DisableCooldownScale">Multiplier applied to the cooldown penalty when the block is broken (e.g. by an axe). Defaults to 1.0.</param>
/// <param name="DamageReductions">Optional list of specific damage absorption rules and angle thresholds.</param>
/// <param name="ItemDamage">Optional rules determining how much durability the item loses when blocking damage.</param>
/// <param name="BlockSound">Optional sound event played when successfully blocking an incoming attack.</param>
/// <param name="DisabledSound">Optional sound event played when blocking is disabled by a heavy attack.</param>
/// <param name="BypassedBy">Optional damage type tag or identifier that bypasses this block completely (e.g. <c>#minecraft:bypasses_shield</c>).</param>
[UsedImplicitly]
public record BlocksAttacksComponent(
    float BlockDelaySeconds = 0.0f,
    float DisableCooldownScale = 1.0f,
    List<DamageReduction>? DamageReductions = null,
    BlocksAttacksItemDamage? ItemDamage = null,
    SoundEvent? BlockSound = null,
    SoundEvent? DisabledSound = null,
    string? BypassedBy = null
) : ICompoundComponent<BlocksAttacksComponent>
{
    /// <inheritdoc/>
    public static string ComponentId => "minecraft:blocks_attacks";

    /// <summary>
    /// Parses a <see cref="BlocksAttacksComponent"/> directly from an SNBT compound node.
    /// </summary>
    /// <param name="compound">The SNBT compound node containing blocking properties.</param>
    /// <returns>A populated <see cref="BlocksAttacksComponent"/> instance.</returns>
    /// <example>
    /// <code>
    /// var node = SnbtParser.Parse("{block_delay_seconds: 0.1f, block_sound: 'item.shield.block'}");
    /// var component = BlocksAttacksComponent.Parse((SnbtCompound)node);
    /// </code>
    /// </example>
    public static BlocksAttacksComponent Parse(SnbtCompound compound)
    {
        var itemDamage = compound.GetNode("item_damage") is SnbtCompound dmgComp
            ? BlocksAttacksItemDamage.Parse(dmgComp)
            : null;

        var blockSound = compound.GetNode("block_sound") is { } bsNode
            ? SoundEvent.Parse(bsNode)
            : null;

        var disabledSound = compound.GetNode("disabled_sound") is { } dsNode
            ? SoundEvent.Parse(dsNode)
            : null;

        return new BlocksAttacksComponent(
            BlockDelaySeconds: compound.GetFloat("block_delay_seconds"),
            DisableCooldownScale: compound.GetFloat("disable_cooldown_scale", 1.0f),
            DamageReductions: compound.GetCompoundList<DamageReduction>("damage_reductions"),
            ItemDamage: itemDamage,
            BlockSound: blockSound,
            DisabledSound: disabledSound,
            BypassedBy: compound.GetOptionalString("bypassed_by")
        );
    }

    /// <summary>
    /// Serializes the component into an SNBT compound node.
    /// </summary>
    /// <returns>An <see cref="ISnbtNode"/> representing the attack blocking configuration.</returns>
    public ISnbtNode ToSnbt()
    {
        var builder = Snbt.Compound()
            .PutOptional("block_delay_seconds", BlockDelaySeconds, 0.0f)
            .PutOptional("disable_cooldown_scale", DisableCooldownScale, 1.0f)
            .PutOptional("bypassed_by", BypassedBy)
            .PutModels("damage_reductions", DamageReductions);

        if (ItemDamage != null)
            builder.Put("item_damage", ItemDamage.ToSnbt());

        if (BlockSound != null)
            builder.Put("block_sound", BlockSound.ToSnbt());

        if (DisabledSound != null)
            builder.Put("disabled_sound", DisabledSound.ToSnbt());

        return builder.Build();
    }
}