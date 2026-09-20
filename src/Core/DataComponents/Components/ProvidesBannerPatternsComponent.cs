using Core.SNBT;
using Core.SNBT.Interfaces;
using Core.SNBT.Nodes;
using JetBrains.Annotations;

using Core.DataComponents.Interfaces;

namespace Core.DataComponents.Components;

/// <summary>
/// Specifies the banner pattern(s) provided by this item when used in a loom (<c>minecraft:provides_banner_patterns</c>).
/// Can be a tag selector (prefixed with <c>#</c>), a single pattern ID, or a list of pattern IDs.
/// </summary>
/// <param name="Patterns">The list of pattern resource locations or a single tag selector string.</param>
[UsedImplicitly]
public record ProvidesBannerPatternsComponent(
    List<string> Patterns
) : IParsableComponent<ProvidesBannerPatternsComponent>
{
    /// <inheritdoc/>
    public static string ComponentId => "minecraft:provides_banner_patterns";

    /// <summary>
    /// Initializes a new instance of the <see cref="ProvidesBannerPatternsComponent"/> record with a single pattern ID or tag selector.
    /// </summary>
    /// <param name="patternOrTag">The pattern resource identifier or tag selector (e.g., <c>#minecraft:pattern_item/globe</c>).</param>
    public ProvidesBannerPatternsComponent(string patternOrTag) : this(new List<string> { patternOrTag })
    {
    }

    /// <summary>
    /// Parses a <see cref="ProvidesBannerPatternsComponent"/> from an SNBT node representation.
    /// Supports both a single string (ID/tag) and a list of pattern strings.
    /// </summary>
    /// <param name="node">The SNBT node to parse (<see cref="SnbtString"/> or <see cref="SnbtList"/>).</param>
    /// <returns>A populated <see cref="ProvidesBannerPatternsComponent"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="node"/> is neither a string nor a list.</exception>
    /// <example>
    /// <code>
    /// // From tag selector:
    /// var comp1 = ProvidesBannerPatternsComponent.Parse(new SnbtString("#minecraft:pattern_item/globe"));
    ///
    /// // From pattern list:
    /// var listNode = SnbtParser.Parse("[\"minecraft:globe\", \"minecraft:flow\"]");
    /// var comp2 = ProvidesBannerPatternsComponent.Parse(listNode);
    /// </code>
    /// </example>
    public static ProvidesBannerPatternsComponent Parse(ISnbtNode node)
    {
        return node switch
        {
            SnbtString str => new ProvidesBannerPatternsComponent(str.Value),
            SnbtList list => new ProvidesBannerPatternsComponent(ExtractPatternsList(list)),
            _ => throw new ArgumentException("Provides banner patterns component must be a string identifier/tag or a list of strings.")
        };
    }

    /// <summary>
    /// Serializes the component into an SNBT node.
    /// Emits a compact <see cref="SnbtString"/> if there is only one pattern entry.
    /// </summary>
    /// <returns>An <see cref="ISnbtNode"/> containing the string or list of banner patterns.</returns>
    public ISnbtNode ToSnbt()
    {
        if (Patterns.Count == 1)
            return new SnbtString(Patterns[0]);

        var builder = Snbt.List();
        foreach (var pattern in Patterns)
            builder.Add(pattern);

        return builder.Build();
    }

    /// <summary>
    /// Implicitly converts a single pattern ID or tag string into a <see cref="ProvidesBannerPatternsComponent"/>.
    /// </summary>
    /// <param name="patternOrTag">The pattern resource identifier or tag selector.</param>
    public static implicit operator ProvidesBannerPatternsComponent(string patternOrTag) => new(patternOrTag);

    private static List<string> ExtractPatternsList(SnbtList list)
    {
        var patterns = new List<string>(list.Items.Count);
        foreach (var item in list.Items)
        {
            if (item is SnbtString str)
                patterns.Add(str.Value);
        }
        return patterns;
    }
}