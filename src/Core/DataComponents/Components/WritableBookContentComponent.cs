
using Core.DataComponents.Models;
using Core.SNBT;

using Core.SNBT.Interfaces;
using Core.SNBT.Nodes;
using JetBrains.Annotations;

using Core.DataComponents.Interfaces;

namespace Core.DataComponents.Components;

/// <summary>
/// Stores the plain text page contents of a Book &amp; Quill (<c>minecraft:writable_book_content</c>).
/// </summary>
/// <param name="Pages">The list of pages written in the book.</param>
[UsedImplicitly]
public record WritableBookContentComponent(
    List<WritableBookPage> Pages
) : ICompoundComponent<WritableBookContentComponent>
{
    /// <inheritdoc/>
    public static string ComponentId => "minecraft:writable_book_content";

    /// <summary>
    /// Initializes a new instance of the <see cref="WritableBookContentComponent"/> record with plain string pages.
    /// </summary>
    /// <param name="pages">The text content for each page.</param>
    public WritableBookContentComponent(params string[] pages)
        : this(pages.Select(p => new WritableBookPage(p)).ToList())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WritableBookContentComponent"/> record from page models.
    /// </summary>
    /// <param name="pages">The page instances to store.</param>
    public WritableBookContentComponent(params WritableBookPage[] pages)
        : this(pages.ToList())
    {
    }

    /// <summary>
    /// Parses a <see cref="WritableBookContentComponent"/> from an SNBT node representation.
    /// </summary>
    /// <param name="compound">The SNBT node to parse, which must be an <see cref="SnbtCompound"/> containing a <c>pages</c> list.</param>
    /// <returns>A populated <see cref="WritableBookContentComponent"/> instance.</returns>
    /// <example>
    /// <code>
    /// var node = SnbtParser.Parse("{pages: [\"Page 1 text\", \"Page 2 text\"]}");
    /// var component = WritableBookContentComponent.Parse(node);
    /// </code>
    /// </example>
    public static WritableBookContentComponent Parse(SnbtCompound compound)
    {

        var pages = new List<WritableBookPage>();
        if (compound.GetNode("pages") is not SnbtList list)
            return new WritableBookContentComponent(pages);

        foreach (var pageNode in list.Items)
            pages.Add(WritableBookPage.Parse(pageNode));

        return new WritableBookContentComponent(pages);
    }

    /// <summary>
    /// Serializes the component into an SNBT compound node.
    /// </summary>
    /// <returns>An <see cref="ISnbtNode"/> representing the writable book compound.</returns>
    public ISnbtNode ToSnbt()
    {
        return Snbt.Compound()
            .PutList("pages", list =>
            {
                foreach (var page in Pages)
                    list.Add(page.ToSnbt());
            })
            .Build();
    }
}