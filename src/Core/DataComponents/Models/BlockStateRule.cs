using Core.DataComponents.Models.Interfaces;
using Core.SNBT;
using Core.SNBT.Interfaces;
using Core.SNBT.Nodes;
using JetBrains.Annotations;

namespace Core.DataComponents.Models;

/// <summary>
/// Represents a conditional rule mapping a block predicate to a resulting block state provider.
/// </summary>
/// <param name="IfTrue">The block condition tested against the targeted block state and position.</param>
/// <param name="Then">The state provider evaluated and applied when the predicate matches.</param>
[UsedImplicitly]
public record BlockStateRule(
    BlockPredicate IfTrue,
    BlockStateProvider Then
) : ICompoundModel<BlockStateRule>
{
    /// <summary>
    /// Parses a <see cref="BlockStateRule"/> from an SNBT compound node.
    /// </summary>
    /// <param name="compound">The compound node containing <c>if_true</c> and <c>then</c> entries.</param>
    /// <returns>A populated <see cref="BlockStateRule"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="compound"/> is missing the required <c>if_true</c> or <c>then</c> compounds.</exception>
    /// <example>
    /// <code>
    /// var node = SnbtParser.Parse("{if_true: {blocks: \"minecraft:dirt\"}, then: {type: \"simple_state_provider\", state: {Name: \"minecraft:dirt_path\"}}}");
    /// var rule = BlockStateRule.Parse(node);
    /// </code>
    /// </example>
    public static BlockStateRule Parse(SnbtCompound compound)
    {
        if (compound.GetNode("if_true") is not SnbtCompound ifTrueCompound)
            throw new ArgumentException("Block state rule requires an 'if_true' compound.", nameof(compound));

        if (compound.GetNode("then") is not SnbtCompound thenCompound)
            throw new ArgumentException("Block state rule requires a 'then' compound.", nameof(compound));

        return new BlockStateRule(
            IfTrue: BlockPredicate.Parse(ifTrueCompound),
            Then: BlockStateProvider.Parse(thenCompound)
        );
    }

    /// <summary>
    /// Serializes the rule into an SNBT compound containing <c>if_true</c> and <c>then</c> nodes.
    /// </summary>
    /// <returns>An <see cref="ISnbtNode"/> representing the rule compound.</returns>
    public ISnbtNode ToSnbt() => Snbt.Compound()
        .Put("if_true", IfTrue.ToSnbt())
        .Put("then", Then.ToSnbt())
        .Build();
}