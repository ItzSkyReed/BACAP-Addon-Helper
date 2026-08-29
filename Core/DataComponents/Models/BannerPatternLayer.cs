using Core.DataComponents.Models.Interfaces;
using Core.SNBT;

using Core.SNBT.Interfaces;
using Core.SNBT.Nodes;

namespace Core.DataComponents.Models;

/// <summary>
/// Represents a single pattern layer applied to a banner or shield, defined by dye color and pattern ID/definition.
/// </summary>
/// <param name="Color">The dye color identifier of this pattern layer (e.g. <c>red</c>, <c>black</c>).</param>
/// <param name="PatternId">The resource location of a registered vanilla pattern (e.g. <c>minecraft:flower</c>, <c>minecraft:skull</c>).</param>
/// <param name="CustomPattern">Optional inline custom banner pattern definition.</param>
public record BannerPatternLayer(
    string Color,
    string? PatternId = null,
    CustomBannerPattern? CustomPattern = null
) : ICompoundModel<BannerPatternLayer>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BannerPatternLayer"/> record with a registered pattern identifier.
    /// </summary>
    /// <param name="color">The dye color identifier.</param>
    /// <param name="patternId">The resource location of the pattern.</param>
    public BannerPatternLayer(string color, string patternId)
        : this(color, PatternId: patternId, CustomPattern: null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BannerPatternLayer"/> record with an inline custom pattern.
    /// </summary>
    /// <param name="color">The dye color identifier.</param>
    /// <param name="customPattern">The inline custom pattern definition.</param>
    public BannerPatternLayer(string color, CustomBannerPattern customPattern)
        : this(color, PatternId: null, CustomPattern: customPattern)
    {
    }

    /// <summary>
    /// Parses a <see cref="BannerPatternLayer"/> from an SNBT compound node.
    /// </summary>
    /// <param name="compound">The SNBT compound node containing pattern layer fields.</param>
    /// <returns>A populated <see cref="BannerPatternLayer"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown when <c>color</c> or valid <c>pattern</c> is missing.</exception>
    public static BannerPatternLayer Parse(SnbtCompound compound)
    {
        var color = compound.GetString("color");
        var patternNode = compound.GetNode("pattern");

        return patternNode switch
        {
            SnbtString strNode => new BannerPatternLayer(color, PatternId: strNode.Value),
            SnbtCompound customComp => new BannerPatternLayer(color, CustomPattern: CustomBannerPattern.Parse(customComp)),
            _ => throw new ArgumentException("Banner pattern layer must contain either a string 'pattern' ID or a custom pattern compound.")
        };
    }

    /// <summary>
    /// Serializes the banner pattern layer into an SNBT compound node.
    /// </summary>
    /// <returns>An <see cref="ISnbtNode"/> representing the banner pattern layer compound.</returns>
    /// <exception cref="InvalidOperationException">Thrown when neither pattern ID nor custom pattern definition is set.</exception>
    public ISnbtNode ToSnbt()
    {
        var builder = Snbt.Compound()
            .Put("color", Color);

        if (PatternId != null)
            builder.Put("pattern", PatternId);
        else if (CustomPattern != null)
            builder.Put("pattern", CustomPattern.ToSnbt());
        else
            throw new InvalidOperationException("BannerPatternLayer must contain either a PatternId or a CustomPattern.");

        return builder.Build();
    }
}