using Core.SNBT;
using Core.SNBT.Nodes;

using Core.SNBT.Interfaces;
using JetBrains.Annotations;

using Core.DataComponents.Interfaces;

namespace Core.DataComponents.Components;

/// <summary>
/// Configures an item to be playable inside a jukebox, referencing a song identifier (<c>minecraft:jukebox_playable</c>).
/// </summary>
/// <param name="Song">The resource location of the jukebox song (e.g. <c>minecraft:pigstep</c>).</param>
/// <param name="ShowInTooltip">Whether the song title and author are shown in the item's tooltip. Defaults to <see langword="true"/>.</param>
[UsedImplicitly]
public record JukeboxPlayableComponent(
    string Song,
    bool ShowInTooltip = true
) : IParsableComponent<JukeboxPlayableComponent>
{
    /// <inheritdoc/>
    public static string ComponentId => "minecraft:jukebox_playable";

    /// <summary>
    /// Parses a <see cref="JukeboxPlayableComponent"/> from an SNBT node representation.
    /// Supports direct song ID strings or compounds containing <c>song</c> and <c>show_in_tooltip</c>.
    /// </summary>
    /// <param name="node">The SNBT node (either <see cref="SnbtString"/> or <see cref="SnbtCompound"/>).</param>
    /// <returns>A populated <see cref="JukeboxPlayableComponent"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="node"/> is neither a string nor a compound.</exception>
    /// <example>
    /// <code>
    /// // From string song ID:
    /// var comp1 = JukeboxPlayableComponent.Parse(new SnbtString("minecraft:pigstep"));
    ///
    /// // From compound:
    /// var node = SnbtParser.Parse("{song: \"minecraft:cat\", show_in_tooltip: 0b}");
    /// var comp2 = JukeboxPlayableComponent.Parse(node);
    /// </code>
    /// </example>
    public static JukeboxPlayableComponent Parse(ISnbtNode node)
    {
        return node switch
        {
            SnbtString str => new JukeboxPlayableComponent(str.Value),
            SnbtCompound compound => new JukeboxPlayableComponent(
                Song: compound.GetString("song"),
                ShowInTooltip: compound.GetBool("show_in_tooltip", true)
            ),
            _ => throw new ArgumentException("Jukebox playable component must be either a string song ID or a compound.")
        };
    }

    /// <summary>
    /// Serializes the component into an SNBT node.
    /// Emits a compact <see cref="SnbtString"/> if <see cref="ShowInTooltip"/> is default (<see langword="true"/>).
    /// </summary>
    /// <returns>An <see cref="ISnbtNode"/> representing the jukebox playable configuration.</returns>
    public ISnbtNode ToSnbt()
    {
        if (ShowInTooltip)
            return new SnbtString(Song);

        return Snbt.Compound()
            .Put("song", Song)
            .Put("show_in_tooltip", false)
            .Build();
    }

    /// <summary>
    /// Implicitly converts a song identifier string into a <see cref="JukeboxPlayableComponent"/>.
    /// </summary>
    /// <param name="song">The song resource location identifier.</param>
    public static implicit operator JukeboxPlayableComponent(string song) => new(song);
}