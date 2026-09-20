using Core.SNBT.Interfaces;
using Core.TextComponents.Components;
using Core.DataComponents.Interfaces;
using Core.TextComponents;
using JetBrains.Annotations;

namespace Core.DataComponents.Components;

/// <summary>
/// Represents the custom player-assigned display name of an item, entity, or block (<c>minecraft:custom_name</c>).
/// </summary>
/// <param name="Value">The parsed, strongly-typed text component model.</param>
[UsedImplicitly]
public record CustomNameComponent(
    TextComponent Value
) : IParsableComponent<CustomNameComponent>
{
    /// <inheritdoc/>
    public static string ComponentId => "minecraft:custom_name";

    /// <summary>
    /// Initializes a new instance of the <see cref="CustomNameComponent"/> record with a plain text string.
    /// </summary>
    /// <param name="plainText">The literal unformatted string name.</param>
    public CustomNameComponent(string plainText) : this(new PlainTextComponent(Text: plainText))
    {
    }

    /// <summary>
    /// Parses a <see cref="CustomNameComponent"/> by transforming an SNBT node into a TextComponent.
    /// </summary>
    /// <param name="node">The SNBT node (string, compound, or list) representing the text component.</param>
    /// <returns>A populated <see cref="CustomNameComponent"/> instance.</returns>
    public static CustomNameComponent Parse(ISnbtNode node)
    {
        // Immediately parse the raw SNBT node into our domain TextComponent model!
        // This ensures the rest of the application never has to deal with raw SNBT for custom names.
        var parsedText = TextComponentParser.Parse(node);
        return new CustomNameComponent(parsedText);
    }

    /// <summary>
    /// Serializes the text component back into its corresponding SNBT node.
    /// </summary>
    /// <returns>The <see cref="ISnbtNode"/> containing the text component representation.</returns>
    public ISnbtNode ToSnbt() => Value.ToSnbt(); // Delegates serialization to the text component itself
}