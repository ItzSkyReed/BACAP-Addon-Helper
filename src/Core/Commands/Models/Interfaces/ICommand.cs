namespace Core.Commands.Models.Interfaces;

/// <summary>
/// Represents a generic executable Minecraft command.
/// </summary>
public interface ICommand
{
    /// <summary>
    /// Indicates whether this command is a macro (prefixed with '$' in a mcfunction).
    /// </summary>
    bool IsMacro { get; }

    /// <summary>
    /// Serializes the command object back into a valid Minecraft command string.
    /// </summary>
    string Build();

    /// <summary>
    /// Creates a copy of the command with the specified macro state.
    /// </summary>
    ICommand WithMacro(bool isMacro);
}