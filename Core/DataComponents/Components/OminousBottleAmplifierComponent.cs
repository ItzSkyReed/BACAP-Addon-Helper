using Core.SNBT.Interfaces;
using Core.SNBT.Nodes;
using JetBrains.Annotations;

using Core.DataComponents.Interfaces;

namespace Core.DataComponents.Components;

/// <summary>
/// Specifies the Bad Omen effect amplifier level applied when consuming an ominous bottle (<c>minecraft:ominous_bottle_amplifier</c>).
/// </summary>
/// <remarks>
/// Must be an integer between 0 and 4 (inclusive), corresponding to Bad Omen I through V.
/// The duration is fixed by the game to 120,000 ticks.
/// </remarks>
/// <param name="Amplifier">The effect amplifier index (0 to 4).</param>
[UsedImplicitly]
public record OminousBottleAmplifierComponent(
    int Amplifier
) : IParsableComponent<OminousBottleAmplifierComponent>
{
    /// <inheritdoc/>
    public static string ComponentId => "minecraft:ominous_bottle_amplifier";

    /// <summary>
    /// Parses an <see cref="OminousBottleAmplifierComponent"/> from an SNBT node representation.
    /// </summary>
    /// <param name="node">The SNBT node to parse, which must be an <see cref="SnbtInt"/>, <see cref="SnbtByte"/>, or <see cref="SnbtLong"/>.</param>
    /// <returns>A populated <see cref="OminousBottleAmplifierComponent"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="node"/> is not an integer node.</exception>
    /// <example>
    /// <code>
    /// var node = SnbtParser.Parse("2");
    /// var component = OminousBottleAmplifierComponent.Parse(node);
    /// </code>
    /// </example>
    public static OminousBottleAmplifierComponent Parse(ISnbtNode node)
    {
        return node switch
        {
            SnbtInt intNode => new OminousBottleAmplifierComponent(intNode.Value),
            SnbtByte byteNode => new OminousBottleAmplifierComponent(byteNode.Value),
            SnbtLong longNode => new OminousBottleAmplifierComponent((int)longNode.Value),
            _ => throw new ArgumentException("Ominous bottle amplifier component must be an integer node.")
        };
    }

    /// <summary>
    /// Serializes the component into an SNBT integer node.
    /// </summary>
    /// <returns>An <see cref="ISnbtNode"/> containing the amplifier integer.</returns>
    public ISnbtNode ToSnbt() => new SnbtInt(Amplifier);

    /// <summary>
    /// Implicitly converts an integer amplifier into an <see cref="OminousBottleAmplifierComponent"/>.
    /// </summary>
    /// <param name="amplifier">The effect amplifier (0 to 4).</param>
    public static implicit operator OminousBottleAmplifierComponent(int amplifier) => new(amplifier);
}