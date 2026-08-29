
using Core.SNBT;
using Core.SNBT.Nodes;

using Core.SNBT.Interfaces;
using JetBrains.Annotations;

using Core.DataComponents.Interfaces;

namespace Core.DataComponents.Components;

/// <summary>
/// Stores the item stack contained inside a sulfur cube (<c>minecraft:sulfur_cube_content</c>).
/// Doubles as the body armor slot on a sulfur cube entity and displays a "Contains: &lt;item&gt;" tooltip.
/// </summary>
/// <param name="Id">The resource identifier of the contained item.</param>
/// <param name="Count">The stack size count (1 to 99). Defaults to 1.</param>
/// <param name="CustomComponents">Optional map of additional non-default data components attached to the contained item.</param>
[UsedImplicitly]
public record SulfurCubeContentComponent(
    string Id,
    int Count = 1,
    SnbtCompound? CustomComponents = null
) : ICompoundComponent<SulfurCubeContentComponent>
{
    /// <inheritdoc/>
    public static string ComponentId => "minecraft:sulfur_cube_content";

    /// <summary>
    /// Parses a <see cref="SulfurCubeContentComponent"/> from an SNBT node representation.
    /// </summary>
    /// <param name="compound">The SNBT node to parse, which must be an <see cref="SnbtCompound"/>.</param>
    /// <returns>A populated <see cref="SulfurCubeContentComponent"/> instance.</returns>
    /// <example>
    /// <code>
    /// var node = SnbtParser.Parse("{id: \"minecraft:diamond\", count: 3}");
    /// var component = SulfurCubeContentComponent.Parse(node);
    /// </code>
    /// </example>
    public static SulfurCubeContentComponent Parse(SnbtCompound compound)
    {

        return new SulfurCubeContentComponent(
            Id: compound.GetString("id"),
            Count: compound.GetInt("count", 1),
            CustomComponents: compound.GetNode("components") as SnbtCompound
        );
    }

    /// <summary>
    /// Serializes the component into an SNBT compound node representing a single item stack.
    /// </summary>
    /// <returns>An <see cref="ISnbtNode"/> containing the item stack definition.</returns>
    public ISnbtNode ToSnbt()
    {
        var builder = Snbt.Compound()
            .Put("id", Id)
            .PutOptional("count", Count, 1);

        if (CustomComponents != null)
            builder.Put("components", CustomComponents);

        return builder.Build();
    }

    /// <summary>
    /// Implicitly converts an item identifier string into a <see cref="SulfurCubeContentComponent"/> with count 1.
    /// </summary>
    /// <param name="id">The item resource identifier.</param>
    public static implicit operator SulfurCubeContentComponent(string id) => new(id);
}