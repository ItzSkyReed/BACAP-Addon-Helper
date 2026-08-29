using Core.DataComponents.Interfaces;
using Core.DataComponents.Models;

using Core.SNBT.Interfaces;
using Core.SNBT.Nodes;
using JetBrains.Annotations;

namespace Core.DataComponents.Components;

/// <summary>
/// Configures the ordered list of pattern layers applied to banners and shields (<c>minecraft:banner_patterns</c>).
/// </summary>
/// <param name="Patterns">The sequential list of banner pattern layers rendered from bottom to top.</param>
[UsedImplicitly]
public record BannerPatternsComponent(
    List<BannerPatternLayer> Patterns
) : IListComponent<BannerPatternsComponent>
{
    /// <inheritdoc/>
    public static string ComponentId => "minecraft:banner_patterns";

    /// <summary>
    /// Initializes a new instance of the <see cref="BannerPatternsComponent"/> record from an array of pattern layers.
    /// </summary>
    /// <param name="patterns">The pattern layers to apply to the banner.</param>
    public BannerPatternsComponent(params BannerPatternLayer[] patterns)
        : this(patterns.ToList())
    {
    }

    /// <summary>
    /// Parses a <see cref="BannerPatternsComponent"/> from an SNBT node representation.
    /// </summary>
    /// <param name="list">The SNBT list to parse, which must be an <see cref="SnbtList"/>.</param>
    /// <returns>A populated <see cref="BannerPatternsComponent"/> instance.</returns>
    public static BannerPatternsComponent Parse(SnbtList list)
    {

        var patterns = new List<BannerPatternLayer>(list.Items.Count);
        foreach (var item in list.Items)
        {
            if (item is SnbtCompound comp)
                patterns.Add(BannerPatternLayer.Parse(comp));
        }

        return new BannerPatternsComponent(patterns);
    }

    /// <summary>
    /// Serializes the banner pattern layers into an SNBT list node.
    /// </summary>
    /// <returns>An <see cref="ISnbtNode"/> representing the list of pattern layer compounds.</returns>
    public ISnbtNode ToSnbt()
    {
        var list = new SnbtList();
        foreach (var pattern in Patterns)
            list.Items.Add(pattern.ToSnbt());

        return list;
    }
}