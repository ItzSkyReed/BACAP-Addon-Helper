using Core.DataComponents.Components.Base;
using Core.DataComponents.Interfaces;
using Core.SNBT.Nodes;
using JetBrains.Annotations;

namespace Core.DataComponents.Components;

/// <summary>
/// Specifies the decorated pot pattern provided when this item is used on a side of a decorated pot (<c>minecraft:provides_pottery_pattern</c>).
/// </summary>
/// <param name="Value">The namespaced resource identifier of the decorated pot pattern (e.g., <c>"minecraft:angler"</c>).</param>
[UsedImplicitly]
public record ProvidesPotteryPatternComponent(
    string Value
) : StringComponentBase(Value), IStringComponent<ProvidesPotteryPatternComponent>
{
    /// <inheritdoc/>
    public static string ComponentId => "minecraft:provides_pottery_pattern";

    /// <summary>
    /// Gets the decorated pot pattern resource identifier. Alias for <see cref="StringComponentBase.Value"/>.
    /// </summary>
    public string Pattern => Value;

    /// <summary>
    /// Parses a <see cref="ProvidesPotteryPatternComponent"/> from an SNBT string node representation.
    /// </summary>
    /// <param name="node">The SNBT string node containing the pattern identifier.</param>
    /// <returns>A populated <see cref="ProvidesPotteryPatternComponent"/> instance.</returns>
    /// <example>
    /// <code>
    /// var node = new SnbtString("minecraft:angler");
    /// var component = ProvidesPotteryPatternComponent.Parse(node);
    /// </code>
    /// </example>
    public static ProvidesPotteryPatternComponent Parse(SnbtString node)
    {
        return new ProvidesPotteryPatternComponent(node.Value);
    }

    /// <summary>
    /// Implicitly converts a string pattern identifier into a <see cref="ProvidesPotteryPatternComponent"/>.
    /// </summary>
    /// <param name="value">The decorated pot pattern resource identifier.</param>
    public static implicit operator ProvidesPotteryPatternComponent(string value) => new(value);

    /// <summary>
    /// Implicitly converts a <see cref="ProvidesPotteryPatternComponent"/> to its underlying string pattern identifier.
    /// </summary>
    /// <param name="component">The component instance.</param>
    public static implicit operator string(ProvidesPotteryPatternComponent component) => component.Value;
}