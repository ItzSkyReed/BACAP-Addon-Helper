using System.Globalization;
using Core.SNBT.Interfaces;

namespace Core.SNBT.Nodes;

public record SnbtDouble(double Value) : ISnbtNode
{
    public string ToSnbtString(bool pretty = true, string indent = "") =>
        Value.ToString(CultureInfo.InvariantCulture) + "d";
}