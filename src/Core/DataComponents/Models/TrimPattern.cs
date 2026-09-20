
using Core.DataComponents.Models.Interfaces;
using Core.SNBT;
using Core.SNBT.Nodes;

using Core.SNBT.Interfaces;

namespace Core.DataComponents.Models;

/// <summary>
/// Represents an inline armor trim pattern definition.
/// </summary>
/// <param name="AssetId">The resource location used as a prefix in sprite names and translation keys.</param>
/// <param name="Description">Text component used as the trim pattern's display name.</param>
/// <param name="Decal">Whether the pattern renders as a decal texture layer. Defaults to <see langword="false"/>.</param>
public record TrimPattern(
    string AssetId,
    ISnbtNode Description,
    bool Decal = false
): ICompoundModel<TrimPattern>
{
    /// <summary>
    /// Parses a <see cref="TrimPattern"/> from an SNBT compound node.
    /// </summary>
    /// <param name="compound">The SNBT compound containing trim pattern fields.</param>
    /// <returns>A populated <see cref="TrimPattern"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown when required fields are missing.</exception>
    public static TrimPattern Parse(SnbtCompound compound)
    {
        var description = compound.GetNode("description")
                          ?? throw new ArgumentException("Trim pattern compound is missing required 'description' tag.");

        return new TrimPattern(
            AssetId: compound.GetString("asset_id"),
            Description: description,
            Decal: compound.GetBool("decal")
        );
    }

    /// <summary>
    /// Serializes the trim pattern definition into an SNBT compound node.
    /// </summary>
    /// <returns>An <see cref="ISnbtNode"/> representing the trim pattern compound.</returns>
    public ISnbtNode ToSnbt() => Snbt.Compound()
        .Put("asset_id", AssetId)
        .Put("description", Description)
        .PutOptional("decal", Decal, false)
        .Build();
}