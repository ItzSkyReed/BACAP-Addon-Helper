using Core.SNBT;
using Core.SNBT.Interfaces;
using Core.SNBT.Nodes;
using JetBrains.Annotations;

using Core.DataComponents.Interfaces;

namespace Core.DataComponents.Components;

/// <summary>
/// Represents custom non-vanilla NBT data attached to an item stack (<c>minecraft:custom_data</c>).
/// </summary>
/// <param name="Tag">The underlying SNBT compound containing arbitrary user-defined keys and values.</param>
[UsedImplicitly]
public record CustomDataComponent(
    SnbtCompound Tag
) : IParsableComponent<CustomDataComponent>
{
    /// <inheritdoc/>
    public static string ComponentId => "minecraft:custom_data";

    /// <summary>
    /// Initializes a new instance of the <see cref="CustomDataComponent"/> record with key-value pairs.
    /// </summary>
    /// <param name="tags">The dictionary mapping tag names to their respective SNBT nodes.</param>
    public CustomDataComponent(Dictionary<string, ISnbtNode> tags) : this(new SnbtCompound(tags))
    {
    }

    /// <summary>
    /// Parses a <see cref="CustomDataComponent"/> from an SNBT node representation.
    /// </summary>
    /// <param name="node">The SNBT node (either <see cref="SnbtCompound"/> or <see cref="SnbtString"/>).</param>
    /// <returns>The constructed <see cref="CustomDataComponent"/> instance.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="node"/> is neither an <see cref="SnbtCompound"/> nor a valid stringified compound.
    /// </exception>
    /// <example>
    /// <code>
    /// var node = SnbtParser.Parse("{foo: 1, bar: \"value\"}");
    /// var component = CustomDataComponent.Parse(node);
    /// </code>
    /// </example>
    public static CustomDataComponent Parse(ISnbtNode node)
    {
        switch (node)
        {
            case SnbtCompound compound:
                return new CustomDataComponent(compound);

            case SnbtString stringNode:
            {
                var parsed = SnbtParser.Parse(stringNode.Value);
                if (parsed is SnbtCompound parsedCompound)
                    return new CustomDataComponent(parsedCompound);

                throw new ArgumentException("Parsed custom data string is not a valid SNBT compound.");
            }

            default:
                throw new ArgumentException("Custom data component must be either a compound or an SNBT string representation of a compound.");
        }
    }

    /// <summary>
    /// Serializes the component into an SNBT node structure.
    /// </summary>
    /// <returns>The <see cref="SnbtCompound"/> containing the custom tag hierarchy.</returns>
    public ISnbtNode ToSnbt() => Tag;
}