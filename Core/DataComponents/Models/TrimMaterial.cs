
using Core.DataComponents.Models.Interfaces;
using Core.SNBT;
using Core.SNBT.Nodes;

using Core.SNBT.Interfaces;

namespace Core.DataComponents.Models;

/// <summary>
/// Represents an inline armor trim material definition.
/// </summary>
/// <param name="AssetName">The asset identifier used in armor trim textures (e.g. <c>quartz</c>, <c>gold</c>).</param>
/// <param name="Ingredient">The resource identifier of the ingredient item (e.g. <c>minecraft:quartz</c>).</param>
/// <param name="ItemModelIndex">The model index float modifier used for the armor item model.</param>
/// <param name="Description">Optional text component shown in tooltips for this trim material.</param>
/// <param name="OverrideArmorAssets">Optional map of armor material overrides to custom texture asset names.</param>
public record TrimMaterial(
    string AssetName,
    string Ingredient,
    float ItemModelIndex,
    ISnbtNode? Description = null,
    Dictionary<string, string>? OverrideArmorAssets = null
): ICompoundModel<TrimMaterial>
{
    /// <summary>
    /// Parses a <see cref="TrimMaterial"/> from an SNBT compound node.
    /// </summary>
    /// <param name="compound">The SNBT compound containing trim material fields.</param>
    /// <returns>A populated <see cref="TrimMaterial"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown when required fields are missing.</exception>
    public static TrimMaterial Parse(SnbtCompound compound)
    {
        Dictionary<string, string>? overrideAssets = null;
        if (compound.GetNode("override_armor_assets") is SnbtCompound overrides)
        {
            overrideAssets = new Dictionary<string, string>(overrides.Tags.Count);
            foreach (var (k, v) in overrides.Tags)
                if (v is SnbtString s) overrideAssets[k] = s.Value;
        }

        return new TrimMaterial(
            AssetName: compound.GetString("asset_name"),
            Ingredient: compound.GetString("ingredient"),
            ItemModelIndex: compound.GetFloat("item_model_index"),
            Description: compound.GetNode("description"),
            OverrideArmorAssets: overrideAssets
        );
    }

    /// <summary>
    /// Serializes the trim material definition into an SNBT compound node.
    /// </summary>
    /// <returns>An <see cref="ISnbtNode"/> representing the trim material compound.</returns>
    public ISnbtNode ToSnbt()
    {
        var builder = Snbt.Compound()
            .Put("asset_name", AssetName)
            .Put("ingredient", Ingredient)
            .Put("item_model_index", ItemModelIndex)
            .PutOptional("description", Description);

        if (OverrideArmorAssets is not { Count: > 0 })
            return builder.Build();

        var overrides = Snbt.Compound();
        foreach (var (k, v) in OverrideArmorAssets)
            overrides.Put(k, v);

        builder.Put("override_armor_assets", overrides.Build());

        return builder.Build();
    }
}