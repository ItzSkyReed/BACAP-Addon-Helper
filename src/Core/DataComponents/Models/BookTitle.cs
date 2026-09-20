
using Core.DataComponents.Models.Interfaces;
using Core.SNBT;

using Core.SNBT.Interfaces;
using Core.SNBT.Nodes;

namespace Core.DataComponents.Models;

/// <summary>
/// Represents the title of a written book, supporting optional chat-filtered text.
/// </summary>
/// <param name="Raw">The plain text title (up to 32 characters).</param>
/// <param name="Filtered">Optional filtered title shown to players with chat filtering enabled.</param>
public record BookTitle(
    string Raw,
    string? Filtered = null
) : IFlexibleModel<BookTitle>
{
    public static BookTitle Parse(ISnbtNode node)
    {
        return node switch
        {
            SnbtString str => new BookTitle(str.Value),
            SnbtCompound comp => new BookTitle(
                Raw: comp.GetString("raw", string.Empty),
                Filtered: comp.GetOptionalString("filtered")
            ),
            _ => throw new ArgumentException("Book title must be a string or a compound.")
        };
    }

    public ISnbtNode ToSnbt()
    {
        if (Filtered == null)
            return new SnbtString(Raw);

        return Snbt.Compound()
            .Put("raw", Raw)
            .Put("filtered", Filtered)
            .Build();
    }

    public static implicit operator BookTitle(string raw) => new(raw);
}