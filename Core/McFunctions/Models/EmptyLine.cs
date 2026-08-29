using Core.McFunctions.Models.Interfaces;

namespace Core.McFunctions.Models;

/// <summary>
/// Represents an empty line or a line containing only whitespace.
/// Useful for preserving formatting when round-tripping files.
/// </summary>
public record EmptyLine : IMcFunctionLine
{
    public string Build() => string.Empty;
}