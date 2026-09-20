
using Core.DataComponents.Models.Interfaces;
using Core.SNBT;
using Core.SNBT.Nodes;

using Core.SNBT.Interfaces;

namespace Core.DataComponents.Models;

/// <summary>
/// Represents a single map icon or marker decoration displayed on a filled map.
/// </summary>
/// <param name="Type">The icon type identifier (e.g., <c>mansion</c>, <c>monument</c>, <c>red_marker</c>, <c>target_x</c>, <c>banner_blue</c>).</param>
/// <param name="X">The world X coordinate of the marker position.</param>
/// <param name="Z">The world Z coordinate of the marker position.</param>
/// <param name="Rotation">The icon rotation in degrees clockwise from north (0.0 to 360.0).</param>
public record MapDecoration(
    string Type,
    double X,
    double Z,
    float Rotation
) : ICompoundModel<MapDecoration>
{
    /// <summary>
    /// Parses a <see cref="MapDecoration"/> from an SNBT compound node.
    /// </summary>
    /// <param name="compound">The SNBT compound node containing decoration fields.</param>
    /// <returns>A populated <see cref="MapDecoration"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown when required fields are missing.</exception>
    public static MapDecoration Parse(SnbtCompound compound)
    {

        return new MapDecoration(
            Type: compound.GetString("type"),
            X: compound.GetDouble("x"),
            Z: compound.GetDouble("z"),
            Rotation: compound.GetFloat("rotation")
        );
    }

    /// <summary>
    /// Serializes the decoration into an SNBT compound node.
    /// </summary>
    /// <returns>An <see cref="ISnbtNode"/> representing the decoration compound.</returns>
    public ISnbtNode ToSnbt() => Snbt.Compound()
        .Put("type", Type)
        .Put("x", X)
        .Put("z", Z)
        .Put("rotation", Rotation)
        .Build();
}