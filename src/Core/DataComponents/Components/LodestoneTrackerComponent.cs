using Core.DataComponents.Models;
using Core.SNBT;
using Core.SNBT.Nodes;

using Core.SNBT.Interfaces;
using JetBrains.Annotations;

using Core.DataComponents.Interfaces;

namespace Core.DataComponents.Components;

/// <summary>
/// Stores tracking information for a compass pointing toward a lodestone (<c>minecraft:lodestone_tracker</c>).
/// </summary>
/// <remarks>
/// When present, the item name changes to "Lodestone Compass". If <see cref="Target"/> is omitted, the compass needle spins randomly.
/// </remarks>
/// <param name="Target">Optional target containing coordinates and dimension of the lodestone.</param>
/// <param name="Tracked">Whether the component is automatically removed when the target lodestone is broken. Defaults to <see langword="true"/>.</param>
[UsedImplicitly]
public record LodestoneTrackerComponent(
    LodestoneTarget? Target = null,
    bool Tracked = true
) : ICompoundComponent<LodestoneTrackerComponent>
{
    /// <inheritdoc/>
    public static string ComponentId => "minecraft:lodestone_tracker";

    /// <summary>
    /// Initializes a new instance of the <see cref="LodestoneTrackerComponent"/> record pointing to specific coordinates.
    /// </summary>
    /// <param name="x">The X block coordinate.</param>
    /// <param name="y">The Y block coordinate.</param>
    /// <param name="z">The Z block coordinate.</param>
    /// <param name="dimension">The dimension identifier (e.g., <c>minecraft:overworld</c>).</param>
    /// <param name="tracked">Whether the compass is actively tracked and removed upon lodestone destruction.</param>
    public LodestoneTrackerComponent(int x, int y, int z, string dimension, bool tracked = true)
        : this(new LodestoneTarget(x, y, z, dimension), tracked)
    {
    }

    /// <summary>
    /// Parses a <see cref="LodestoneTrackerComponent"/> from an SNBT node representation.
    /// </summary>
    /// <param name="compound">The SNBT node to parse, which must be an <see cref="SnbtCompound"/>.</param>
    /// <returns>A populated <see cref="LodestoneTrackerComponent"/> instance.</returns>
    /// <example>
    /// <code>
    /// var node = SnbtParser.Parse("{target: {pos: [I; 1, 2, 3], dimension: \"minecraft:overworld\"}, tracked: 1b}");
    /// var component = LodestoneTrackerComponent.Parse(node);
    /// </code>
    /// </example>
    public static LodestoneTrackerComponent Parse(SnbtCompound compound)
    {

        LodestoneTarget? target = null;
        if (compound.GetNode("target") is SnbtCompound targetComp)
            target = LodestoneTarget.Parse(targetComp);

        return new LodestoneTrackerComponent(
            Target: target,
            Tracked: compound.GetBool("tracked", true)
        );
    }

    /// <summary>
    /// Serializes the component into an SNBT compound node.
    /// </summary>
    /// <returns>An <see cref="ISnbtNode"/> representing the lodestone tracker configuration.</returns>
    public ISnbtNode ToSnbt()
    {
        var builder = Snbt.Compound()
            .PutOptional("tracked", Tracked, true);

        if (Target != null)
            builder.Put("target", Target.ToSnbt());

        return builder.Build();
    }
}