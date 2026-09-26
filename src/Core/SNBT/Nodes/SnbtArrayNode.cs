using System.Runtime.InteropServices;
using System.Text;
using Core.SNBT.Interfaces;

namespace Core.SNBT.Nodes;

/// <summary>
/// Represents the abstract base record for SNBT array nodes ([B;...], [I;...], [L;...]).
/// </summary>
public abstract record SnbtArrayNode : ISnbtNode
{
    /// <summary>
    /// Gets the SNBT array prefix character ('B', 'I', or 'L').
    /// </summary>
    public char TypePrefix { get; }

    /// <summary>
    /// Gets the underlying list of nodes contained within the array.
    /// Compatible with <see cref="CollectionsMarshal.AsSpan{T}"/>.
    /// </summary>
    public List<ISnbtNode> Items { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SnbtArrayNode"/> record safely without invoking virtual members.
    /// </summary>
    /// <param name="typePrefix">The array type prefix character.</param>
    /// <param name="items">The collection of items.</param>
    /// <param name="itemValidator">A static validation callback invoked for each node during construction.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="items"/>, <paramref name="itemValidator"/>, or any element is null.</exception>
    protected SnbtArrayNode(char typePrefix, List<ISnbtNode> items, Action<ISnbtNode, int> itemValidator)
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(itemValidator);

        TypePrefix = typePrefix;

        var span = CollectionsMarshal.AsSpan(items);
        for (var i = 0; i < span.Length; i++)
        {
            var item = span[i];
            ArgumentNullException.ThrowIfNull(item, $"{nameof(items)}[{i}]");
            itemValidator(item, i);
        }

        Items = items;
    }

    /// <summary>
    /// Converts the array node into its SNBT string representation.
    /// </summary>
    /// <param name="pretty">Indicates whether formatting with spaces is enabled.</param>
    /// <param name="indent">The indentation prefix for nested structures.</param>
    /// <returns>A formatted SNBT array string such as [I;1,2] or [I; 1, 2].</returns>
    public string ToSnbtString(bool pretty = false, string indent = "")
    {
        var span = CollectionsMarshal.AsSpan(Items);
        if (span.Length == 0)
        {
            return $"[{TypePrefix};]";
        }

        var prefix = pretty ? $"[{TypePrefix}; " : $"[{TypePrefix};";
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
    /// Determines whether two SNBT arrays are of the exact same type and contain equivalent elements in identical sequence.
    /// </summary>
    /// <param name="other">The other array node to compare with.</param>
    /// <returns>True if both arrays have matching types and equal elements; otherwise, false.</returns>
    public virtual bool Equals(SnbtArrayNode? other)
    {
        if (other is null || EqualityContract != other.EqualityContract) return false;
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
    /// Calculates a hash code combining the record contract and each item.
    /// </summary>
    /// <returns>A computed integer hash code.</returns>
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(EqualityContract);

        var span = CollectionsMarshal.AsSpan(Items);
        foreach (var t in span)
            hash.Add(t);

        return hash.ToHashCode();
    }

    /// <summary>
    /// Deconstructs the array node into its underlying list of items.
    /// </summary>
    /// <param name="items">The encapsulated list of items.</param>
    public void Deconstruct(out List<ISnbtNode> items) => items = Items;
}