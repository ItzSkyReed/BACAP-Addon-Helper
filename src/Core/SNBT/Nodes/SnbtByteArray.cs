using System.Runtime.InteropServices;
using System.Text;
using Core.SNBT.Interfaces;

namespace Core.SNBT.Nodes;

/// <summary>
/// Represents an SNBT byte array node formatted as [B; &lt;items&gt;].
/// </summary>
public sealed record SnbtByteArray : ISnbtNode
{
    /// <summary>
    /// Gets the list of byte tags contained within the array.
    /// Compatible with <see cref="CollectionsMarshal.AsSpan{T}"/>.
    /// </summary>
    public List<ISnbtNode> Items { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SnbtByteArray"/> record.
    /// </summary>
    /// <param name="items">The list of byte nodes. Booleans and non-byte nodes are not allowed by the SNBT specification.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="items"/> or any of its elements is null.</exception>
    /// <exception cref="ArgumentException">Thrown when an item in <paramref name="items"/> is not a valid <see cref="SnbtByte"/>.</exception>
    public SnbtByteArray(List<ISnbtNode> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        var span = CollectionsMarshal.AsSpan(items);
        for (var i = 0; i < span.Length; i++)
        {
            var item = span[i];
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
    /// <returns>A string formatted as [B;1b,2b] or [B; 1b, 2b] when pretty is true.</returns>
    /// <example>
    /// <code>
    /// var array = new SnbtByteArray([new SnbtByte(1), new SnbtByte(2)]);
    /// string snbt = array.ToSnbtString(); // returns "[B;1b,2b]"
    /// </code>
    /// </example>
    public string ToSnbtString(bool pretty = false, string indent = "")
    {
        var span = CollectionsMarshal.AsSpan(Items);
        if (span.Length == 0)
        {
            return "[B;]";
        }

        var prefix = pretty ? "[B; " : "[B;";
        var separator = pretty ? ", " : ",";

        var builder = new StringBuilder(prefix);
        for (var i = 0; i < span.Length; i++)
        {
            if (i > 0)
                builder.Append(separator);

            builder.Append(span[i].ToSnbtString(pretty, indent));
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
        if (ReferenceEquals(this, other)) return true;

        var thisSpan = CollectionsMarshal.AsSpan(Items);
        var otherSpan = CollectionsMarshal.AsSpan(other.Items);

        if (thisSpan.Length != otherSpan.Length)
            return false;

        for (var i = 0; i < thisSpan.Length; i++)
        {
            if (!EqualityComparer<ISnbtNode>.Default.Equals(thisSpan[i], otherSpan[i]))
                return false;
        }

        return true;
    }

    /// <summary>
    /// Returns the hash code computed from the elements of this byte array.
    /// </summary>
    /// <returns>A hash code for the current node.</returns>
    public override int GetHashCode()
    {
        var hash = new HashCode();
        var span = CollectionsMarshal.AsSpan(Items);
        foreach (var t in span)
            hash.Add(t);

        return hash.ToHashCode();
    }
}