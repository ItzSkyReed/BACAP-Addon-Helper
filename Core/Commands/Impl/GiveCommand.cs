using Core.Commands.Models;
using Core.Items;

namespace Core.Commands.Impl;

/// <summary>
/// Represents the /give command.
/// </summary>
public record GiveCommand(Selector Target, ItemStack Item, int? Count = null, bool IsMacro = false) : CommandBase(IsMacro)
{
    protected override string BuildInternal()
    {
        // If the count is passed explicitly, we use it.
        // Otherwise, we take it from the stack itself (the default is 1).
        var amount = Count ?? Item.Count;
        var countStr = amount != 1 ? $" {amount}" : "";

        return $"give {Target} {Item.ToCommandString()}{countStr}";
    }
}