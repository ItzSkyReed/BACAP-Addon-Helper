using Core.McFunctions.Models.Interfaces;

namespace Core.McFunctions.Models;

/// <summary>
/// A fallback for commands that we don't have a strongly-typed model for yet.
/// </summary>
public record RawCommandLine(string RawText, bool IsMacro = false) : IMcFunctionLine
{
    public string Build() => IsMacro ? $"${RawText}" : RawText;
}