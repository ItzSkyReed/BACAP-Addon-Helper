using Core.Commands.Models;
using Core.Commands.Models.Interfaces;

namespace Core.Commands.Impl;

/// <summary>
/// Represents the /execute command, storing modifiers as a raw string
/// and evaluating the final subcommand via the command parser.
/// </summary>
public record ExecuteCommand(
    string RawModifiers,
    ICommand? RunCommand,
    bool IsMacro = false
) : CommandBase(IsMacro)
{
    protected override string BuildInternal()
    {
        var modifiers = string.IsNullOrWhiteSpace(RawModifiers) ? string.Empty : $"{RawModifiers} ";
        var runPart = RunCommand != null ? $"run {RunCommand.Build()}" : string.Empty;

        return $"execute {modifiers}{runPart}".Trim();
    }
}