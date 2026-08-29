using Core.SNBT.Interfaces;

namespace Core.SNBT.Nodes;

public record SnbtLongArray(List<ISnbtNode> Items) : ISnbtNode
{
    public string ToSnbtString(bool pretty = true, string indent = "") =>
        $"[L; {string.Join(pretty ? ", " : ",", Items.Select(i => i.ToSnbtString(pretty, indent)))}]";
}