using Core.DataComponents.Models.Interfaces;
using Core.SNBT;
using Core.SNBT.Nodes;

using Core.SNBT.Interfaces;

namespace Core.DataComponents.Models;

/// <summary>
/// Represents a formatted page in a finalized written book.
/// </summary>
/// <param name="Raw">The raw text component representing the page content.</param>
/// <param name="Filtered">Optional filtered text component shown to players with chat filtering enabled.</param>
public record WrittenBookPage(
    ISnbtNode Raw,
    ISnbtNode? Filtered = null
) : IFlexibleModel<WrittenBookPage>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WrittenBookPage"/> record using a plain text string.
    /// </summary>
    /// <param name="text">The plain text content of the page.</param>
    public WrittenBookPage(string text) : this(new SnbtString(text))
    {
    }

    /// <summary>
    /// Parses a <see cref="WrittenBookPage"/> from an SNBT node representation.
    /// </summary>
    /// <param name="node">The SNBT node to parse.</param>
    /// <returns>A populated <see cref="WrittenBookPage"/> instance.</returns>
    public static WrittenBookPage Parse(ISnbtNode node)
    {
        if (node is not SnbtCompound comp
            || (!comp.Tags.ContainsKey("raw")
                && !comp.Tags.ContainsKey("filtered")))
            return new WrittenBookPage(node);

        var rawNode = comp.GetNode("raw") ?? new SnbtString(string.Empty);
        var filteredNode = comp.GetNode("filtered");
        return new WrittenBookPage(rawNode, filteredNode);
    }

    /// <summary>
    /// Serializes the page into an SNBT node.
    /// </summary>
    /// <returns>An <see cref="ISnbtNode"/> representing the page.</returns>
    public ISnbtNode ToSnbt()
    {
        if (Filtered == null)
            return Raw;

        return Snbt.Compound()
            .Put("raw", Raw)
            .Put("filtered", Filtered)
            .Build();
    }

    /// <summary>
    /// Implicitly converts a text string into a <see cref="WrittenBookPage"/>.
    /// </summary>
    /// <param name="rawText">The plain text page content.</param>
    public static implicit operator WrittenBookPage(string rawText) => new(new SnbtString(rawText));
}