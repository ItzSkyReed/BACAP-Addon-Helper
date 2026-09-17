namespace BacapGenerator.McMeta;

/// <summary>
/// Represents a Minecraft pack version format (Major, Minor).
/// </summary>
public readonly record struct PackVersion(int Major, int Minor = 0);