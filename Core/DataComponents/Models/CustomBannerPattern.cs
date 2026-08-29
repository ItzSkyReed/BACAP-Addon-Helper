using Core.DataComponents.Models.Interfaces;
using Core.SNBT;

using Core.SNBT.Interfaces;
using Core.SNBT.Nodes;

namespace Core.DataComponents.Models;

/// <summary>
/// Represents an inline custom banner pattern definition specifying its sprite asset and translation key.
/// </summary>
/// <param name="AssetId">The resource location of the banner pattern texture asset.</param>
/// <param name="TranslationKey">The localization translation key used for tooltip display.</param>
public record CustomBannerPattern(
    string AssetId,
    string TranslationKey
) : ICompoundModel<CustomBannerPattern>
{
    /// <summary>
    /// Parses a <see cref="CustomBannerPattern"/> from an SNBT compound node.
    /// </summary>
    /// <param name="compound">The SNBT compound node containing pattern parameters.</param>
    /// <returns>A populated <see cref="CustomBannerPattern"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown when required fields are missing.</exception>
    public static CustomBannerPattern Parse(SnbtCompound compound)
    {
        return new CustomBannerPattern(
            AssetId: compound.GetString("asset_id"),
            TranslationKey: compound.GetString("translation_key")
        );
    }

    /// <summary>
    /// Serializes the custom banner pattern definition into an SNBT compound node.
    /// </summary>
    /// <returns>An <see cref="ISnbtNode"/> representing the pattern compound.</returns>
    public ISnbtNode ToSnbt() => Snbt.Compound()
        .Put("asset_id", AssetId)
        .Put("translation_key", TranslationKey)
        .Build();
}