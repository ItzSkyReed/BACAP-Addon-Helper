using System.Globalization;
using Core.SNBT.Interfaces;

namespace Core.SNBT.Nodes;

public record SnbtFloat(float Value) : ISnbtNode
{
    public string ToSnbtString(bool pretty = true, string indent = "") =>
        Value.ToString(CultureInfo.InvariantCulture) + "f";
}