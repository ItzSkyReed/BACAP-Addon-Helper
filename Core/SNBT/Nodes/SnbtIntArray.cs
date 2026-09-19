using Core.SNBT.Interfaces;

namespace Core.SNBT.Nodes;

public record SnbtIntArray(List<ISnbtNode> Items) : ISnbtNode
{
    public string ToSnbtString(bool pretty = false, string indent = "") =>
        $"[I; {string.Join(pretty ? ", " : ",", Items.Select(i => i.ToSnbtString(pretty, indent)))}]";
}