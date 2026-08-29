using Core.DataComponents.Models;
using Core.SNBT;
using Core.SNBT.Nodes;

using Core.SNBT.Interfaces;
using JetBrains.Annotations;

using Core.DataComponents.Interfaces;

namespace Core.DataComponents.Components;

/// <summary>
/// Configures an item to function as a tool with customizable mining speed, durability wear, and block rules (<c>minecraft:tool</c>).
/// </summary>
/// <param name="DefaultMiningSpeed">The baseline mining speed multiplier when no rule matches. Defaults to 1.0.</param>
/// <param name="DamagePerBlock">The durability damage consumed per block broken. Defaults to 1.</param>
/// <param name="CanDestroyBlocksInCreative">Whether blocks can be broken with this tool in Creative mode. Defaults to <see langword="true"/>.</param>
/// <param name="Rules">Ordered list of matching rules for specific block types or tags.</param>
[UsedImplicitly]
public record ToolComponent(
    float DefaultMiningSpeed = 1.0f,
    int DamagePerBlock = 1,
    bool CanDestroyBlocksInCreative = true,
    List<ToolRule>? Rules = null
) : ICompoundComponent<ToolComponent>
{
    /// <inheritdoc/>
    public static string ComponentId => "minecraft:tool";

    /// <summary>
    /// Initializes a new instance of the <see cref="ToolComponent"/> record with specific tool rules.
    /// </summary>
    /// <param name="rules">The block mining rules to attach to this tool.</param>
    public ToolComponent(params ToolRule[] rules)
        : this(1.0f, 1, true, rules.ToList())
    {
    }

    /// <summary>
    /// Parses a <see cref="ToolComponent"/> from an SNBT node representation.
    /// </summary>
    /// <param name="compound">The SNBT node to parse, which must be an <see cref="SnbtCompound"/>.</param>
    /// <returns>A populated <see cref="ToolComponent"/> instance.</returns>
    /// <example>
    /// <code>
    /// var node = SnbtParser.Parse("{default_mining_speed: 1.5f, damage_per_block: 2, rules: [{blocks: \"#mineable/pickaxe\", speed: 6.0f, correct_for_drops: 1b}]}");
    /// var component = ToolComponent.Parse(node);
    /// </code>
    /// </example>
    public static ToolComponent Parse(SnbtCompound compound)
    {

        List<ToolRule>? rules = null;
        if (compound.GetNode("rules") is SnbtList list)
        {
            rules = new List<ToolRule>(list.Items.Count);
            foreach (var item in list.Items)
            {
                if (item is SnbtCompound ruleComp)
                    rules.Add(ToolRule.Parse(ruleComp));
            }
        }

        return new ToolComponent(
            DefaultMiningSpeed: compound.GetFloat("default_mining_speed", 1.0f),
            DamagePerBlock: compound.GetInt("damage_per_block", 1),
            CanDestroyBlocksInCreative: compound.GetBool("can_destroy_blocks_in_creative", true),
            Rules: rules
        );
    }

    /// <summary>
    /// Serializes the component into an SNBT compound node.
    /// </summary>
    /// <returns>An <see cref="ISnbtNode"/> representing the tool configuration compound.</returns>
    public ISnbtNode ToSnbt()
    {
        var builder = Snbt.Compound()
            .PutOptional("default_mining_speed", DefaultMiningSpeed, 1.0f)
            .PutOptional("damage_per_block", DamagePerBlock, 1)
            .PutOptional("can_destroy_blocks_in_creative", CanDestroyBlocksInCreative, true);

        if (Rules is { Count: > 0 })
        {
            builder.PutList("rules", list =>
            {
                foreach (var rule in Rules)
                    list.Add(rule.ToSnbt());
            });
        }

        return builder.Build();
    }
}