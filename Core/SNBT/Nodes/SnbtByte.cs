using Core.SNBT.Interfaces;

namespace Core.SNBT.Nodes;

public record SnbtByte(sbyte Value) : ISnbtNode
{
    public string ToSnbtString(bool pretty = true, string indent = "") => $"{Value}b";
}