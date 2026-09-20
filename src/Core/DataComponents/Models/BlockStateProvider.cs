using Core.DataComponents.Models.Interfaces;
using Core.SNBT;
using Core.SNBT.Interfaces;
using Core.SNBT.Nodes;
using JetBrains.Annotations;

namespace Core.DataComponents.Models;

/// <summary>
/// Defines a block state provider used during block transformation to supply replacement states.
/// </summary>
/// <param name="Type">The provider type identifier (e.g., <c>"rule_based"</c>, <c>"simple_state_provider"</c>).</param>
/// <param name="Fallback">Optional fallback provider evaluated when no rules match.</param>
/// <param name="Rules">The ordered collection of condition-action rules evaluated sequentially.</param>
/// <param name="ExtraData">The original raw compound retained to preserve provider-specific tags.</param>
[UsedImplicitly]
public record BlockStateProvider(
    string Type,
    BlockStateProvider? Fallback = null,
    List<BlockStateRule>? Rules = null,
    SnbtCompound? ExtraData = null
) : ICompoundModel<BlockStateProvider>
{
    /// <summary>
    /// Identifier for rule-based providers starting in Java Edition 26.3+.
    /// </summary>
    public const string RuleBased = "rule_based";

    /// <summary>
    /// Legacy identifier for rule-based providers prior to Java Edition 26.3.
    /// </summary>
    public const string LegacyRuleBased = "rule_based_state_provider";

    /// <summary>
    /// Parses a <see cref="BlockStateProvider"/> from an SNBT compound node.
    /// </summary>
    /// <param name="compound">The SNBT compound node representing the provider.</param>
    /// <returns>A populated <see cref="BlockStateProvider"/> instance.</returns>
    /// <example>
    /// <code>
    /// var node = SnbtParser.Parse("{type: \"rule_based\", rules: []}");
    /// var provider = BlockStateProvider.Parse(node);
    /// </code>
    /// </example>
    public static BlockStateProvider Parse(SnbtCompound compound)
    {
        var type = compound.GetString("type");

        var fallback = compound.GetNode("fallback") is SnbtCompound fallbackNode
            ? Parse(fallbackNode)
            : null;

        List<BlockStateRule>? rules = null;
        if (compound.GetNode("rules") is not SnbtList rulesList)
            return new BlockStateProvider(
                Type: type,
                Fallback: fallback,
                Rules: rules,
                ExtraData: compound
            );

        rules = new List<BlockStateRule>(rulesList.Items.Count);
        foreach (var item in rulesList.Items)
        {
            if (item is SnbtCompound ruleCompound)
                rules.Add(BlockStateRule.Parse(ruleCompound));
        }

        return new BlockStateProvider(
            Type: type,
            Fallback: fallback,
            Rules: rules,
            ExtraData: compound
        );
    }

    /// <summary>
    /// Serializes the provider into an SNBT compound node, including all type-specific parameters.
    /// </summary>
    /// <returns>An <see cref="ISnbtNode"/> representing the block state provider compound.</returns>
    public ISnbtNode ToSnbt()
    {
        var builder = Snbt.Compound().Put("type", Type);

        if (Fallback != null)
            builder.Put("fallback", Fallback.ToSnbt());

        if (Rules != null)
        {
            var rulesList = new SnbtList();
            foreach (var rule in Rules)
                rulesList.Items.Add(rule.ToSnbt());

            builder.Put("rules", rulesList);
        }

        if (ExtraData == null)
            return builder.Build();

        foreach (var (key, value) in ExtraData.Tags)
        {
            if (key is not ("type" or "fallback" or "rules"))
                builder.Put(key, value);
        }

        return builder.Build();
    }
}