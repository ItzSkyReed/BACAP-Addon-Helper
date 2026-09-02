using Core.SNBT.Interfaces;
using Core.SNBT.Nodes;
using Core.TextComponents;
using Core.TextComponents.Components;
using Core.DataComponents.Interfaces;
using JetBrains.Annotations;

namespace Core.DataComponents.Components;

/// <summary>
/// Represents the lore tooltip lines displayed on an item (<c>minecraft:lore</c>).
/// Holds a list of up to 256 strongly-typed text component lines.
/// </summary>
/// <param name="Lines">The list of text component lines to display in the tooltip.</param>
[UsedImplicitly]
public record LoreComponent(
    IReadOnlyList<TextComponent> Lines
) : IListComponent<LoreComponent>
{
    /// <inheritdoc/>
    public static string ComponentId => "minecraft:lore";

    /// <summary>
    /// Initializes a new instance of the <see cref="LoreComponent"/> record with plain string lines.
    /// </summary>
    /// <param name="lines">Literal plain text lines.</param>
    public LoreComponent(params string[] lines)
        : this([.. lines.Select(line => new PlainTextComponent(Text: line))])
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LoreComponent"/> record with strongly-typed text components.
    /// </summary>
    /// <param name="lines">Text components representing lore lines.</param>
    public LoreComponent(params TextComponent[] lines)
        : this(lines.ToList())
    {
    }

    /// <summary>
    /// Parses a <see cref="LoreComponent"/> from an SNBT node representation.
    /// Transforms each raw SNBT item in the list into a strongly-typed <see cref="TextComponent"/>.
    /// </summary>
    /// <param name="list">The SNBT node to parse, which must be an <see cref="SnbtList"/>.</param>
    /// <returns>A populated <see cref="LoreComponent"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="list"/> is not an <see cref="SnbtList"/>.</exception>
    public static LoreComponent Parse(SnbtList list)
    {
        // Immediately parse the raw SNBT nodes into our domain TextComponent models
        List<TextComponent> parsedLines = [.. list.Items.Select(TextComponentParser.Parse)];

        return new LoreComponent(parsedLines);
    }

    /// <summary>
    /// Serializes the component into an SNBT list node.
    /// </summary>
    /// <returns>An <see cref="ISnbtNode"/> containing the list of serialized text component representations.</returns>
    public ISnbtNode ToSnbt()
    {
        // Delegate serialization to each text component's internal ToSnbt method
        List<ISnbtNode> serializedLines = [.. Lines.Select(line => line.ToSnbt())];

        return new SnbtList(serializedLines);
    }
}