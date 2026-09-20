using Core.SNBT.Interfaces;
using Core.SNBT.Nodes;
using JetBrains.Annotations;

using Core.DataComponents.Interfaces;

namespace Core.DataComponents.Components;

/// <summary>
/// Specifies the custom GUI sprite style used for rendering tooltip backgrounds and frames (<c>minecraft:tooltip_style</c>).
/// References <c>assets/&lt;namespace&gt;/textures/gui/sprites/tooltip/&lt;id&gt;_background</c> and <c>..._frame</c>.
/// </summary>
/// <param name="Style">The resource location of the custom tooltip sprite style.</param>
[UsedImplicitly]
public record TooltipStyleComponent(
    string Style
) : IStringComponent<TooltipStyleComponent>
{
    /// <inheritdoc/>
    public static string ComponentId => "minecraft:tooltip_style";

    /// <summary>
    /// Parses a <see cref="TooltipStyleComponent"/> from an SNBT node representation.
    /// </summary>
    /// <param name="strNode">The SNBT node to parse, which must be an <see cref="SnbtString"/>.</param>
    /// <returns>A populated <see cref="TooltipStyleComponent"/> instance.</returns>
    /// <example>
    /// <code>
    /// var node = SnbtParser.Parse("\"special_event\"");
    /// var component = TooltipStyleComponent.Parse(node);
    /// </code>
    /// </example>
    public static TooltipStyleComponent Parse(SnbtString strNode)
    {
        return new TooltipStyleComponent(strNode.Value);
    }

    /// <summary>
    /// Serializes the component into an SNBT string node.
    /// </summary>
    /// <returns>An <see cref="ISnbtNode"/> containing the tooltip style identifier.</returns>
    public ISnbtNode ToSnbt() => new SnbtString(Style);

    /// <summary>
    /// Implicitly converts a tooltip style resource location into a <see cref="TooltipStyleComponent"/>.
    /// </summary>
    /// <param name="style">The tooltip style resource identifier.</param>
    public static implicit operator TooltipStyleComponent(string style) => new(style);
}