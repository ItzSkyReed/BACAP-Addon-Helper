using Core.McFunctions.Models.Interfaces;
using JetBrains.Annotations;

namespace Core.McFunctions.Models;

/// <summary>
/// Represents an empty line or a line containing only whitespace.
/// Implemented as a singleton to eliminate heap allocations when parsing or generating functions.
/// </summary>
public sealed record EmptyLine : IMcFunctionLine
{
    /// <summary>
    /// Gets the shared singleton instance of <see cref="EmptyLine"/>.
    /// </summary>
    [PublicAPI]
    public static EmptyLine Instance { get; } = new();

    private EmptyLine()
    {
    }

    /// <inheritdoc />
    public string Build() => string.Empty;
}