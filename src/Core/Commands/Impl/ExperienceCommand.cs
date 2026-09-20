using System.Text.Json.Serialization;
using Core.Commands.Models;

namespace Core.Commands.Impl;

/// <summary>
/// Specifies the action to perform with the /experience command.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ExperienceAction
{
    Add,
    Set,
    Query
}

/// <summary>
/// Specifies the unit of experience (levels or points).
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ExperienceUnit
{
    Levels,
    Points
}

/// <summary>
/// Represents the /experience (or /xp) command, which manages player experience.
/// </summary>
public record ExperienceCommand(
    ExperienceAction Action,
    Selector Target,
    int? Amount = null,
    ExperienceUnit? Unit = null,
    bool IsMacro = false
) : CommandBase(IsMacro)
{
    protected override string BuildInternal()
    {
        var actionStr = Action.ToString().ToLowerInvariant();
        var unitStr = Unit?.ToString().ToLowerInvariant();

        // Query mode: experience query <targets> (levels|points)
        if (Action == ExperienceAction.Query)
            return $"xp {actionStr} {Target} {unitStr}";

        // Add / Set mode: experience add|set <targets> <amount> [levels|points]
        var baseCmd = $"xp {actionStr} {Target} {Amount}";

        return unitStr != null
            ? $"{baseCmd} {unitStr}"
            : baseCmd;
    }
}