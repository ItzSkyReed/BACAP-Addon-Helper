using System.Text.Json.Serialization;
using Core.Commands.Models;

namespace Core.Commands.Impl;

/// <summary>
/// Specifies whether to add (Grant) or remove (Revoke) an advancement.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AdvancementAction
{

    Grant,
    Revoke
}

/// <summary>
/// Specifies the scope of the advancement modification.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AdvancementMode
{
    Everything,
    Only,
    From,
    Through,
    Until
}

/// <summary>
/// Represents the /advancement command, which gives or takes an advancement from players.
/// </summary>
public record AdvancementCommand(
    AdvancementAction Action,
    Selector Target,
    AdvancementMode Mode,
    string? Advancement = null,
    string? Criterion = null,
    bool IsMacro = false
) : CommandBase(IsMacro)
{
    protected override string BuildInternal()
    {
        var actionStr = Action.ToString().ToLowerInvariant();
        var modeStr = Mode.ToString().ToLowerInvariant();

        // "everything" mode takes no further arguments
        if (Mode == AdvancementMode.Everything)
            return $"advancement {actionStr} {Target} everything";

        // Include criterion if provided (only valid for "only" mode)
        var critStr = !string.IsNullOrWhiteSpace(Criterion)
            ? $" {Criterion}"
            : string.Empty;

        return $"advancement {actionStr} {Target} {modeStr} {Advancement}{critStr}";
    }
}