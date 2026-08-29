using Core.Commands.Models;
namespace Core.Commands.Impl;

/// <summary>
/// Represents the /tellraw command.
/// </summary>
public record SayCommand(
    string Message,
    bool IsMacro = false
) : CommandBase(IsMacro)
{
    protected override string BuildInternal()
    {
        return $"say {Message}";
    }
}