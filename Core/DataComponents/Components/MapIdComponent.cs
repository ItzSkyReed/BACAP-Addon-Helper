using Core.SNBT.Interfaces;
using Core.SNBT.Nodes;
using JetBrains.Annotations;

using Core.DataComponents.Interfaces;

namespace Core.DataComponents.Components;

/// <summary>
/// Represents the map data number identifying the saved map state file (<c>minecraft:map_id</c>).
/// </summary>
/// <remarks>
/// Links a filled map item to its corresponding saved map texture, banners, and frame markers.
/// </remarks>
/// <param name="Id">The numeric identifier of the shared map state.</param>
[UsedImplicitly]
public record MapIdComponent(
    int Id
) : IParsableComponent<MapIdComponent>
{
    /// <inheritdoc/>
    public static string ComponentId => "minecraft:map_id";

    /// <summary>
    /// Parses a <see cref="MapIdComponent"/> from an SNBT node representation.
    /// </summary>
    /// <param name="node">The SNBT node to parse, which must be an <see cref="SnbtInt"/> or <see cref="SnbtLong"/>.</param>
    /// <returns>A populated <see cref="MapIdComponent"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="node"/> is not an integer node.</exception>
    /// <example>
    /// <code>
    /// var node = SnbtParser.Parse("42");
    /// var component = MapIdComponent.Parse(node);
    /// </code>
    /// </example>
    public static MapIdComponent Parse(ISnbtNode node)
    {
        return node switch
        {
            SnbtInt intNode => new MapIdComponent(intNode.Value),
            SnbtLong longNode => new MapIdComponent((int)longNode.Value),
            _ => throw new ArgumentException("Map ID component must be an integer node.")
        };
    }

    /// <summary>
    /// Serializes the component into an SNBT integer node.
    /// </summary>
    /// <returns>An <see cref="ISnbtNode"/> containing the numeric map ID.</returns>
    public ISnbtNode ToSnbt() => new SnbtInt(Id);

    /// <summary>
    /// Implicitly converts an integer map identifier into a <see cref="MapIdComponent"/>.
    /// </summary>
    /// <param name="id">The numeric map ID.</param>
    public static implicit operator MapIdComponent(int id) => new(id);
}