using Core.DataComponents.Models.Interfaces;
using Core.SNBT;
using Core.SNBT.Nodes;

using Core.SNBT.Interfaces;

namespace Core.DataComponents.Models;

/// <summary>
/// Represents a mining speed and drop behavior override rule for matching block types.
/// </summary>
/// <param name="Blocks">The list of matching block identifiers or a single tag selector (e.g. <c>#mineable/pickaxe</c>).</param>
/// <param name="Speed">Optional overriding mining speed multiplier when breaking matching blocks.</param>
/// <param name="CorrectForDrops">Optional override indicating if this tool is considered effective and drops loot from matching blocks.</param>
public record ToolRule(
    List<string> Blocks,
    float? Speed = null,
    bool? CorrectForDrops = null
) : ICompoundModel<ToolRule>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ToolRule"/> record for a single block ID or block tag.
    /// </summary>
    /// <param name="blockOrTag">The block identifier or tag selector (e.g. <c>#mineable/axe</c>).</param>
    /// <param name="speed">Optional overriding mining speed multiplier.</param>
    /// <param name="correctForDrops">Optional override indicating whether tool is correct for loot drops.</param>
    public ToolRule(string blockOrTag, float? speed = null, bool? correctForDrops = null)
        : this([blockOrTag], speed, correctForDrops)
    {
    }

    /// <summary>
    /// Parses a <see cref="ToolRule"/> from an SNBT compound node.
    /// </summary>
    /// <param name="compound">The SNBT compound node containing rule fields.</param>
    /// <returns>A populated <see cref="ToolRule"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown when the required <c>blocks</c> tag is missing.</exception>
    public static ToolRule Parse(SnbtCompound compound)
    {
        var blocksNode = compound.GetNode("blocks")
                         ?? throw new ArgumentException("Tool rule compound is missing required 'blocks' tag.");

        var blocks = blocksNode switch
        {
            SnbtString str => [str.Value],
            SnbtList list => ExtractBlocksList(list),
            _ => throw new ArgumentException("Tool rule 'blocks' must be a string identifier/tag or a list of strings.")
        };

        return new ToolRule(
            Blocks: blocks,
            Speed: compound.GetOptionalFloat("speed"),
            CorrectForDrops: compound.GetOptionalBool("correct_for_drops")
        );
    }

    /// <summary>
    /// Serializes the rule into an SNBT compound node.
    /// </summary>
    /// <returns>An <see cref="ISnbtNode"/> representing the tool rule compound.</returns>
    public ISnbtNode ToSnbt()
    {
        var builder = Snbt.Compound();

        if (Blocks.Count == 1)
            builder.Put("blocks", Blocks[0]);
        else
        {
            builder.PutList("blocks", list =>
            {
                foreach (var block in Blocks)
                    list.Add(block);
            });
        }

        builder.PutOptional("speed", Speed);
        builder.PutOptional("correct_for_drops", CorrectForDrops);

        return builder.Build();
    }

    private static List<string> ExtractBlocksList(SnbtList list)
    {
        var blocks = new List<string>(list.Items.Count);
        foreach (var item in list.Items)
        {
            if (item is SnbtString str)
                blocks.Add(str.Value);
        }

        return blocks;
    }
}