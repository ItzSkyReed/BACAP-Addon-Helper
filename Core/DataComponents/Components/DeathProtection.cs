using Core.DataComponents.Models;
using Core.SNBT;
using Core.SNBT.Nodes;

using Core.SNBT.Interfaces;
using JetBrains.Annotations;

using Core.DataComponents.Interfaces;

namespace Core.DataComponents.Components;

/// <summary>
/// Represents totem-like death protection configuration applied to an item (<c>minecraft:death_protection</c>).
/// Prevents lethal damage, restores 1 health point, and triggers configured effects upon activation.
/// </summary>
/// <param name="DeathEffects">Optional list of consume effects applied when the item prevents the holder's death.</param>
[UsedImplicitly]
public record DeathProtectionComponent(
    List<ConsumeEffect>? DeathEffects = null
) : ICompoundComponent<DeathProtectionComponent>
{
    /// <inheritdoc/>
    public static string ComponentId => "minecraft:death_protection";

    /// <summary>
    /// Parses a <see cref="DeathProtectionComponent"/> from an SNBT node representation.
    /// </summary>
    /// <param name="compound">The SNBT node to parse, which must be an <see cref="SnbtCompound"/>.</param>
    /// <returns>A populated <see cref="DeathProtectionComponent"/> instance.</returns>
    /// <example>
    /// <code>
    /// var node = SnbtParser.Parse("{death_effects: [{type: \"minecraft:clear_all_effects\"}]}");
    /// var component = DeathProtectionComponent.Parse(node);
    /// </code>
    /// </example>
    public static DeathProtectionComponent Parse(SnbtCompound compound)
    {

        List<ConsumeEffect>? effects = null;
        if (compound.GetNode("death_effects") is not SnbtList list)
            return new DeathProtectionComponent(effects);

        effects = new List<ConsumeEffect>(list.Items.Count);

        foreach (var item in list.Items)
        {
            if (item is SnbtCompound effComp && ConsumeEffect.Parse(effComp) is { } effect)
                effects.Add(effect);
        }

        return new DeathProtectionComponent(effects);
    }

    /// <summary>
    /// Serializes the component into an SNBT compound node.
    /// </summary>
    /// <returns>An <see cref="ISnbtNode"/> representing the death protection compound.</returns>
    public ISnbtNode ToSnbt()
    {
        var builder = Snbt.Compound();

        if (DeathEffects is { Count: > 0 })
        {
            builder.PutList("death_effects", list =>
            {
                foreach (var effect in DeathEffects)
                    list.Add(effect.ToSnbt());
            });
        }

        return builder.Build();
    }
}