using Core.DataComponents.Models.Interfaces;
using Core.SNBT;
using Core.SNBT.Nodes;

using Core.SNBT.Interfaces;

namespace Core.DataComponents.Models;

/// <summary>
/// Represents the target coordinates and dimension of a lodestone block.
/// </summary>
/// <param name="X">The X coordinate of the lodestone.</param>
/// <param name="Y">The Y coordinate of the lodestone.</param>
/// <param name="Z">The Z coordinate of the lodestone.</param>
/// <param name="Dimension">The resource location of the dimension (e.g. <c>minecraft:overworld</c>).</param>
public record LodestoneTarget(
    int X,
    int Y,
    int Z,
    string Dimension
) : ICompoundModel<LodestoneTarget>
{
    /// <summary>
    /// Parses a <see cref="LodestoneTarget"/> from an SNBT compound node.
    /// </summary>
    /// <param name="compound">The compound node containing <c>pos</c> and <c>dimension</c>.</param>
    /// <returns>A populated <see cref="LodestoneTarget"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown when <c>dimension</c> or valid coordinates in <c>pos</c> are missing.</exception>
    public static LodestoneTarget Parse(SnbtCompound compound)
    {
        var posNode = compound.GetNode("pos");
        int x, y, z;
        switch (posNode)
        {
            case SnbtIntArray { Items.Count: >= 3 } intArray:
                x = (intArray.Items[0] as SnbtInt)?.Value ?? 0;
                y = (intArray.Items[1] as SnbtInt)?.Value ?? 0;
                z = (intArray.Items[2] as SnbtInt)?.Value ?? 0;
                break;
            case SnbtList { Items.Count: >= 3 } list:
                x = (list.Items[0] as SnbtInt)?.Value ?? 0;
                y = (list.Items[1] as SnbtInt)?.Value ?? 0;
                z = (list.Items[2] as SnbtInt)?.Value ?? 0;
                break;
            default:
                throw new ArgumentException("Lodestone target is missing a valid 3-element 'pos' int array.");
        }

        return new LodestoneTarget(
            X: x,
            Y: y,
            Z: z,
            Dimension: compound.GetString("dimension")
        );
    }

    /// <summary>
    /// Serializes the lodestone target into an SNBT compound node.
    /// </summary>
    /// <returns>An <see cref="ISnbtNode"/> containing the coordinates and dimension.</returns>
    public ISnbtNode ToSnbt() => Snbt.Compound()
        .Put("pos", new SnbtIntArray([new SnbtInt(X), new SnbtInt(Y), new SnbtInt(Z)]))
        .Put("dimension", Dimension)
        .Build();
}