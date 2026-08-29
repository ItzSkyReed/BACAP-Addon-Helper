using System.Text;
using Core.SNBT.Interfaces;

namespace Core.SNBT.Nodes;

/// <summary>
/// Represents an SNBT/NBT list node containing indexed sequential nodes.
/// </summary>
public record SnbtList(List<ISnbtNode> Items) : ISnbtNode
{
    /// <summary>
    /// Initializes an empty <see cref="SnbtList"/>.
    /// </summary>
    public SnbtList() : this([])
    {
    }

    /// <inheritdoc/>
    public string ToSnbtString(bool pretty = true, string indent = "")
    {
        if (Items.Count == 0) return "[]";

        if (!pretty)
            return "[" + string.Join(",", Items.Select(i => i.ToSnbtString(false))) + "]";

        var sb = new StringBuilder();
        sb.AppendLine("[");
        var nextIndent = indent + "  ";

        for (var i = 0; i < Items.Count; i++)
        {
            sb.Append($"{nextIndent}{Items[i].ToSnbtString(true, nextIndent)}");
            if (i < Items.Count - 1) sb.AppendLine(","); else sb.AppendLine();
        }

        sb.Append(indent + "]");
        return sb.ToString();
    }
}