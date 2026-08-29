using Core.DataComponents.Models.Interfaces;
using Core.SNBT;
using Core.SNBT.Nodes;

using Core.SNBT.Interfaces;

namespace Core.DataComponents.Models;

/// <summary>
/// Represents a Minecraft sound event reference or inline sound definition.
/// Can be defined as a direct sound identifier string or as a compound containing <c>sound_id</c> and an optional <c>range</c>.
/// </summary>
/// <param name="SoundId">The resource identifier of the client-side sound event (e.g., <c>minecraft:item.armor.equip_generic</c>).</param>
/// <param name="Range">Optional fixed distance range within which the sound is audible. If omitted, range is variable.</param>
public record SoundEvent(
    string SoundId,
    float? Range = null
) : IFlexibleModel<SoundEvent>
{
    /// <summary>
    /// Parses a <see cref="SoundEvent"/> from an SNBT node representation.
    /// </summary>
    /// <param name="node">The SNBT node (either <see cref="SnbtString"/> or <see cref="SnbtCompound"/>).</param>
    /// <returns>A populated <see cref="SoundEvent"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="node"/> is neither a string nor a sound compound.</exception>
    /// <example>
    /// <code>
    /// // From simple string ID:
    /// var sound1 = SoundEvent.Parse(new SnbtString("entity.generic.eat"));
    ///
    /// // From sound compound:
    /// var sound2 = SoundEvent.Parse(SnbtParser.Parse("{sound_id: \"entity.generic.eat\", range: 16.0f}"));
    /// </code>
    /// </example>
    public static SoundEvent Parse(ISnbtNode node)
    {
        return node switch
        {
            SnbtString str => new SoundEvent(str.Value),
            SnbtCompound compound => new SoundEvent(
                SoundId: compound.GetString("sound_id"),
                Range: compound.GetOptionalFloat("range")
            ),
            _ => throw new ArgumentException("Sound event must be either a string identifier or a compound.")
        };
    }

    /// <summary>
    /// Serializes the sound event into its most compact SNBT representation.
    /// If no custom <see cref="Range"/> is defined, serializes directly to <see cref="SnbtString"/>.
    /// </summary>
    /// <returns>An <see cref="ISnbtNode"/> containing the string or compound representation.</returns>
    public ISnbtNode ToSnbt()
    {
        if (Range == null)
            return new SnbtString(SoundId);

        return Snbt.Compound()
            .Put("sound_id", SoundId)
            .Put("range", Range.Value)
            .Build();
    }

    /// <summary>
    /// Implicitly converts a string identifier into a <see cref="SoundEvent"/>.
    /// </summary>
    /// <param name="soundId">The resource identifier of the sound.</param>
    public static implicit operator SoundEvent(string soundId) => new(soundId);
}