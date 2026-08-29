using Core.Commands.Models.Interfaces;

namespace Core.Commands.Models;

/// <summary>
/// A base implementation for Minecraft commands that automatically handles macro prefixing.
/// </summary>
/// <param name="IsMacro">Indicates if the command should be prefixed with '$'.</param>
public abstract record CommandBase(bool IsMacro = false) : ICommand
{
    public ICommand WithMacro(bool isMacro) => this with { IsMacro = isMacro };
    /// <summary>
    /// Builds the full command string, automatically prepending the macro symbol if required.
    /// </summary>
    /// <returns>The complete command string.</returns>
    public string Build()
    {
        var prefix = IsMacro ? "$" : string.Empty;
        return $"{prefix}{BuildInternal()}";
    }

    /// <summary>
    /// Generates the core command string without the macro prefix.
    /// </summary>
    /// <returns>The specific command syntax (e.g., "tellraw @a {}").</returns>
    protected abstract string BuildInternal();
}