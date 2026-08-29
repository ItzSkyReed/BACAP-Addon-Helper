
using Core.SNBT.Interfaces;
using Core.SNBT.Nodes;
using JetBrains.Annotations;

using Core.DataComponents.Interfaces;

namespace Core.DataComponents.Components;

/// <summary>
/// Scales the duration of all potion status effects applied by an item or area effect cloud (<c>minecraft:potion_duration_scale</c>).
/// </summary>
/// <param name="Value">The multiplier scaling factor applied to effect durations.</param>
[UsedImplicitly]
public record PotionDurationScaleComponent(
    float Value
) : IParsableComponent<PotionDurationScaleComponent>
{
    /// <inheritdoc/>
    public static string ComponentId => "minecraft:potion_duration_scale";

    /// <summary>
    /// Parses a <see cref="PotionDurationScaleComponent"/> from an SNBT node representation.
    /// </summary>
    /// <param name="node">The SNBT node to parse, which must be a floating point or integer node.</param>
    /// <returns>A populated <see cref="PotionDurationScaleComponent"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="node"/> is not a numeric node.</exception>
    /// <example>
    /// <code>
    /// var node = SnbtParser.Parse("2.0f");
    /// var component = PotionDurationScaleComponent.Parse(node);
    /// </code>
    /// </example>
    public static PotionDurationScaleComponent Parse(ISnbtNode node)
    {
        return node switch
        {
            SnbtFloat floatNode => new PotionDurationScaleComponent(floatNode.Value),
            SnbtDouble doubleNode => new PotionDurationScaleComponent((float)doubleNode.Value),
            SnbtInt intNode => new PotionDurationScaleComponent(intNode.Value),
            _ => throw new ArgumentException("Potion duration scale component must be a numeric floating-point node.")
        };
    }

    /// <summary>
    /// Serializes the component into an SNBT float node.
    /// </summary>
    /// <returns>An <see cref="ISnbtNode"/> containing the float scaling factor.</returns>
    public ISnbtNode ToSnbt() => new SnbtFloat(Value);

    /// <summary>
    /// Implicitly converts a float scaling factor into a <see cref="PotionDurationScaleComponent"/>.
    /// </summary>
    /// <param name="value">The duration scale factor.</param>
    public static implicit operator PotionDurationScaleComponent(float value) => new(value);
}