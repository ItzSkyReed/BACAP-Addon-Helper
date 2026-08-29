using Core.SNBT.Interfaces;
using Core.SNBT.Nodes;
using JetBrains.Annotations;

using Core.DataComponents.Interfaces;

namespace Core.DataComponents.Components;

/// <summary>
/// Specifies the minimum cooldown charge required on the attack indicator to attack with this item (<c>minecraft:minimum_attack_charge</c>).
/// </summary>
/// <param name="Value">The minimum required attack meter charge between 0.0 and 1.0.</param>
[UsedImplicitly]
public record MinimumAttackChargeComponent(
    float Value
) : IParsableComponent<MinimumAttackChargeComponent>
{
    /// <inheritdoc/>
    public static string ComponentId => "minecraft:minimum_attack_charge";

    /// <summary>
    /// Parses a <see cref="MinimumAttackChargeComponent"/> from an SNBT node representation.
    /// </summary>
    /// <param name="node">The SNBT node to parse, which must be a floating point or integer node.</param>
    /// <returns>A populated <see cref="MinimumAttackChargeComponent"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="node"/> is not a numeric node.</exception>
    /// <example>
    /// <code>
    /// var node = SnbtParser.Parse("0.5f");
    /// var component = MinimumAttackChargeComponent.Parse(node);
    /// </code>
    /// </example>
    public static MinimumAttackChargeComponent Parse(ISnbtNode node)
    {
        return node switch
        {
            SnbtFloat floatNode => new MinimumAttackChargeComponent(floatNode.Value),
            SnbtDouble doubleNode => new MinimumAttackChargeComponent((float)doubleNode.Value),
            SnbtInt intNode => new MinimumAttackChargeComponent(intNode.Value),
            _ => throw new ArgumentException("Minimum attack charge component must be a numeric floating-point node.")
        };
    }

    /// <summary>
    /// Serializes the component into an SNBT float node.
    /// </summary>
    /// <returns>An <see cref="ISnbtNode"/> containing the attack charge float.</returns>
    public ISnbtNode ToSnbt() => new SnbtFloat(Value);

    /// <summary>
    /// Implicitly converts a float value into a <see cref="MinimumAttackChargeComponent"/>.
    /// </summary>
    /// <param name="value">The minimum charge value between 0.0 and 1.0.</param>
    public static implicit operator MinimumAttackChargeComponent(float value) => new(value);
}