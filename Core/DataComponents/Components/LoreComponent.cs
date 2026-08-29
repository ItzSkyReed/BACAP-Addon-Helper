using Core.SNBT.Interfaces;
using Core.SNBT.Nodes;
using JetBrains.Annotations;

using Core.DataComponents.Interfaces;

namespace Core.DataComponents.Components;

/// <summary>
/// Represents the lore tooltip lines displayed on an item (<c>minecraft:lore</c>).
/// Holds a list of up to 256 formatted text component lines.
/// </summary>
/// <param name="Lines">The list of text component lines to display in the tooltip.</param>
[UsedImplicitly]
public record LoreComponent(
    List<ISnbtNode> Lines
) : IListComponent<LoreComponent>
{
    /// <inheritdoc/>
    public static string ComponentId => "minecraft:lore";

    /// <summary>
    /// Initializes a new instance of the <see cref="LoreComponent"/> record with plain string lines.
    /// </summary>
    /// <param name="lines">Literal plain text lines.</param>
    public LoreComponent(params string[] lines)
        : this(lines.Select(ISnbtNode (line) => new SnbtString(line)).ToList())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LoreComponent"/> record with text component nodes.
    /// </summary>
    /// <param name="lines">Text component nodes representing lore lines.</param>
    public LoreComponent(params ISnbtNode[] lines)
        : this(new List<ISnbtNode>(lines))
    {
    }

    /// <summary>
    /// Parses a <see cref="LoreComponent"/> from an SNBT node representation.
    /// </summary>
    /// <param name="list">The SNBT node to parse, which must be an <see cref="SnbtList"/>.</param>
    /// <returns>A populated <see cref="LoreComponent"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="list"/> is not an <see cref="SnbtList"/>.</exception>
    /// <example>
    /// <code>
    /// var node = SnbtParser.Parse("[{text: \"A shiny Diamond!\", italic: false, color: \"gold\"}]");
    /// var component = LoreComponent.Parse(node);
    /// </code>
    /// </example>
    public static LoreComponent Parse(SnbtList list)
    {
        return new LoreComponent(new List<ISnbtNode>(list.Items));
    }

    /// <summary>
    /// Serializes the component into an SNBT list node.
    /// </summary>
    /// <returns>An <see cref="ISnbtNode"/> containing the list of lore lines.</returns>
    public ISnbtNode ToSnbt() => new SnbtList(Lines);
}