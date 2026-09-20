using System.Globalization;
using Core.SNBT.Interfaces;

namespace Core.SNBT.Nodes;

public record SnbtFloat(float Value) : ISnbtNode
{
    public string ToSnbtString(bool pretty = false, string indent = "") =>
        Value.ToString(CultureInfo.InvariantCulture) + "f";
}