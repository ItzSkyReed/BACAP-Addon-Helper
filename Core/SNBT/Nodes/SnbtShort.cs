using Core.SNBT.Interfaces;

namespace Core.SNBT.Nodes;

public record SnbtShort(short Value) : ISnbtNode
{
    public string ToSnbtString(bool pretty = true, string indent = "") => $"{Value}s";
}