using Core.SNBT.Interfaces;

namespace Core.SNBT.Nodes;

public record SnbtInt(int Value) : ISnbtNode
{
    public string ToSnbtString(bool pretty = false, string indent = "") => $"{Value}";
}