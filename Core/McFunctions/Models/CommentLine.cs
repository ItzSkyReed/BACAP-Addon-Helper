using Core.McFunctions.Models.Interfaces;

namespace Core.McFunctions.Models;

/// <summary>
/// Represents a comment line starting with '#'.
/// </summary>
/// <param name="Text">The comment text, excluding the leading '#'.</param>
public record CommentLine(string Text) : IMcFunctionLine
{
    public string Build() => $"# {Text.TrimStart()}";
}