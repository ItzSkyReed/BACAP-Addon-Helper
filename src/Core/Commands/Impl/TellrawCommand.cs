using Core.Commands.Models;
using Core.TextComponents.Components;

namespace Core.Commands.Impl;

/// <summary>
/// Represents the /tellraw command.
/// </summary>
public record TellrawCommand(
    Selector Target,
    TextComponent Message,
    bool IsMacro = false
) : CommandBase(IsMacro)
{
    protected override string BuildInternal()
    {
        return $"tellraw {Target} {Message.ToSnbt().ToSnbtString()}";
    }
}