namespace Core.McFunctions.Models.Interfaces;

/// <summary>
/// Represents a single logical line within an .mcfunction file.
/// </summary>
public interface IMcFunctionLine
{
    /// <summary>
    /// Serializes the line back into a string format valid for an .mcfunction file.
    /// </summary>
    string Build();
}