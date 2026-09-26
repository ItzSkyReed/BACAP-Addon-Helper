using System.Text;
using Core.SNBT.Interfaces;

namespace Core.SNBT.Nodes;

/// <summary>
/// Represents an SNBT byte array node formatted as [B; &lt;items&gt;].
/// </summary>
public sealed record SnbtByteArray : ISnbtNode, IEquatable<SnbtByteArray>
{
    /// <summary>
    /// Gets the ordered collection of byte tags contained within the array.
    /// </summary>
    public IReadOnlyList<ISnbtNode> Items { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SnbtByteArray"/> record.
    /// </summary>
    /// <param name="items">The collection of byte nodes. Booleans and non-byte nodes are not allowed by the SNBT specification.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="items"/> or any of its elements is null.</exception>
    /// <exception cref="ArgumentException">Thrown when an item in <paramref name="items"/> is not a valid <see cref="SnbtByte"/>.</exception>
    public SnbtByteArray(IReadOnlyList<ISnbtNode> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        for (var i = 0; i < items.Count; i++)
        {
            var item = items[i];
            ArgumentNullException.ThrowIfNull(item, $"{nameof(items)}[{i}]");

            if (item is not SnbtByte)
            {
                throw new ArgumentException(
                    $"Byte arrays can only contain byte tags. Element at index {i} is of type '{item.GetType().Name}'. Booleans and other types are strictly prohibited.",
                    nameof(items));
            }
        }

        Items = items;
    }

    /// <summary>
    /// Converts the byte array node into its SNBT string representation.
    /// </summary>
    /// <param name="pretty">Indicates whether formatting with spaces is enabled.</param>
    /// <param name="indent">The indentation prefix for nested structures.</param>
    /// <returns>A string representation in the format [B;1b,2b] or [B; 1b, 2b] when pretty is true.</returns>
    /// <example>
    /// <code>
    /// var array = new SnbtByteArray([new SnbtByte(1), new SnbtByte(2)]);
    /// string snbt = array.ToSnbtString(); // returns "[B;1b,2b]"
    /// </code>
    /// </example>
    public string ToSnbtString(bool pretty = false, string indent = "")
    {
        if (Items.Count == 0)
        {
            return "[B;]";
        }

        var prefix = pretty ? "[B; " : "[B;";
        var separator = pretty ? ", " : ",";

        var builder = new StringBuilder(prefix);
        for (var i = 0; i < Items.Count; i++)
        {
            if (i > 0)
            {
                builder.Append(separator);
            }

            builder.Append(Items[i].ToSnbtString(pretty, indent));
        }

        builder.Append(']');
        return builder.ToString();
    }

    /// <summary>
    /// Determines whether the specified <see cref="SnbtByteArray"/> contains identical elements in the same sequence.
    /// </summary>
    /// <param name="other">The other byte array to compare with.</param>
    /// <returns>True if the arrays have equal elements in the same order; otherwise, false.</returns>
    public bool Equals(SnbtByteArray? other)
    {
        if (other is null) return false;
        return ReferenceEquals(this, other) || Items.SequenceEqual(other.Items);
    }

    /// <summary>
    /// Returns the hash code computed from the elements of this byte array.
    /// </summary>
    /// <returns>A hash code for the current node.</returns>
    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var item in Items)
        {
            hash.Add(item);
        }

        return hash.ToHashCode();
    }
}