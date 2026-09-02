
using Core.DataComponents.Models;
using Core.SNBT;
using Core.SNBT.Nodes;

using Core.SNBT.Interfaces;
using JetBrains.Annotations;

using Core.DataComponents.Interfaces;

namespace Core.DataComponents.Components;

/// <summary>
/// Represents the armor trim configuration applied to an armor piece (<c>minecraft:trim</c>).
/// </summary>
/// <param name="PatternId">Optional resource location of a registered trim pattern (e.g. <c>minecraft:host</c>).</param>
/// <param name="InlinePattern">Optional inline trim pattern definition.</param>
/// <param name="MaterialId">Optional resource location of a registered trim material (e.g. <c>minecraft:emerald</c>).</param>
/// <param name="InlineMaterial">Optional inline trim material definition.</param>
/// <param name="ShowInTooltip">Whether to display trim details in the item tooltip. Defaults to <see langword="true"/>.</param>
[UsedImplicitly]
public record TrimComponent(
    string? PatternId = null,
    TrimPattern? InlinePattern = null,
    string? MaterialId = null,
    TrimMaterial? InlineMaterial = null,
    bool ShowInTooltip = true
) : ICompoundComponent<TrimComponent>
{
    /// <inheritdoc/>
    public static string ComponentId => "minecraft:trim";

    /// <summary>
    /// Initializes a new instance of the <see cref="TrimComponent"/> record referencing registered pattern and material IDs.
    /// </summary>
    /// <param name="patternId">The resource identifier of the trim pattern.</param>
    /// <param name="materialId">The resource identifier of the trim material.</param>
    /// <param name="showInTooltip">Whether the trim is visible in tooltips.</param>
    public TrimComponent(string patternId, string materialId, bool showInTooltip = true)
        : this(PatternId: patternId, InlinePattern: null, MaterialId: materialId, InlineMaterial: null, ShowInTooltip: showInTooltip)
    {
    }

    /// <summary>
    /// Parses a <see cref="TrimComponent"/> from an SNBT node representation.
    /// </summary>
    /// <param name="compound">The SNBT node to parse, which must be an <see cref="SnbtCompound"/>.</param>
    /// <returns>A populated <see cref="TrimComponent"/> instance.</returns>
    /// <example>
    /// <code>
    /// var node = SnbtParser.Parse("{pattern: \"minecraft:host\", material: \"minecraft:emerald\"}");
    /// var component = TrimComponent.Parse(node);
    /// </code>
    /// </example>
    public static TrimComponent Parse(SnbtCompound compound)
    {
        var patternNode = compound.GetNode("pattern")
            ?? throw new ArgumentException("Trim component is missing required 'pattern' tag.");

        var materialNode = compound.GetNode("material")
            ?? throw new ArgumentException("Trim component is missing required 'material' tag.");

        string? patternId = null;
        TrimPattern? inlinePattern = null;
        switch (patternNode)
        {
            case SnbtString pStr:
                patternId = pStr.Value;
                break;

            case SnbtCompound pComp:
                inlinePattern = TrimPattern.Parse(pComp);
                break;

            default:
                throw new ArgumentException("Trim 'pattern' must be a string identifier or a compound definition.");
        }

        string? materialId = null;
        TrimMaterial? inlineMaterial = null;
        switch (materialNode)
        {
            case SnbtString mStr:
                materialId = mStr.Value;
                break;

            case SnbtCompound mComp:
                inlineMaterial = TrimMaterial.Parse(mComp);
                break;

            default:
                throw new ArgumentException("Trim 'material' must be a string identifier or a compound definition.");
        }

        return new TrimComponent(
            PatternId: patternId,
            InlinePattern: inlinePattern,
            MaterialId: materialId,
            InlineMaterial: inlineMaterial,
            ShowInTooltip: compound.GetBool("show_in_tooltip", true)
        );
    }

    /// <summary>
    /// Serializes the component into an SNBT compound node.
    /// </summary>
    /// <returns>An <see cref="ISnbtNode"/> representing the trim compound.</returns>
    /// <exception cref="InvalidOperationException">Thrown when neither pattern nor material is configured.</exception>
    public ISnbtNode ToSnbt()
    {
        var builder = Snbt.Compound();

        if (MaterialId != null) builder.Put("material", MaterialId);
        else if (InlineMaterial != null) builder.Put("material", InlineMaterial.ToSnbt());
        else throw new InvalidOperationException("TrimComponent must contain either a MaterialId or an InlineMaterial.");

        if (PatternId != null) builder.Put("pattern", PatternId);
        else if (InlinePattern != null) builder.Put("pattern", InlinePattern.ToSnbt());
        else throw new InvalidOperationException("TrimComponent must contain either a PatternId or an InlinePattern.");


        builder.PutOptional("show_in_tooltip", ShowInTooltip, true);

        return builder.Build();
    }
}