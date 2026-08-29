using Core.DataComponents.Models;
using Core.SNBT;
using Core.SNBT.Nodes;

using Core.SNBT.Interfaces;
using JetBrains.Annotations;

using Core.DataComponents.Interfaces;

namespace Core.DataComponents.Components;

/// <summary>
/// Specifies the base potion type, custom mob status effects, and liquid color of a potion or tipped projectile (<c>minecraft:potion_contents</c>).
/// </summary>
/// <param name="Potion">Optional resource location of the base potion preset (e.g. <c>minecraft:long_invisibility</c>).</param>
/// <param name="CustomColor">Optional overriding packed 24-bit RGB integer color for liquid and area cloud particles (<c>0xRRGGBB</c>).</param>
/// <param name="CustomName">Optional translation key suffix to generate item name (<c>item.minecraft.&lt;type&gt;.effect.&lt;value&gt;</c>).</param>
/// <param name="CustomEffects">Optional list of additional custom status effects applied upon consumption.</param>
[UsedImplicitly]
public record PotionContentsComponent(
    string? Potion = null,
    int? CustomColor = null,
    string? CustomName = null,
    List<CustomEffect>? CustomEffects = null
) : IParsableComponent<PotionContentsComponent>
{
    /// <inheritdoc/>
    public static string ComponentId => "minecraft:potion_contents";

    /// <summary>
    /// Initializes a new instance of the <see cref="PotionContentsComponent"/> record referencing only a base potion type.
    /// </summary>
    /// <param name="potion">The base potion resource location.</param>
    public PotionContentsComponent(string potion) : this(Potion: potion, CustomColor: null, CustomName: null, CustomEffects: null)
    {
    }

    /// <summary>
    /// Parses a <see cref="PotionContentsComponent"/> from an SNBT node representation.
    /// Supports both direct potion string identifiers and full compound configurations.
    /// </summary>
    /// <param name="node">The SNBT node to parse (<see cref="SnbtString"/> or <see cref="SnbtCompound"/>).</param>
    /// <returns>A populated <see cref="PotionContentsComponent"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="node"/> is neither a string nor a compound.</exception>
    /// <example>
    /// <code>
    /// // From simple potion ID string:
    /// var comp1 = PotionContentsComponent.Parse(new SnbtString("minecraft:swiftness"));
    ///
    /// // From compound with custom effects:
    /// var node = SnbtParser.Parse("{potion: \"minecraft:water\", custom_color: 16711680, custom_effects: [{id: \"minecraft:speed\", amplifier: 1b, duration: 600}]}");
    /// var comp2 = PotionContentsComponent.Parse(node);
    /// </code>
    /// </example>
    public static PotionContentsComponent Parse(ISnbtNode node)
    {
        return node switch
        {
            SnbtString str => new PotionContentsComponent(str.Value),
            SnbtCompound compound => new PotionContentsComponent(
                Potion: compound.GetOptionalString("potion"),
                CustomColor: compound.GetOptionalInt("custom_color"),
                CustomName: compound.GetOptionalString("custom_name"),
                CustomEffects: ParseEffectsList(compound.GetNode("custom_effects"))
            ),
            _ => throw new ArgumentException("Potion contents component must be a string or compound.")
        };
    }

    /// <summary>
    /// Serializes the component into an SNBT node.
    /// Emits a compact <see cref="SnbtString"/> if only <see cref="Potion"/> is set.
    /// </summary>
    /// <returns>An <see cref="ISnbtNode"/> representing the potion configuration.</returns>
    public ISnbtNode ToSnbt()
    {
        if (CustomColor == null && CustomName == null && (CustomEffects == null || CustomEffects.Count == 0))
        {
            if (Potion != null)
                return new SnbtString(Potion);
        }

        var builder = Snbt.Compound()
            .PutOptional("potion", Potion)
            .PutOptional("custom_color", CustomColor)
            .PutOptional("custom_name", CustomName);

        if (CustomEffects is { Count: > 0 })
        {
            builder.PutList("custom_effects", list =>
            {
                foreach (var effect in CustomEffects)
                    list.Add(effect.ToSnbt());
            });
        }

        return builder.Build();
    }

    /// <summary>
    /// Implicitly converts a potion identifier string into a <see cref="PotionContentsComponent"/>.
    /// </summary>
    /// <param name="potionId">The base potion resource location.</param>
    public static implicit operator PotionContentsComponent(string potionId) => new(potionId);

    private static List<CustomEffect>? ParseEffectsList(ISnbtNode? node)
    {
        if (node is not SnbtList list)
            return null;

        var effects = new List<CustomEffect>(list.Items.Count);
        foreach (var item in list.Items)
        {
            if (item is SnbtCompound effectCompound)
                effects.Add(CustomEffect.Parse(effectCompound));
        }

        return effects;
    }
}