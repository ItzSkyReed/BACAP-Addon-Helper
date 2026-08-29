using Core.SNBT.Interfaces;

namespace Core.SNBT.Nodes;

public record SnbtByteArray(List<ISnbtNode> Items) : ISnbtNode
{
    public string ToSnbtString(bool pretty = true, string indent = "") =>
        $"[B; {string.Join(pretty ? ", " : ",", Items.Select(i => i.ToSnbtString(pretty, indent)))}]";
}