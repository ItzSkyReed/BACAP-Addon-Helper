using Core.DataComponents.Interfaces;
using Core.SNBT;

using Core.SNBT.Interfaces;
using Core.SNBT.Nodes;
using JetBrains.Annotations;

namespace Core.DataComponents.Components;

/// <summary>
/// Stores persistent entity NBT data applied to fish, axolotls, or tadpoles when released from a bucket (<c>minecraft:bucket_entity_data</c>).
/// </summary>
/// <param name="NoAi">Whether the spawned mob's AI logic should be disabled.</param>
/// <param name="Silent">Whether the spawned mob produces no ambient, hurt, or death sounds.</param>
/// <param name="NoGravity">Whether gravitational physics should be disabled for the spawned mob.</param>
/// <param name="Glowing">Whether the spawned mob emits a persistent spectral outline effect.</param>
/// <param name="Invulnerable">Whether the spawned mob is immune to standard incoming damage sources.</param>
/// <param name="AgeLocked">Whether the spawned baby mob is prevented from growing into an adult.</param>
/// <param name="Health">The current health points preserved for the mob.</param>
/// <param name="Age">The age of the entity in ticks (negative for babies, zero or positive for adults).</param>
/// <param name="HuntingCooldown">The remaining cooldown ticks before an axolotl hunts again.</param>
[UsedImplicitly]
public record BucketEntityDataComponent(
    bool? NoAi = null,
    bool? Silent = null,
    bool? NoGravity = null,
    bool? Glowing = null,
    bool? Invulnerable = null,
    bool? AgeLocked = null,
    float? Health = null,
    int? Age = null,
    long? HuntingCooldown = null
) : ICompoundComponent<BucketEntityDataComponent>
{
    /// <inheritdoc/>
    public static string ComponentId => "minecraft:bucket_entity_data";

    /// <summary>
    /// Parses a <see cref="BucketEntityDataComponent"/> directly from an SNBT compound node.
    /// </summary>
    /// <param name="compound">The SNBT compound node containing preserved entity tags.</param>
    /// <returns>A populated <see cref="BucketEntityDataComponent"/> instance.</returns>
    /// <example>
    /// <code>
    /// var node = SnbtParser.Parse("{NoAI: 1b, Health: 14.0f, Age: -24000}");
    /// var component = BucketEntityDataComponent.Parse((SnbtCompound)node);
    /// </code>
    /// </example>
    public static BucketEntityDataComponent Parse(SnbtCompound compound)
    {
        return new BucketEntityDataComponent(
            NoAi: compound.GetOptionalBool("NoAI"),
            Silent: compound.GetOptionalBool("Silent"),
            NoGravity: compound.GetOptionalBool("NoGravity"),
            Glowing: compound.GetOptionalBool("Glowing"),
            Invulnerable: compound.GetOptionalBool("Invulnerable"),
            AgeLocked: compound.GetOptionalBool("AgeLocked"),
            Health: compound.GetOptionalFloat("Health"),
            Age: compound.GetOptionalInt("Age"),
            HuntingCooldown: compound.GetOptionalLong("HuntingCooldown")
        );
    }

    /// <summary>
    /// Serializes the entity bucket data into an SNBT compound node.
    /// </summary>
    /// <returns>An <see cref="ISnbtNode"/> representing the entity NBT state compound.</returns>
    public ISnbtNode ToSnbt() => Snbt.Compound()
        .PutOptional("NoAI", NoAi)
        .PutOptional("Silent", Silent)
        .PutOptional("NoGravity", NoGravity)
        .PutOptional("Glowing", Glowing)
        .PutOptional("Invulnerable", Invulnerable)
        .PutOptional("AgeLocked", AgeLocked)
        .PutOptional("Health", Health)
        .PutOptional("Age", Age)
        .PutOptional("HuntingCooldown", HuntingCooldown)
        .Build();
}