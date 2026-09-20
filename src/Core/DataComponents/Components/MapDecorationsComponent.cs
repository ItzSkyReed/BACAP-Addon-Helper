using Core.DataComponents.Models;
using Core.SNBT;
using Core.SNBT.Interfaces;
using Core.SNBT.Nodes;
using JetBrains.Annotations;

using Core.DataComponents.Interfaces;

namespace Core.DataComponents.Components;

/// <summary>
/// Contains a collection of marker icons and points of interest displayed on a filled map (<c>minecraft:map_decorations</c>).
/// </summary>
/// <param name="Decorations">A map of arbitrary unique string keys to their corresponding <see cref="MapDecoration"/> definitions.</param>
[UsedImplicitly]
public record MapDecorationsComponent(
    Dictionary<string, MapDecoration> Decorations
) : ICompoundComponent<MapDecorationsComponent>
{
    /// <inheritdoc/>
    public static string ComponentId => "minecraft:map_decorations";

    /// <summary>
    /// Initializes an empty instance of the <see cref="MapDecorationsComponent"/> record.
    /// </summary>
    public MapDecorationsComponent() : this(new Dictionary<string, MapDecoration>())
    {
    }

    /// <summary>
    /// Parses a <see cref="MapDecorationsComponent"/> from an SNBT node representation.
    /// </summary>
    /// <param name="compound">The SNBT node to parse, which must be an <see cref="SnbtCompound"/>.</param>
    /// <returns>A populated <see cref="MapDecorationsComponent"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="compound"/> is not an <see cref="SnbtCompound"/>.</exception>
    /// <example>
    /// <code>
    /// var node = SnbtParser.Parse("{mansion_icon: {type: \"mansion\", x: 1200.0d, z: -450.0d, rotation: 180.0f}}");
    /// var component = MapDecorationsComponent.Parse(node);
    /// </code>
    /// </example>
    public static MapDecorationsComponent Parse(SnbtCompound compound)
    {

        var decorations = new Dictionary<string, MapDecoration>(compound.Tags.Count);

        foreach (var (key, decorationNode) in compound.Tags)
        {
            if (decorationNode is SnbtCompound decCompound)
                decorations[key] = MapDecoration.Parse(decCompound);
        }

        return new MapDecorationsComponent(decorations);
    }

    /// <summary>
    /// Serializes the component into an SNBT compound node.
    /// </summary>
    /// <returns>An <see cref="ISnbtNode"/> representing the map decorations map.</returns>
    public ISnbtNode ToSnbt()
    {
        var builder = Snbt.Compound();

        foreach (var (key, decoration) in Decorations)
            builder.Put(key, decoration.ToSnbt());

        return builder.Build();
    }
}