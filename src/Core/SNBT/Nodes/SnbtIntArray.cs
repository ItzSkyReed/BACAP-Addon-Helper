using Core.SNBT.Interfaces;

namespace Core.SNBT.Nodes;

/// <summary>
/// Represents an SNBT 32-bit integer array node formatted as [I; &lt;items&gt;].
/// </summary>
public sealed record SnbtIntArray : SnbtArrayNode
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SnbtIntArray"/> record.
    /// </summary>
    /// <param name="items">The list of integer nodes. Int, short, and byte tags are permitted.</param>
    public SnbtIntArray(List<ISnbtNode> items) : base('I', items, ValidateItem) { }

    private static void ValidateItem(ISnbtNode item, int index)
    {
        if (item is not (SnbtInt or SnbtShort or SnbtByte))
        {
            throw new ArgumentException(
                $"Int arrays can only contain int, short, or byte tags per SNBT specification. Element at index {index} is of type '{item.GetType().Name}'.",
                nameof(item));
        }
    }
}