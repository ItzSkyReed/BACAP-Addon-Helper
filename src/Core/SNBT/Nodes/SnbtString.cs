using Core.SNBT.Interfaces;

namespace Core.SNBT.Nodes;

/// <summary>
/// Represents a string value node in an SNBT structure.
/// </summary>
public record SnbtString(string Value) : ISnbtNode
{
    public string ToSnbtString(bool pretty = false, string indent = "")
    {

        var escaped = Value
            .Replace("\\", @"\\")
            .Replace("\"", "\\\"")
            .Replace("\b", "\\b")
            .Replace("\f", "\\f")
            .Replace("\n", "\\n")
            .Replace("\r", "\\r")
            .Replace("\t", "\\t");

        return $"\"{escaped}\"";
    }
}