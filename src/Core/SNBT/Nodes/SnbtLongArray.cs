using Core.SNBT.Interfaces;

namespace Core.SNBT.Nodes;

/// <summary>
/// Represents an SNBT 64-bit integer array node formatted as [L; &lt;items&gt;].
/// </summary>
public sealed record SnbtLongArray : SnbtArrayNode
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SnbtLongArray"/> record.
    /// </summary>
    /// <param name="items">The list of long integer nodes. Long, int, short, and byte tags are permitted.</param>
    public SnbtLongArray(List<ISnbtNode> items) : base('L', items, ValidateItem) { }

    private static void ValidateItem(ISnbtNode item, int index)
    {
        if (item is not (SnbtLong or SnbtInt or SnbtShort or SnbtByte))
        {
            throw new ArgumentException(
                $"Long arrays can only contain long, int, short, or byte tags per SNBT specification. Element at index {index} is of type '{item.GetType().Name}'.",
                nameof(item));
        }
    }
}