using Core.DataComponents.Interfaces;
using Core.DataComponents.Models;
using Core.SNBT;

using Core.SNBT.Interfaces;
using Core.SNBT.Nodes;
using JetBrains.Annotations;

namespace Core.DataComponents.Components;

/// <summary>
/// Configures consumable item mechanics such as eating duration, animations, sounds, particles, and on-consume effects (<c>minecraft:consumable</c>).
/// </summary>
/// <param name="ConsumeSeconds">The duration in seconds required to finish consuming the item. Defaults to 1.6.</param>
/// <param name="Animation">The usage animation played during consumption (e.g. <c>eat</c>, <c>drink</c>, <c>bow</c>, <c>spear</c>). Defaults to <c>eat</c>.</param>
/// <param name="Sound">Optional sound event played while consuming the item.</param>
/// <param name="HasConsumeParticles">Whether particle effects are emitted while the item is being consumed. Defaults to <see langword="true"/>.</param>
/// <param name="OnConsumeEffects">Optional list of actions and status effects triggered upon completing consumption.</param>
[UsedImplicitly]
public record ConsumableComponent(
    float ConsumeSeconds = 1.6f,
    string Animation = ConsumableComponent.AnimationEat,
    SoundEvent? Sound = null,
    bool HasConsumeParticles = true,
    List<ConsumeEffect>? OnConsumeEffects = null
) : ICompoundComponent<ConsumableComponent>
{
    [PublicAPI] public const string AnimationEat = "eat";
    [PublicAPI] public const string AnimationDrink = "drink";
    [PublicAPI] public const string AnimationBlock = "block";
    [PublicAPI] public const string AnimationBow = "bow";
    [PublicAPI] public const string AnimationSpear = "spear";
    [PublicAPI] public const string AnimationCrossbow = "crossbow";
    [PublicAPI] public const string AnimationSpyglass = "spyglass";
    [PublicAPI] public const string AnimationTootHorn = "toot_horn";
    [PublicAPI] public const string AnimationBrush = "brush";
    [PublicAPI] public const string AnimationNone = "none";

    /// <inheritdoc/>
    public static string ComponentId => "minecraft:consumable";

    /// <summary>
    /// Parses a <see cref="ConsumableComponent"/> directly from an SNBT compound node.
    /// </summary>
    /// <param name="compound">The SNBT compound node containing consumable configuration data.</param>
    /// <returns>A populated <see cref="ConsumableComponent"/> instance.</returns>
    /// <example>
    /// <code>
    /// var node = SnbtParser.Parse("{consume_seconds: 2.0f, animation: 'drink', sound: 'entity.generic.drink'}");
    /// var component = ConsumableComponent.Parse((SnbtCompound)node);
    /// </code>
    /// </example>
    public static ConsumableComponent Parse(SnbtCompound compound)
    {
        List<ConsumeEffect>? effects = null;
        if (compound.GetNode("on_consume_effects") is SnbtList effList)
        {
            effects = new List<ConsumeEffect>(effList.Items.Count);
            foreach (var item in effList.Items)
            {
                if (item is SnbtCompound effComp && ConsumeEffect.Parse(effComp) is { } effect)
                {
                    effects.Add(effect);
                }
            }
        }

        var sound = compound.GetNode("sound") is { } sNode
            ? SoundEvent.Parse(sNode)
            : null;

        return new ConsumableComponent(
            ConsumeSeconds: compound.GetFloat("consume_seconds", 1.6f),
            Animation: compound.GetString("animation", AnimationEat),
            Sound: sound,
            HasConsumeParticles: compound.GetBool("has_consume_particles", true),
            OnConsumeEffects: effects
        );
    }

    /// <summary>
    /// Serializes the consumable properties into an SNBT compound node.
    /// </summary>
    /// <returns>An <see cref="ISnbtNode"/> representing the consumable compound.</returns>
    public ISnbtNode ToSnbt()
    {
        var builder = Snbt.Compound()
            .PutOptional("consume_seconds", ConsumeSeconds, 1.6f)
            .PutOptional("animation", Animation, AnimationEat)
            .PutOptional("has_consume_particles", HasConsumeParticles, true);

        if (Sound != null)
        {
            builder.Put("sound", Sound.ToSnbt());
        }

        if (OnConsumeEffects is { Count: > 0 })
        {
            builder.PutList("on_consume_effects", list =>
            {
                foreach (var effect in OnConsumeEffects)
                {
                    list.Add(effect.ToSnbt());
                }
            });
        }

        return builder.Build();
    }
}