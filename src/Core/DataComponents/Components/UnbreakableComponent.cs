using Core.SNBT;
using Core.SNBT.Nodes;

using Core.SNBT.Interfaces;
using JetBrains.Annotations;

using Core.DataComponents.Interfaces;

namespace Core.DataComponents.Components;

/// <summary>
/// Prevents an item from taking durability damage and hides its durability bar (<c>minecraft:unbreakable</c>).
/// Displays a blue "Unbreakable" line in the tooltip.
/// </summary>
/// <param name="ShowInTooltip">Whether to display the "Unbreakable" text line in tooltips. Defaults to <see langword="true"/>.</param>
[UsedImplicitly]
public record UnbreakableComponent(
    bool ShowInTooltip = true
) : ICompoundComponent<UnbreakableComponent>
{
    /// <inheritdoc/>
    public static string ComponentId => "minecraft:unbreakable";

    /// <summary>
    /// Parses an <see cref="UnbreakableComponent"/> from an SNBT node representation.
    /// </summary>
    /// <param name="compound">The SNBT node to parse, which must be an <see cref="SnbtCompound"/>.</param>
    /// <returns>A populated <see cref="UnbreakableComponent"/> instance.</returns>
    /// <example>
    /// <code>
    /// var node = SnbtParser.Parse("{}");
    /// var component = UnbreakableComponent.Parse(node);
    /// </code>
    /// </example>
    public static UnbreakableComponent Parse(SnbtCompound compound)
    {

        return new UnbreakableComponent(
            ShowInTooltip: compound.GetBool("show_in_tooltip", true)
        );
    }

    /// <summary>
    /// Serializes the component into an SNBT compound node.
    /// </summary>
    /// <returns>An <see cref="ISnbtNode"/> representing the unbreakable compound.</returns>
    public ISnbtNode ToSnbt()
    {
        var builder = Snbt.Compound();

        if (!ShowInTooltip)
            builder.Put("show_in_tooltip", false);

        return builder.Build();
    }
}