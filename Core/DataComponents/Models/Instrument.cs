using Core.DataComponents.Models.Interfaces;
using Core.SNBT;
using Core.SNBT.Nodes;

using Core.SNBT.Interfaces;

namespace Core.DataComponents.Models;

/// <summary>
/// Represents an inline instrument definition containing sound, description, duration, and range settings.
/// </summary>
/// <param name="Description">The text component used as the instrument's tooltip description.</param>
/// <param name="SoundEvent">The sound event played when the instrument is used.</param>
/// <param name="UseDuration">The cooldown duration in seconds before the instrument can be used again.</param>
/// <param name="Range">The audible range of the sound in blocks.</param>
/// <param name="DurabilityDamage">Optional durability consumed on each use.</param>
public record Instrument(
    ISnbtNode Description,
    SoundEvent SoundEvent,
    float UseDuration,
    float Range,
    int? DurabilityDamage = null
) : ICompoundModel<Instrument>
{
    /// <summary>
    /// Parses an <see cref="Instrument"/> definition from an SNBT compound node.
    /// </summary>
    /// <param name="compound">The SNBT compound containing instrument fields.</param>
    /// <returns>A populated <see cref="Instrument"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown when required fields (<c>description</c> or <c>sound_event</c>) are missing.</exception>
    public static Instrument Parse(SnbtCompound compound)
    {
        var description = compound.GetNode("description")
                          ?? throw new ArgumentException("Instrument compound is missing required 'description' tag.");
        var soundNode = compound.GetNode("sound_event")
                        ?? throw new ArgumentException("Instrument compound is missing required 'sound_event' tag.");

        return new Instrument(
            Description: description,
            SoundEvent: SoundEvent.Parse(soundNode),
            UseDuration: compound.GetFloat("use_duration"),
            Range: compound.GetFloat("range"),
            DurabilityDamage: compound.GetOptionalInt("durability_damage")
        );
    }

    /// <summary>
    /// Serializes the instrument definition into an SNBT compound node.
    /// </summary>
    /// <returns>An <see cref="ISnbtNode"/> representing the instrument compound.</returns>
    public ISnbtNode ToSnbt() => Snbt.Compound()
        .Put("description", Description)
        .Put("sound_event", SoundEvent.ToSnbt())
        .Put("use_duration", UseDuration)
        .Put("range", Range)
        .PutOptional("durability_damage", DurabilityDamage)
        .Build();
}