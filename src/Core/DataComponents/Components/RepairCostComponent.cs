
using Core.SNBT.Interfaces;
using Core.SNBT.Nodes;
using JetBrains.Annotations;

using Core.DataComponents.Interfaces;

namespace Core.DataComponents.Components;

/// <summary>
/// Specifies the additional anvil penalty cost in experience levels added when repairing, renaming, or combining this item (<c>minecraft:repair_cost</c>).
/// </summary>
/// <param name="Cost">The repair cost penalty in experience levels (defaults to 0).</param>
[UsedImplicitly]
public record RepairCostComponent(
    int Cost = 0
) : IParsableComponent<RepairCostComponent>
{
    /// <inheritdoc/>
    public static string ComponentId => "minecraft:repair_cost";

    /// <summary>
    /// Parses a <see cref="RepairCostComponent"/> from an SNBT node representation.
    /// </summary>
    /// <param name="node">The SNBT node to parse, which must be an integer node.</param>
    /// <returns>A populated <see cref="RepairCostComponent"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="node"/> is not an integer node.</exception>
    /// <example>
    /// <code>
    /// var node = SnbtParser.Parse("5");
    /// var component = RepairCostComponent.Parse(node);
    /// </code>
    /// </example>
    public static RepairCostComponent Parse(ISnbtNode node)
    {
        return node switch
        {
            SnbtInt intNode => new RepairCostComponent(intNode.Value),
            SnbtByte byteNode => new RepairCostComponent(byteNode.Value),
            SnbtLong longNode => new RepairCostComponent((int)longNode.Value),
            _ => throw new ArgumentException("Repair cost component must be an integer node.")
        };
    }

    /// <summary>
    /// Serializes the component into an SNBT integer node.
    /// </summary>
    /// <returns>An <see cref="ISnbtNode"/> containing the repair cost integer.</returns>
    public ISnbtNode ToSnbt() => new SnbtInt(Cost);

    /// <summary>
    /// Implicitly converts an integer cost into a <see cref="RepairCostComponent"/>.
    /// </summary>
    /// <param name="cost">The repair cost in levels.</param>
    public static implicit operator RepairCostComponent(int cost) => new(cost);
}