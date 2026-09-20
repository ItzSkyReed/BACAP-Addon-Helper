using Core.DataComponents.Models;
using Core.SNBT.Interfaces;
using Core.SNBT.Nodes;
using JetBrains.Annotations;

using Core.DataComponents.Interfaces;

namespace Core.DataComponents.Components;

/// <summary>
/// Specifies the list of unamplified mob effects granted upon consuming a suspicious stew (<c>minecraft:suspicious_stew_effects</c>).
/// </summary>
/// <param name="Effects">The list of status effects applied upon consumption.</param>
[UsedImplicitly]
public record SuspiciousStewEffectsComponent(
    List<SuspiciousStewEffect> Effects
) : IListComponent<SuspiciousStewEffectsComponent>
{
    /// <inheritdoc/>
    public static string ComponentId => "minecraft:suspicious_stew_effects";

    /// <summary>
    /// Initializes a new instance of the <see cref="SuspiciousStewEffectsComponent"/> record from params of <see cref="SuspiciousStewEffect"/>.
    /// </summary>
    /// <param name="effects">The effects to apply.</param>
    public SuspiciousStewEffectsComponent(params SuspiciousStewEffect[] effects) : this(effects.ToList())
    {
    }

    /// <summary>
    /// Parses a <see cref="SuspiciousStewEffectsComponent"/> from an SNBT node representation.
    /// </summary>
    /// <param name="list">The SNBT node to parse, which must be an <see cref="SnbtList"/>.</param>
    /// <returns>A populated <see cref="SuspiciousStewEffectsComponent"/> instance.</returns>
    /// <example>
    /// <code>
    /// var node = SnbtParser.Parse("[{id: \"minecraft:night_vision\", duration: 200}, {id: \"minecraft:blindness\", duration: 160}]");
    /// var component = SuspiciousStewEffectsComponent.Parse(node);
    /// </code>
    /// </example>
    public static SuspiciousStewEffectsComponent Parse(SnbtList list)
    {

        var effects = new List<SuspiciousStewEffect>(list.Items.Count);
        foreach (var item in list.Items)
        {
            if (item is SnbtCompound comp)
                effects.Add(SuspiciousStewEffect.Parse(comp));
        }

        return new SuspiciousStewEffectsComponent(effects);
    }

    /// <summary>
    /// Serializes the component into an SNBT list node.
    /// </summary>
    /// <returns>An <see cref="ISnbtNode"/> containing the list of stew effects.</returns>
    public ISnbtNode ToSnbt()
    {
        var items = new List<ISnbtNode>(Effects.Count);
        foreach (var effect in Effects)
            items.Add(effect.ToSnbt());

        return new SnbtList(items);
    }
}