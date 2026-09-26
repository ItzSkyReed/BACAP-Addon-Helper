using Core.SNBT.Interfaces;

namespace Core.SNBT.Nodes;

/// <summary>
/// Represents an SNBT byte array node formatted as [B; &lt;items&gt;].
/// </summary>
public sealed record SnbtByteArray : SnbtArrayNode
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SnbtByteArray"/> record.
    /// </summary>
    /// <param name="items">The list of byte nodes. Booleans and non-byte nodes are prohibited.</param>
    public SnbtByteArray(List<ISnbtNode> items) : base('B', items, ValidateItem) { }

    private static void ValidateItem(ISnbtNode item, int index)
    {
        if (item is not SnbtByte)
        {
            throw new ArgumentException(
                $"Byte arrays can only contain byte tags. Element at index {index} is of type '{item.GetType().Name}'. Booleans and other types are strictly prohibited.",
                nameof(item));
        }
    }
}