
using Core.DataComponents.Models;
using Core.SNBT.Interfaces;
using Core.SNBT.Nodes;
using JetBrains.Annotations;

using Core.DataComponents.Interfaces;

namespace Core.DataComponents.Components;

/// <summary>
/// Configures the instrument played by an item such as a goat horn (<c>minecraft:instrument</c>).
/// Can reference a registered instrument ID or define an inline instrument configuration.
/// </summary>
/// <param name="InstrumentId">Optional resource location identifier of a registered instrument (e.g., <c>minecraft:feel_goat_horn</c>).</param>
/// <param name="InlineInstrument">Optional inline instrument definition.</param>
[UsedImplicitly]
public record InstrumentComponent(
    string? InstrumentId = null,
    Instrument? InlineInstrument = null
) : IParsableComponent<InstrumentComponent>
{
    /// <inheritdoc/>
    public static string ComponentId => "minecraft:instrument";

    /// <summary>
    /// Initializes a new instance of the <see cref="InstrumentComponent"/> record with a registered instrument ID.
    /// </summary>
    /// <param name="instrumentId">The resource identifier of the instrument.</param>
    public InstrumentComponent(string instrumentId) : this(InstrumentId: instrumentId, InlineInstrument: null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="InstrumentComponent"/> record with an inline instrument definition.
    /// </summary>
    /// <param name="inlineInstrument">The inline instrument configuration.</param>
    public InstrumentComponent(Instrument inlineInstrument) : this(InstrumentId: null, InlineInstrument: inlineInstrument)
    {
    }

    /// <summary>
    /// Parses an <see cref="InstrumentComponent"/> from an SNBT node representation.
    /// </summary>
    /// <param name="node">The SNBT node (either <see cref="SnbtString"/> or <see cref="SnbtCompound"/>).</param>
    /// <returns>A populated <see cref="InstrumentComponent"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="node"/> is neither a string nor a compound.</exception>
    /// <example>
    /// <code>
    /// // From ID:
    /// var comp1 = InstrumentComponent.Parse(new SnbtString("minecraft:feel_goat_horn"));
    ///
    /// // From inline compound:
    /// var node = SnbtParser.Parse("{description: {text: \"Prank!\"}, sound_event: \"entity.creeper.primed\", use_duration: 2.0f, range: 30.0f}");
    /// var comp2 = InstrumentComponent.Parse(node);
    /// </code>
    /// </example>
    public static InstrumentComponent Parse(ISnbtNode node)
    {
        return node switch
        {
            SnbtString str => new InstrumentComponent(str.Value),
            SnbtCompound compound => new InstrumentComponent(Instrument.Parse(compound)),
            _ => throw new ArgumentException("Instrument component must be either a string identifier or a compound.")
        };
    }

    /// <summary>
    /// Serializes the component into an SNBT node structure.
    /// </summary>
    /// <returns>An <see cref="ISnbtNode"/> containing the string ID or compound definition.</returns>
    /// <exception cref="InvalidOperationException">Thrown when neither an ID nor an inline instrument is set.</exception>
    public ISnbtNode ToSnbt()
    {
        if (InstrumentId != null)
            return new SnbtString(InstrumentId);

        if (InlineInstrument != null)
            return InlineInstrument.ToSnbt();

        throw new InvalidOperationException("InstrumentComponent must contain either an InstrumentId or an InlineInstrument.");
    }

    /// <summary>
    /// Implicitly converts a string identifier into an <see cref="InstrumentComponent"/>.
    /// </summary>
    /// <param name="instrumentId">The resource identifier of the instrument.</param>
    public static implicit operator InstrumentComponent(string instrumentId) => new(instrumentId);

    /// <summary>
    /// Implicitly converts an inline <see cref="Instrument"/> into an <see cref="InstrumentComponent"/>.
    /// </summary>
    /// <param name="instrument">The instrument definition.</param>
    public static implicit operator InstrumentComponent(Instrument instrument) => new(instrument);
}