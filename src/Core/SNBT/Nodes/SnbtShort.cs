using Core.SNBT.Interfaces;

namespace Core.SNBT.Nodes;

public record SnbtShort(short Value) : ISnbtNode
{
    public string ToSnbtString(bool pretty = false, string indent = "") => $"{Value}s";
}