using Core.SNBT.Interfaces;

namespace Core.SNBT.Nodes;

public record SnbtLong(long Value) : ISnbtNode
{
    public string ToSnbtString(bool pretty = false, string indent = "") => $"{Value}L";
}