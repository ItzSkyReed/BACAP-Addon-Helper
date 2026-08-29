using Core.SNBT.Interfaces;

namespace Core.SNBT.Nodes;

public record SnbtString(string Value) : ISnbtNode
{
    public string ToSnbtString(bool pretty = true, string indent = "")
    {
        var escaped = Value.Replace("\"", "\\\"");
        return $"\"{escaped}\"";
    }
}