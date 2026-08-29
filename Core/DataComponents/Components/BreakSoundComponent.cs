using Core.DataComponents.Interfaces;
using Core.DataComponents.Models;
using Core.SNBT.Interfaces;
using JetBrains.Annotations;

namespace Core.DataComponents.Components;

/// <summary>
/// Overrides the sound played when an item breaks and reaches 0 durability (<c>minecraft:break_sound</c>).
/// </summary>
/// <param name="Sound">The sound event played upon item destruction (either a direct resource location or an inline sound definition with custom range).</param>
[UsedImplicitly]
public record BreakSoundComponent(
    SoundEvent Sound
) : IFlexibleComponent<BreakSoundComponent>
{
    /// <inheritdoc/>
    public static string ComponentId => "minecraft:break_sound";

    /// <summary>
    /// Parses a <see cref="BreakSoundComponent"/> from an SNBT node representation (string ID or compound).
    /// </summary>
    /// <param name="node">The SNBT node containing the sound event definition.</param>
    /// <returns>A populated <see cref="BreakSoundComponent"/> instance.</returns>
    /// <example>
    /// <code>
    /// var node = new SnbtString("minecraft:entity.item.break");
    /// var component = BreakSoundComponent.Parse(node);
    /// </code>
    /// </example>
    public static BreakSoundComponent Parse(ISnbtNode node) => new(SoundEvent.Parse(node));

    /// <summary>
    /// Serializes the break sound into its compact SNBT representation.
    /// </summary>
    /// <returns>An <see cref="ISnbtNode"/> representing the sound event.</returns>
    public ISnbtNode ToSnbt() => Sound.ToSnbt();

    /// <summary>
    /// Implicitly converts a sound event identifier string into a <see cref="BreakSoundComponent"/>.
    /// </summary>
    /// <param name="soundId">The resource identifier of the sound.</param>
    public static implicit operator BreakSoundComponent(string soundId) => new(new SoundEvent(soundId));

    /// <summary>
    /// Implicitly converts a <see cref="SoundEvent"/> model into a <see cref="BreakSoundComponent"/>.
    /// </summary>
    /// <param name="sound">The sound event model.</param>
    public static implicit operator BreakSoundComponent(SoundEvent sound) => new(sound);
}