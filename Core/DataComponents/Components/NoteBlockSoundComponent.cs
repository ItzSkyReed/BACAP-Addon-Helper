using Core.SNBT.Interfaces;
using Core.SNBT.Nodes;
using JetBrains.Annotations;

using Core.DataComponents.Interfaces;

namespace Core.DataComponents.Components;

/// <summary>
/// Specifies the sound event identifier played by a note block when this head block is placed directly on top of it (<c>minecraft:note_block_sound</c>).
/// </summary>
/// <param name="SoundId">The resource identifier of the sound event (e.g. <c>minecraft:entity.item.pickup</c>).</param>
[UsedImplicitly]
public record NoteBlockSoundComponent(
    string SoundId
) : IParsableComponent<NoteBlockSoundComponent>
{
    /// <inheritdoc/>
    public static string ComponentId => "minecraft:note_block_sound";

    /// <summary>
    /// Parses a <see cref="NoteBlockSoundComponent"/> from an SNBT node representation.
    /// </summary>
    /// <param name="node">The SNBT node to parse, which must be an <see cref="SnbtString"/>.</param>
    /// <returns>A populated <see cref="NoteBlockSoundComponent"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="node"/> is not an <see cref="SnbtString"/>.</exception>
    /// <example>
    /// <code>
    /// var node = SnbtParser.Parse("\"minecraft:entity.item.pickup\"");
    /// var component = NoteBlockSoundComponent.Parse(node);
    /// </code>
    /// </example>
    public static NoteBlockSoundComponent Parse(ISnbtNode node)
    {
        return node is not SnbtString str
            ? throw new ArgumentException("Note block sound component must be a string sound identifier.")
            : new NoteBlockSoundComponent(str.Value);
    }

    /// <summary>
    /// Serializes the component into an SNBT string node.
    /// </summary>
    /// <returns>An <see cref="ISnbtNode"/> containing the sound event identifier.</returns>
    public ISnbtNode ToSnbt() => new SnbtString(SoundId);

    /// <summary>
    /// Implicitly converts a sound event identifier string into a <see cref="NoteBlockSoundComponent"/>.
    /// </summary>
    /// <param name="soundId">The resource identifier of the sound.</param>
    public static implicit operator NoteBlockSoundComponent(string soundId) => new(soundId);
}