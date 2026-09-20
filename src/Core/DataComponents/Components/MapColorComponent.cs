using Core.SNBT.Interfaces;
using Core.SNBT.Nodes;
using Core.Utils;
using JetBrains.Annotations;

using Core.DataComponents.Interfaces;

namespace Core.DataComponents.Components;

/// <summary>
/// Specifies the color tint code applied to the map markings via the <c>minecraft:map_color</c> color provider (<c>minecraft:map_color</c>).
/// Primarily used by filled map item models.
/// </summary>
/// <param name="Rgb">The packed 24-bit RGB integer color code (<c>0xRRGGBB</c>).</param>
[UsedImplicitly]
public record MapColorComponent(
    int Rgb
) : IParsableComponent<MapColorComponent>
{
    /// <inheritdoc/>
    public static string ComponentId => "minecraft:map_color";

    /// <summary>
    /// Initializes a new instance of the <see cref="MapColorComponent"/> record using normalized RGB float components.
    /// </summary>
    /// <param name="r">Red component in the range [0.0, 1.0].</param>
    /// <param name="g">Green component in the range [0.0, 1.0].</param>
    /// <param name="b">Blue component in the range [0.0, 1.0].</param>
    public MapColorComponent(float r, float g, float b) : this(ColorUtils.PackRgb(r, g, b))
    {
    }

    /// <summary>
    /// Parses a <see cref="MapColorComponent"/> from an SNBT node representation.
    /// </summary>
    /// <param name="node">The SNBT node to parse, which must be an <see cref="SnbtInt"/> or <see cref="SnbtLong"/>.</param>
    /// <returns>A populated <see cref="MapColorComponent"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="node"/> is not an integer node.</exception>
    /// <example>
    /// <code>
    /// var node = SnbtParser.Parse("16711680");
    /// var component = MapColorComponent.Parse(node);
    /// </code>
    /// </example>
    public static MapColorComponent Parse(ISnbtNode node)
    {
        return node switch
        {
            SnbtInt intNode => new MapColorComponent(intNode.Value),
            SnbtLong longNode => new MapColorComponent((int)longNode.Value),
            _ => throw new ArgumentException("Map color component must be an integer node.")
        };
    }

    /// <summary>
    /// Serializes the component into an SNBT integer node.
    /// </summary>
    /// <returns>An <see cref="ISnbtNode"/> containing the packed RGB color value.</returns>
    public ISnbtNode ToSnbt() => new SnbtInt(Rgb);

    /// <summary>
    /// Implicitly converts a packed integer color into a <see cref="MapColorComponent"/>.
    /// </summary>
    /// <param name="rgb">The packed 24-bit RGB integer color.</param>
    public static implicit operator MapColorComponent(int rgb) => new(rgb);
}