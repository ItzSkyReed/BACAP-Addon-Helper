using Core.SNBT.Interfaces;
using Core.SNBT.Nodes;
using Core.Utils;
using JetBrains.Annotations;
using Core.DataComponents.Interfaces;

namespace Core.DataComponents.Components;

/// <summary>
/// Represents the custom RGB color tint applied to a dyeable item (<c>minecraft:dyed_color</c>).
/// </summary>
/// <param name="Rgb">The packed 24-bit RGB integer color value (<c>0xRRGGBB</c>).</param>
[UsedImplicitly]
public record DyedColorComponent(
    int Rgb
) : IParsableComponent<DyedColorComponent>
{
    /// <inheritdoc/>
    public static string ComponentId => "minecraft:dyed_color";

    /// <summary>
    /// Initializes a new instance of the <see cref="DyedColorComponent"/> record using normalized RGB float components.
    /// </summary>
    /// <param name="r">Red component in the range [0.0, 1.0].</param>
    /// <param name="g">Green component in the range [0.0, 1.0].</param>
    /// <param name="b">Blue component in the range [0.0, 1.0].</param>
    public DyedColorComponent(float r, float g, float b) : this(ColorUtils.PackRgb(r, g, b))
    {
    }

    /// <summary>
    /// Parses a <see cref="DyedColorComponent"/> from an SNBT node representation.
    /// Supports packed integers, longs, or 3-element float lists ([r, g, b]).
    /// </summary>
    /// <param name="node">The SNBT node to parse (<see cref="SnbtInt"/>, <see cref="SnbtLong"/>, or <see cref="SnbtList"/>).</param>
    /// <returns>A populated <see cref="DyedColorComponent"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="node"/> is not a valid color format.</exception>
    /// <example>
    /// <code>
    /// // From integer RGB code:
    /// var node1 = SnbtParser.Parse("8388403");
    /// var component1 = DyedColorComponent.Parse(node1);
    ///
    /// // From float RGB list:
    /// var node2 = SnbtParser.Parse("[0.5f, 1.0f, 0.2f]");
    /// var component2 = DyedColorComponent.Parse(node2);
    /// </code>
    /// </example>
    public static DyedColorComponent Parse(ISnbtNode node)
    {
        switch (node)
        {
            case SnbtInt intNode:
                return new DyedColorComponent(intNode.Value);

            case SnbtLong longNode:
                return new DyedColorComponent((int)longNode.Value);

            case SnbtList list when list.Items.Count >= 3:
            {
                var r = GetFloat(list.Items[0]);
                var g = GetFloat(list.Items[1]);
                var b = GetFloat(list.Items[2]);
                return new DyedColorComponent(ColorUtils.PackRgb(r, g, b));
            }

            default:
                throw new ArgumentException("Dyed color component must be an integer, long, or a list of 3 float values.");
        }
    }

    /// <summary>
    /// Serializes the component into an SNBT integer node.
    /// </summary>
    /// <returns>An <see cref="ISnbtNode"/> containing the packed RGB integer.</returns>
    public ISnbtNode ToSnbt() => new SnbtInt(Rgb);

    private static float GetFloat(ISnbtNode node) => node switch
    {
        SnbtFloat f => f.Value,
        SnbtDouble d => (float)d.Value,
        SnbtInt i => i.Value,
        _ => 0.0f
    };
}