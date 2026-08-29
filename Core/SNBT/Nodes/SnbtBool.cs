using Core.SNBT.Interfaces;

namespace Core.SNBT.Nodes;

public record SnbtBool(bool Value) : ISnbtNode
{
    public string ToSnbtString(bool pretty = true, string indent = "") => $"{Value.ToString().ToLower()}";
}