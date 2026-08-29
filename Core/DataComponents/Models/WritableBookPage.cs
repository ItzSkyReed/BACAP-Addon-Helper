using Core.DataComponents.Models.Interfaces;
using Core.SNBT;
using Core.SNBT.Nodes;

using Core.SNBT.Interfaces;

namespace Core.DataComponents.Models;

/// <summary>
/// Represents a single page in a writable book (Book &amp; Quill).
/// </summary>
/// <param name="Raw">The plain text content of the page.</param>
/// <param name="Filtered">Optional filtered text shown to players with chat filtering enabled.</param>
public record WritableBookPage(
    string Raw,
    string? Filtered = null
): IFlexibleModel<WritableBookPage>
{
    /// <summary>
    /// Parses a <see cref="WritableBookPage"/> from an SNBT node representation.
    /// Supports both simple string lines and filtered text compound objects.
    /// </summary>
    /// <param name="node">The SNBT node to parse (<see cref="SnbtString"/> or <see cref="SnbtCompound"/>).</param>
    /// <returns>A populated <see cref="WritableBookPage"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="node"/> is neither a string nor a compound.</exception>
    public static WritableBookPage Parse(ISnbtNode node)
    {
        return node switch
        {
            SnbtString str => new WritableBookPage(str.Value),
            SnbtCompound compound => new WritableBookPage(
                Raw: compound.GetString("raw", string.Empty),
                Filtered: compound.GetOptionalString("filtered")
            ),
            _ => throw new ArgumentException("Writable book page must be a string or a compound.")
        };
    }

    /// <summary>
    /// Serializes the page into an SNBT node.
    /// Emits a compact <see cref="SnbtString"/> if no filtered text is specified.
    /// </summary>
    /// <returns>An <see cref="ISnbtNode"/> representing the page content.</returns>
    public ISnbtNode ToSnbt()
    {
        if (Filtered == null)
            return new SnbtString(Raw);

        return Snbt.Compound()
            .Put("raw", Raw)
            .Put("filtered", Filtered)
            .Build();
    }

    /// <summary>
    /// Implicitly converts a plain text string into a <see cref="WritableBookPage"/>.
    /// </summary>
    /// <param name="raw">The plain text page content.</param>
    public static implicit operator WritableBookPage(string raw) => new(raw);
}