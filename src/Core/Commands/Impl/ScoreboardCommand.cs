using Core.Commands.Models;
using Core.TextComponents.Components;

namespace Core.Commands.Impl;

/// <summary>
/// Specifies the mathematical operation for modifying a player's score.
/// </summary>
public enum ScoreboardMathOperation
{
    Set,
    Add,
    Remove
}

/// <summary>
/// A base abstract record for all scoreboard commands.
/// </summary>
/// <param name="IsMacro">Indicates if the command should be prefixed with '$'.</param>
public abstract record ScoreboardCommand(bool IsMacro = false) : CommandBase(IsMacro);

// Objectives Commands

/// <summary>
/// Represents the '/scoreboard objectives list' command.
/// </summary>
public record ScoreboardObjectivesListCommand(bool IsMacro = false) : ScoreboardCommand(IsMacro)
{
    protected override string BuildInternal() => "scoreboard objectives list";
}

/// <summary>
/// Represents the '/scoreboard objectives add' command.
/// </summary>
public record ScoreboardObjectivesAddCommand(
    string Objective,
    string Criteria,
    TextComponent? DisplayName = null,
    bool IsMacro = false
) : ScoreboardCommand(IsMacro)
{
    protected override string BuildInternal() =>
        DisplayName is not null
            ? $"scoreboard objectives add {Objective} {Criteria} {DisplayName}"
            : $"scoreboard objectives add {Objective} {Criteria}";
}

/// <summary>
/// Represents the '/scoreboard objectives remove' command.
/// </summary>
public record ScoreboardObjectivesRemoveCommand(
    string Objective,
    bool IsMacro = false
) : ScoreboardCommand(IsMacro)
{
    protected override string BuildInternal() => $"scoreboard objectives remove {Objective}";
}

/// <summary>
/// Represents the '/scoreboard objectives setdisplay' command.
/// </summary>
public record ScoreboardObjectivesSetDisplayCommand(
    string Slot,
    string? Objective = null,
    bool IsMacro = false
) : ScoreboardCommand(IsMacro)
{
    protected override string BuildInternal() =>
        string.IsNullOrEmpty(Objective)
            ? $"scoreboard objectives setdisplay {Slot}"
            : $"scoreboard objectives setdisplay {Slot} {Objective}";
}

/// <summary>
/// Represents the '/scoreboard objectives modify' command.
/// </summary>
public record ScoreboardObjectivesModifyCommand(
    string Objective,
    string Property,
    string Value,
    bool IsMacro = false
) : ScoreboardCommand(IsMacro)
{
    protected override string BuildInternal() => $"scoreboard objectives modify {Objective} {Property} {Value}";
}

// Players Commands

/// <summary>
/// Represents the '/scoreboard players list' command.
/// </summary>
public record ScoreboardPlayersListCommand(
    Selector? Target = null,
    bool IsMacro = false
) : ScoreboardCommand(IsMacro)
{
    protected override string BuildInternal() =>
        Target is null
            ? "scoreboard players list"
            : $"scoreboard players list {Target}";
}

/// <summary>
/// Represents the '/scoreboard players get' command.
/// </summary>
public record ScoreboardPlayersGetCommand(
    Selector Target,
    string Objective,
    bool IsMacro = false
) : ScoreboardCommand(IsMacro)
{
    protected override string BuildInternal() => $"scoreboard players get {Target} {Objective}";
}

/// <summary>
/// Represents the '/scoreboard players set/add/remove' commands.
/// </summary>
public record ScoreboardPlayersMathCommand(
    ScoreboardMathOperation Operation,
    Selector Targets,
    string Objective,
    int Score,
    bool IsMacro = false
) : ScoreboardCommand(IsMacro)
{
    protected override string BuildInternal()
    {
        var opString = Operation switch
        {
            ScoreboardMathOperation.Set => "set",
            ScoreboardMathOperation.Add => "add",
            ScoreboardMathOperation.Remove => "remove",
            _ => Operation.ToString().ToLowerInvariant()
        };

        return $"scoreboard players {opString} {Targets} {Objective} {Score}";
    }
}

/// <summary>
/// Represents the '/scoreboard players reset' command.
/// </summary>
public record ScoreboardPlayersResetCommand(
    Selector Targets,
    string? Objective = null,
    bool IsMacro = false
) : ScoreboardCommand(IsMacro)
{
    protected override string BuildInternal() =>
        string.IsNullOrEmpty(Objective)
            ? $"scoreboard players reset {Targets}"
            : $"scoreboard players reset {Targets} {Objective}";
}

/// <summary>
/// Represents the '/scoreboard players enable' command.
/// </summary>
public record ScoreboardPlayersEnableCommand(
    Selector Targets,
    string Objective,
    bool IsMacro = false
) : ScoreboardCommand(IsMacro)
{
    protected override string BuildInternal() => $"scoreboard players enable {Targets} {Objective}";
}

/// <summary>
/// Represents the '/scoreboard players operation' command.
/// </summary>
public record ScoreboardPlayersOperationCommand(
    Selector Targets,
    string TargetObjective,
    string Operation,
    Selector Source,
    string SourceObjective,
    bool IsMacro = false
) : ScoreboardCommand(IsMacro)
{
    protected override string BuildInternal() =>
        $"scoreboard players operation {Targets} {TargetObjective} {Operation} {Source} {SourceObjective}";
}

/// <summary>
/// Represents the '/scoreboard players display' command.
/// </summary>
public record ScoreboardPlayersDisplayCommand(
    Selector Targets,
    string Objective,
    string DisplayType,
    string? Value = null,
    bool IsMacro = false
) : ScoreboardCommand(IsMacro)
{
    protected override string BuildInternal() =>
        string.IsNullOrEmpty(Value)
            ? $"scoreboard players display {DisplayType} {Targets} {Objective}"
            : $"scoreboard players display {DisplayType} {Targets} {Objective} {Value}";
}