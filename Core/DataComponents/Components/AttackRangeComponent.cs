using Core.DataComponents.Interfaces;
using Core.SNBT;

using Core.SNBT.Interfaces;
using Core.SNBT.Nodes;
using JetBrains.Annotations;

namespace Core.DataComponents.Components;

/// <summary>
/// Configures entity melee attack reach distances, creative reach modifiers, hitbox expansion margins, and mob scaling factors (<c>minecraft:attack_range</c>).
/// </summary>
/// <param name="MinReach">The minimum distance in blocks required to perform an attack in Survival/Adventure mode. Defaults to 0.0.</param>
/// <param name="MaxReach">The maximum attack reach distance in blocks in Survival/Adventure mode. Defaults to 3.0.</param>
/// <param name="MinCreativeReach">The minimum attack distance in blocks required while in Creative mode. Defaults to 0.0.</param>
/// <param name="MaxCreativeReach">The maximum attack reach distance in blocks while in Creative mode. Defaults to 5.0.</param>
/// <param name="HitboxMargin">The bounding box expansion margin in blocks considered valid for registering hits. Defaults to 0.3.</param>
/// <param name="MobFactor">The reach distance multiplier applied when non-player entities wield this item. Defaults to 1.0.</param>
[UsedImplicitly]
public record AttackRangeComponent(
    float MinReach = 0.0f,
    float MaxReach = 3.0f,
    float MinCreativeReach = 0.0f,
    float MaxCreativeReach = 5.0f,
    float HitboxMargin = 0.3f,
    float MobFactor = 1.0f
) : ICompoundComponent<AttackRangeComponent>
{
    /// <inheritdoc/>
    public static string ComponentId => "minecraft:attack_range";

    /// <summary>
    /// Parses an <see cref="AttackRangeComponent"/> from an SNBT node representation.
    /// </summary>
    /// <param name="compound">The SNBT node to parse, which must be an <see cref="SnbtCompound"/>.</param>
    /// <returns>A populated <see cref="AttackRangeComponent"/> instance.</returns>
    /// <example>
    /// <code>
    /// var node = SnbtParser.Parse("{max_reach: 4.5f, hitbox_margin: 0.5f}");
    /// var attackRange = AttackRangeComponent.Parse(node);
    /// </code>
    /// </example>
    public static AttackRangeComponent Parse(SnbtCompound compound)
    {

        return new AttackRangeComponent(
            MinReach: compound.GetFloat("min_reach"),
            MaxReach: compound.GetFloat("max_reach", 3.0f),
            MinCreativeReach: compound.GetFloat("min_creative_reach"),
            MaxCreativeReach: compound.GetFloat("max_creative_reach", 5.0f),
            HitboxMargin: compound.GetFloat("hitbox_margin", 0.3f),
            MobFactor: compound.GetFloat("mob_factor", 1.0f)
        );
    }

    /// <summary>
    /// Serializes the component into an SNBT compound node, omitting fields that match default values.
    /// </summary>
    /// <returns>An <see cref="ISnbtNode"/> representing the attack range configuration compound.</returns>
    public ISnbtNode ToSnbt() => Snbt.Compound()
        .PutOptional("min_reach", MinReach, 0.0f)
        .PutOptional("max_reach", MaxReach, 3.0f)
        .PutOptional("min_creative_reach", MinCreativeReach, 0.0f)
        .PutOptional("max_creative_reach", MaxCreativeReach, 5.0f)
        .PutOptional("hitbox_margin", HitboxMargin, 0.3f)
        .PutOptional("mob_factor", MobFactor, 1.0f)
        .Build();
}