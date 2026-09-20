using Core.SNBT.Interfaces;
using Core.SNBT.Nodes;
using JetBrains.Annotations;

using Core.DataComponents.Interfaces;

namespace Core.DataComponents.Components;

/// <summary>
/// Specifies the maximum durability damage that this item can sustain before breaking (<c>minecraft:max_damage</c>).
/// </summary>
/// <remarks>
/// Must be a non-zero positive integer. Cannot be combined with <c>minecraft:max_stack_size</c> if the stack size is greater than 1.
/// </remarks>
/// <param name="Value">The maximum durability points of the item.</param>
[UsedImplicitly]
public record MaxDamageComponent(
    int Value
) : IParsableComponent<MaxDamageComponent>
{
    /// <inheritdoc/>
    public static string ComponentId => "minecraft:max_damage";

    /// <summary>
    /// Parses a <see cref="MaxDamageComponent"/> from an SNBT node representation.
    /// </summary>
    /// <param name="node">The SNBT node to parse, which must be an <see cref="SnbtInt"/> or <see cref="SnbtLong"/>.</param>
    /// <returns>A populated <see cref="MaxDamageComponent"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="node"/> is not an integer node.</exception>
    /// <example>
    /// <code>
    /// var node = SnbtParser.Parse("4");
    /// var component = MaxDamageComponent.Parse(node);
    /// </code>
    /// </example>
    public static MaxDamageComponent Parse(ISnbtNode node)
    {
        return node switch
        {
            SnbtInt intNode => new MaxDamageComponent(intNode.Value),
            SnbtLong longNode => new MaxDamageComponent((int)longNode.Value),
            _ => throw new ArgumentException("Max damage component must be an integer node.")
        };
    }

    /// <summary>
    /// Serializes the component into an SNBT integer node.
    /// </summary>
    /// <returns>An <see cref="ISnbtNode"/> containing the max durability integer.</returns>
    public ISnbtNode ToSnbt() => new SnbtInt(Value);

    /// <summary>
    /// Implicitly converts a positive integer into a <see cref="MaxDamageComponent"/>.
    /// </summary>
    /// <param name="value">The max durability value.</param>
    public static implicit operator MaxDamageComponent(int value) => new(value);
}