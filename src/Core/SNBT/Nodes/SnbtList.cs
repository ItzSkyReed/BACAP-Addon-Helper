using System.Runtime.InteropServices;
using System.Text;
using Core.SNBT.Interfaces;

namespace Core.SNBT.Nodes;

/// <summary>
/// Represents an SNBT/NBT list node containing ordered sequential tags enclosed in square brackets.
/// </summary>
public sealed record SnbtList : ISnbtNode
{
    /// <summary>
    /// Gets the ordered list of child SNBT nodes.
    /// Compatible with <see cref="CollectionsMarshal.AsSpan{T}"/>.
    /// </summary>
    public List<ISnbtNode> Items { get; init; }

    /// <summary>
    /// Initializes an empty <see cref="SnbtList"/> instance.
    /// </summary>
    public SnbtList() : this([])
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SnbtList"/> record.
    /// </summary>
    /// <param name="items">The list of child nodes.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="items"/> or any of its elements is null.</exception>
    public SnbtList(List<ISnbtNode> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        var span = CollectionsMarshal.AsSpan(items);
        for (var i = 0; i < span.Length; i++)
        {
            ArgumentNullException.ThrowIfNull(span[i], $"{nameof(items)}[{i}]");
        }

        Items = items;
    }

    /// <summary>
    /// Converts the list node into its SNBT string representation.
    /// </summary>
    /// <param name="pretty">Indicates whether multi-line formatting with indentation is enabled.</param>
    /// <param name="indent">The current indentation prefix string.</param>
    /// <returns>A string representing the SNBT list syntax.</returns>
    /// <example>
    /// <code>
    /// var list = new SnbtList([new SnbtInt(1), new SnbtInt(2)]);
    /// string compact = list.ToSnbtString(); // "[1,2]"
    /// string pretty = list.ToSnbtString(pretty: true);
    /// // [
    /// //   1,
    /// //   2
    /// // ]
    /// </code>
    /// </example>
    public string ToSnbtString(bool pretty = false, string indent = "")
    {
        var span = CollectionsMarshal.AsSpan(Items);
        if (span.Length == 0)
            return "[]";

        if (!pretty)
        {
            var sb = new StringBuilder("[");
            for (var i = 0; i < span.Length; i++)
            {
                if (i > 0)
                    sb.Append(',');

                sb.Append(span[i].ToSnbtString(pretty: false, indent: string.Empty));
            }

            sb.Append(']');
            return sb.ToString();
        }

        var prettyBuilder = new StringBuilder();
        prettyBuilder.AppendLine("[");
        var nextIndent = indent + "  ";

        for (var i = 0; i < span.Length; i++)
        {
            prettyBuilder.Append(nextIndent);
            prettyBuilder.Append(span[i].ToSnbtString(pretty: true, indent: nextIndent));

            if (i < span.Length - 1)
                prettyBuilder.AppendLine(",");
            else
                prettyBuilder.AppendLine();
        }

        prettyBuilder.Append(indent);
        prettyBuilder.Append(']');
        return prettyBuilder.ToString();
    }

    /// <summary>
    /// Determines whether the specified <see cref="SnbtList"/> contains identical elements in the same sequence.
    /// </summary>
    /// <param name="other">The other list to compare with.</param>
    /// <returns>True if the lists have equal elements in the same sequence; otherwise, false.</returns>
    public bool Equals(SnbtList? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        var thisSpan = CollectionsMarshal.AsSpan(Items);
        var otherSpan = CollectionsMarshal.AsSpan(other.Items);

        if (thisSpan.Length != otherSpan.Length)
        {
            return false;
        }

        for (var i = 0; i < thisSpan.Length; i++)
        {
            if (!EqualityComparer<ISnbtNode>.Default.Equals(thisSpan[i], otherSpan[i]))
                return false;
        }

        return true;
    }

    /// <summary>
    /// Returns the hash code computed from the elements of this list.
    /// </summary>
    /// <returns>A hash code for the current node.</returns>
    public override int GetHashCode()
    {
        var hash = new HashCode();
        var span = CollectionsMarshal.AsSpan(Items);
        foreach (var t in span)
        {
            hash.Add(t);
        }

        return hash.ToHashCode();
    }

    /// <summary>
    /// Deconstructs the list node into its underlying list of items.
    /// </summary>
    /// <param name="items">The encapsulated list of items.</param>
    public void Deconstruct(out List<ISnbtNode> items) => items = Items;
}