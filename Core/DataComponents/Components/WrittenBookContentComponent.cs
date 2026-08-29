using Core.DataComponents.Interfaces;
using Core.DataComponents.Models;
using Core.SNBT;
using Core.SNBT.Nodes;

using Core.SNBT.Interfaces;
using JetBrains.Annotations;

namespace Core.DataComponents.Components;

/// <summary>
/// Stores the metadata, author, generation, and formatted page contents of a signed written book (<c>minecraft:written_book_content</c>).
/// </summary>
/// <param name="Title">The title of the written book (overrides the base item name).</param>
/// <param name="Author">The author's username displayed in the item tooltip.</param>
/// <param name="Generation">The copy generation: 0 = Original, 1 = Copy of original, 2 = Copy of copy, 3 = Tattered. Defaults to 0.</param>
/// <param name="Pages">Optional list of formatted text component pages.</param>
/// <param name="Resolved">Whether dynamic text components have been resolved by the server. Defaults to <see langword="false"/>.</param>
[UsedImplicitly]
public record WrittenBookContentComponent(
    BookTitle Title,
    string Author,
    int Generation = WrittenBookContentComponent.OriginalGeneration,
    List<WrittenBookPage>? Pages = null,
    bool Resolved = false
) : ICompoundComponent<WrittenBookContentComponent>
{
    [PublicAPI]
    public const int OriginalGeneration = 0;
    [PublicAPI]
    public const int CopyOfOriginalGeneration = 1;
    [PublicAPI]
    public const int CopyOfCopyGeneration = 2;
    [PublicAPI]
    public const int TatteredGeneration = 3;

    /// <inheritdoc/>
    public static string ComponentId => "minecraft:written_book_content";

    /// <summary>
    /// Parses a <see cref="WrittenBookContentComponent"/> from an SNBT node representation.
    /// </summary>
    /// <param name="compound">The SNBT node to parse, which must be an <see cref="SnbtCompound"/>.</param>
    /// <returns>A populated <see cref="WrittenBookContentComponent"/> instance.</returns>
    /// <example>
    /// <code>
    /// var node = SnbtParser.Parse("{title: \"Chronicles\", author: \"Steve\", generation: 0, pages: [\"Page 1\"]}");
    /// var component = WrittenBookContentComponent.Parse(node);
    /// </code>
    /// </example>
    public static WrittenBookContentComponent Parse(SnbtCompound compound)
    {

        var titleNode = compound.GetNode("title")
            ?? throw new ArgumentException("Written book content component is missing required 'title' tag.");

        List<WrittenBookPage>? pages = null;
        if (compound.GetNode("pages") is SnbtList list)
        {
            pages = new List<WrittenBookPage>(list.Items.Count);
            foreach (var pageNode in list.Items)
                pages.Add(WrittenBookPage.Parse(pageNode));
        }

        return new WrittenBookContentComponent(
            Title: BookTitle.Parse(titleNode),
            Author: compound.GetString("author", string.Empty),
            Generation: compound.GetInt("generation"),
            Pages: pages,
            Resolved: compound.GetBool("resolved")
        );
    }

    /// <summary>
    /// Serializes the component into an SNBT compound node.
    /// </summary>
    /// <returns>An <see cref="ISnbtNode"/> representing the written book metadata and pages.</returns>
    public ISnbtNode ToSnbt()
    {
        var builder = Snbt.Compound()
            .Put("title", Title.ToSnbt())
            .Put("author", Author)
            .PutOptional("generation", Generation, OriginalGeneration)
            .PutOptional("resolved", Resolved, false);

        if (Pages is { Count: > 0 })
        {
            builder.PutList("pages", list =>
            {
                foreach (var page in Pages)
                    list.Add(page.ToSnbt());
            });
        }
        else
        {
            builder.Put("pages", new SnbtList());
        }

        return builder.Build();
    }
}