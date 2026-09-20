using Core.DataComponents.Interfaces;
using Core.DataComponents.Models;
using Core.SNBT.Interfaces;
using Core.SNBT.Nodes;
using JetBrains.Annotations;

namespace Core.DataComponents.Components;

/// <summary>
/// Configures block transformation behavior applied when using an item on blocks (<c>minecraft:block_transformer</c>).
/// Can reference a predefined transformer identifier or define inline transformation entries.
/// </summary>
/// <param name="Identifier">The resource location of a predefined transformer (e.g., <c>"minecraft:shovel"</c>), if defined by reference.</param>
/// <param name="Transformers">The collection of transformation rules evaluated sequentially until one produces a state.</param>
[UsedImplicitly]
public record BlockTransformerComponent(
    string? Identifier = null,
    List<BlockTransformer>? Transformers = null
) : IFlexibleComponent<BlockTransformerComponent>
{
    /// <inheritdoc/>
    public static string ComponentId => "minecraft:block_transformer";

    /// <summary>
    /// Initializes a new instance of the <see cref="BlockTransformerComponent"/> record referencing a predefined transformer.
    /// </summary>
    /// <param name="identifier">The transformer resource location identifier.</param>
    public BlockTransformerComponent(string identifier)
        : this(Identifier: identifier, Transformers: null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BlockTransformerComponent"/> record with inline transformation rules.
    /// </summary>
    /// <param name="transformers">The block transformer rule entries.</param>
    public BlockTransformerComponent(params BlockTransformer[] transformers)
        : this(Identifier: null, Transformers: [..transformers])
    {
    }

    /// <summary>
    /// Parses a <see cref="BlockTransformerComponent"/> from an SNBT node representation.
    /// </summary>
    /// <param name="node">The SNBT node (either an <see cref="SnbtString"/>, <see cref="SnbtCompound"/>, or <see cref="SnbtList"/>).</param>
    /// <returns>A populated <see cref="BlockTransformerComponent"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="node"/> is not a supported node type.</exception>
    /// <example>
    /// <code>
    /// // From string identifier:
    /// var comp1 = BlockTransformerComponent.Parse(new SnbtString("shovel"));
    ///
    /// // From list of transformers:
    /// var listNode = SnbtParser.Parse("[{block_state_provider: {type: \"rule_based\", rules: []}}]");
    /// var comp2 = BlockTransformerComponent.Parse(listNode);
    /// </code>
    /// </example>
    public static BlockTransformerComponent Parse(ISnbtNode node)
    {
        return node switch
        {
            SnbtString str => new BlockTransformerComponent(str.Value),
            SnbtCompound compound => new BlockTransformerComponent(BlockTransformer.Parse(compound)),
            SnbtList list => new BlockTransformerComponent(
                Identifier: null,
                Transformers: list.Items
                    .OfType<SnbtCompound>()
                    .Select(BlockTransformer.Parse)
                    .ToList()
            ),
            _ => throw new ArgumentException("Block transformer component must be a string, compound, or list node.", nameof(node))
        };
    }

    /// <summary>
    /// Serializes the component into its most compact SNBT representation.
    /// </summary>
    /// <returns>An <see cref="ISnbtNode"/> representing the string ID, single compound, or list of transformers.</returns>
    public ISnbtNode ToSnbt()
    {
        if (Identifier != null)
            return new SnbtString(Identifier);

        if (Transformers == null || Transformers.Count == 0)
            return new SnbtList();

        if (Transformers.Count == 1)
            return Transformers[0].ToSnbt();

        var list = new SnbtList();
        foreach (var transformer in Transformers)
            list.Items.Add(transformer.ToSnbt());

        return list;
    }
}