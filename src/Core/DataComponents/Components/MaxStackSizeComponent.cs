using Core.SNBT.Interfaces;
using Core.SNBT.Nodes;
using JetBrains.Annotations;

using Core.DataComponents.Interfaces;

namespace Core.DataComponents.Components;

/// <summary>
/// Sets the maximum stack size for an item (<c>minecraft:max_stack_size</c>).
/// </summary>
/// <remarks>
/// Must be an integer between 1 and 99. Cannot be combined with <c>minecraft:max_damage</c> if set to a value greater than 1.
/// </remarks>
/// <param name="Value">The maximum number of items in a stack (1 to 99).</param>
[UsedImplicitly]
public record MaxStackSizeComponent(
    int Value
) : IParsableComponent<MaxStackSizeComponent>
{
    /// <inheritdoc/>
    public static string ComponentId => "minecraft:max_stack_size";

    /// <summary>
    /// Parses a <see cref="MaxStackSizeComponent"/> from an SNBT node representation.
    /// </summary>
    /// <param name="node">The SNBT node to parse, which must be an <see cref="SnbtInt"/>, <see cref="SnbtByte"/>, or <see cref="SnbtLong"/>.</param>
    /// <returns>A populated <see cref="MaxStackSizeComponent"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="node"/> is not an integer node.</exception>
    /// <example>
    /// <code>
    /// var node = SnbtParser.Parse("64");
    /// var component = MaxStackSizeComponent.Parse(node);
    /// </code>
    /// </example>
    public static MaxStackSizeComponent Parse(ISnbtNode node)
    {
        return node switch
        {
            SnbtInt intNode => new MaxStackSizeComponent(intNode.Value),
            SnbtByte byteNode => new MaxStackSizeComponent(byteNode.Value),
            SnbtLong longNode => new MaxStackSizeComponent((int)longNode.Value),
            _ => throw new ArgumentException("Max stack size component must be an integer node.")
        };
    }

    /// <summary>
    /// Serializes the component into an SNBT integer node.
    /// </summary>
    /// <returns>An <see cref="ISnbtNode"/> containing the max stack size integer.</returns>
    public ISnbtNode ToSnbt() => new SnbtInt(Value);

    /// <summary>
    /// Implicitly converts an integer into a <see cref="MaxStackSizeComponent"/>.
    /// </summary>
    /// <param name="value">The max stack size value.</param>
    public static implicit operator MaxStackSizeComponent(int value) => new(value);
}