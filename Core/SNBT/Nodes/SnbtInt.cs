using Core.SNBT.Interfaces;

namespace Core.SNBT.Nodes;

public record SnbtInt(int Value) : ISnbtNode
{
    public string ToSnbtString(bool pretty = true, string indent = "") => $"{Value}";
}