namespace Core.Commands.Models;

/// <summary>
/// Represents a 2D or 3D coordinate in Minecraft (e.g., "~ ~ ~", "~ ~", "10 64 -10").
/// </summary>
public readonly record struct Position(string X, string? Y, string Z)
{
    /// <summary>
    /// Initializes a 2D horizontal position (X, Z).
    /// </summary>
    /// <param name="x">The X coordinate.</param>
    /// <param name="z">The Z coordinate.</param>
    public Position(string x, string z) : this(x, null, z) { }

    public override string ToString() => Y is null ? $"{X} {Z}" : $"{X} {Y} {Z}";
}