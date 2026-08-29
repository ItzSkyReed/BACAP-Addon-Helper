using Core.Commands.Models;
using Core.Commands.Models.Interfaces;
using Core.SNBT.Nodes;

namespace Core.Commands.Impl;

/// <summary>
/// Represents the /summon command.
/// </summary>
public record SummonCommand(string EntityId, Position Position, SnbtCompound? Nbt = null, bool IsMacro = false) : CommandBase(IsMacro)
{
    protected override string BuildInternal()
    {
        var nbtStr = Nbt != null ? $" {Nbt.ToSnbtString(false)}" : "";
        return $"summon {EntityId} {Position}{nbtStr}";
    }
}