
using Core.DataComponents.Models;
using Core.SNBT.Interfaces;
using Core.SNBT.Nodes;
using JetBrains.Annotations;

using Core.DataComponents.Interfaces;

namespace Core.DataComponents.Components;

/// <summary>
/// Configures this item to provide a trim material in a smithing table trimming recipe (<c>minecraft:provides_trim_material</c>).
/// Can reference a registered trim material ID or define an inline <see cref="TrimMaterial"/>.
/// </summary>
/// <param name="MaterialId">Optional resource location of a registered trim material (e.g. <c>minecraft:quartz</c>).</param>
/// <param name="InlineMaterial">Optional inline trim material definition.</param>
[UsedImplicitly]
public record ProvidesTrimMaterialComponent(
    string? MaterialId = null,
    TrimMaterial? InlineMaterial = null
) : IParsableComponent<ProvidesTrimMaterialComponent>
{
    /// <inheritdoc/>
    public static string ComponentId => "minecraft:provides_trim_material";

    /// <summary>
    /// Initializes a new instance of the <see cref="ProvidesTrimMaterialComponent"/> record with a registered material ID.
    /// </summary>
    /// <param name="materialId">The resource identifier of the trim material.</param>
    public ProvidesTrimMaterialComponent(string materialId) : this(MaterialId: materialId, InlineMaterial: null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ProvidesTrimMaterialComponent"/> record with an inline material definition.
    /// </summary>
    /// <param name="inlineMaterial">The inline trim material configuration.</param>
    public ProvidesTrimMaterialComponent(TrimMaterial inlineMaterial) : this(MaterialId: null, InlineMaterial: inlineMaterial)
    {
    }

    /// <summary>
    /// Parses a <see cref="ProvidesTrimMaterialComponent"/> from an SNBT node representation.
    /// </summary>
    /// <param name="node">The SNBT node (either <see cref="SnbtString"/> or <see cref="SnbtCompound"/>).</param>
    /// <returns>A populated <see cref="ProvidesTrimMaterialComponent"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="node"/> is neither a string nor a compound.</exception>
    /// <example>
    /// <code>
    /// // From material ID string:
    /// var comp1 = ProvidesTrimMaterialComponent.Parse(new SnbtString("minecraft:quartz"));
    ///
    /// // From compound:
    /// var node = SnbtParser.Parse("{asset_name: \"quartz\", ingredient: \"minecraft:quartz\", item_model_index: 0.1f}");
    /// var comp2 = ProvidesTrimMaterialComponent.Parse(node);
    /// </code>
    /// </example>
    public static ProvidesTrimMaterialComponent Parse(ISnbtNode node)
    {
        return node switch
        {
            SnbtString str => new ProvidesTrimMaterialComponent(str.Value),
            SnbtCompound compound => new ProvidesTrimMaterialComponent(TrimMaterial.Parse(compound)),
            _ => throw new ArgumentException("Provides trim material component must be a string identifier or a compound.")
        };
    }

    /// <summary>
    /// Serializes the component into an SNBT node.
    /// </summary>
    /// <returns>An <see cref="ISnbtNode"/> containing the string ID or compound definition.</returns>
    /// <exception cref="InvalidOperationException">Thrown when neither a MaterialId nor an InlineMaterial is set.</exception>
    public ISnbtNode ToSnbt()
    {
        if (MaterialId != null)
            return new SnbtString(MaterialId);

        return InlineMaterial != null
            ? InlineMaterial.ToSnbt()
            : throw new InvalidOperationException("ProvidesTrimMaterialComponent must contain either a MaterialId or an InlineMaterial.");
    }

    /// <summary>
    /// Implicitly converts a material ID string into a <see cref="ProvidesTrimMaterialComponent"/>.
    /// </summary>
    /// <param name="materialId">The trim material resource identifier.</param>
    public static implicit operator ProvidesTrimMaterialComponent(string materialId) => new(materialId);

    /// <summary>
    /// Implicitly converts an inline <see cref="TrimMaterial"/> into a <see cref="ProvidesTrimMaterialComponent"/>.
    /// </summary>
    /// <param name="material">The trim material definition.</param>
    public static implicit operator ProvidesTrimMaterialComponent(TrimMaterial material) => new(material);
}