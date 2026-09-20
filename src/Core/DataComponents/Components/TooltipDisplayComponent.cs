
using Core.SNBT;
using Core.SNBT.Nodes;

using Core.SNBT.Interfaces;
using JetBrains.Annotations;

using Core.DataComponents.Interfaces;

namespace Core.DataComponents.Components;

/// <summary>
/// Controls tooltip visibility, allowing individual component tooltips or the entire tooltip to be suppressed (<c>minecraft:tooltip_display</c>).
/// </summary>
/// <param name="HideTooltip">If <see langword="true"/>, the item displays no tooltip when hovered. Defaults to <see langword="false"/>.</param>
/// <param name="HiddenComponents">Optional list of component resource identifiers whose tooltips are hidden.</param>
[UsedImplicitly]
public record TooltipDisplayComponent(
    bool HideTooltip = false,
    List<string>? HiddenComponents = null
) : ICompoundComponent<TooltipDisplayComponent>
{
    /// <inheritdoc/>
    public static string ComponentId => "minecraft:tooltip_display";

    /// <summary>
    /// Initializes a new instance of the <see cref="TooltipDisplayComponent"/> record specifying component identifiers to hide.
    /// </summary>
    /// <param name="hiddenComponents">The component resource locations to hide from tooltips.</param>
    public TooltipDisplayComponent(params string[] hiddenComponents)
        : this(HideTooltip: false, HiddenComponents: hiddenComponents.ToList())
    {
    }

    /// <summary>
    /// Parses a <see cref="TooltipDisplayComponent"/> from an SNBT node representation.
    /// </summary>
    /// <param name="compound">The SNBT node to parse, which must be an <see cref="SnbtCompound"/>.</param>
    /// <returns>A populated <see cref="TooltipDisplayComponent"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="compound"/> is not an <see cref="SnbtCompound"/>.</exception>
    /// <example>
    /// <code>
    /// var node = SnbtParser.Parse("{hidden_components: [\"minecraft:enchantments\"], hide_tooltip: 0b}");
    /// var component = TooltipDisplayComponent.Parse(node);
    /// </code>
    /// </example>
    public static TooltipDisplayComponent Parse(SnbtCompound compound)
    {

        List<string>? hiddenComponents = null;
        if (compound.GetNode("hidden_components") is SnbtList list)
        {
            hiddenComponents = new List<string>(list.Items.Count);
            foreach (var item in list.Items)
            {
                if (item is SnbtString str)
                    hiddenComponents.Add(str.Value);
            }
        }

        return new TooltipDisplayComponent(
            HideTooltip: compound.GetBool("hide_tooltip"),
            HiddenComponents: hiddenComponents
        );
    }

    /// <summary>
    /// Serializes the component into an SNBT compound node.
    /// </summary>
    /// <returns>An <see cref="ISnbtNode"/> representing the tooltip display compound.</returns>
    public ISnbtNode ToSnbt()
    {
        var builder = Snbt.Compound()
            .PutOptional("hide_tooltip", HideTooltip, false);

        if (HiddenComponents is { Count: > 0 })
        {
            builder.PutList("hidden_components", list =>
            {
                foreach (var compId in HiddenComponents)
                    list.Add(compId);
            });
        }

        return builder.Build();
    }
}