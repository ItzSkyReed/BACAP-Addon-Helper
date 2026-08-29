using Core.DataComponents.Models.Interfaces;
using Core.SNBT;

using Core.SNBT.Interfaces;
using Core.SNBT.Nodes;
using JetBrains.Annotations;

namespace Core.DataComponents.Models;

/// <summary>
/// Represents a single attribute modifier applied to an entity while holding or wearing an item.
/// </summary>
/// <param name="Type">The resource location of the attribute to modify (e.g. <c>minecraft:generic.attack_damage</c>).</param>
/// <param name="Id">The unique resource location identifying this modifier (e.g. <c>minecraft:base_attack_damage</c>).</param>
/// <param name="Amount">The modifier numeric magnitude value.</param>
/// <param name="Operation">The arithmetic operation: <c>add_value</c>, <c>add_multiplied_base</c>, or <c>add_multiplied_total</c>.</param>
/// <param name="Slot">The equipment slot group required to activate the modifier (e.g. <c>any</c>, <c>mainhand</c>, <c>armor</c>). Defaults to <c>any</c>.</param>
/// <param name="Display">Optional tooltip display formatting rules for this modifier.</param>
public record AttributeModifier(
    string Type,
    string Id,
    double Amount,
    string Operation,
    string Slot = AttributeModifier.AnySlot,
    AttributeModifierDisplay? Display = null
) : ICompoundModel<AttributeModifier>
{
    [PublicAPI] public const string AnySlot = "any";
    [PublicAPI] public const string MainHandSlot = "mainhand";
    [PublicAPI] public const string OffHandSlot = "offhand";
    [PublicAPI] public const string HandSlot = "hand";
    [PublicAPI] public const string FeetSlot = "feet";
    [PublicAPI] public const string LegsSlot = "legs";
    [PublicAPI] public const string ChestSlot = "chest";
    [PublicAPI] public const string HeadSlot = "head";
    [PublicAPI] public const string ArmorSlot = "armor";
    [PublicAPI] public const string BodySlot = "body";
    [PublicAPI] public const string OperationAddValue = "add_value";
    [PublicAPI] public const string OperationAddMultipliedBase = "add_multiplied_base";
    [PublicAPI] public const string OperationAddMultipliedTotal = "add_multiplied_total";

    /// <summary>
    /// Parses an <see cref="AttributeModifier"/> from an SNBT compound node.
    /// </summary>
    /// <param name="compound">The SNBT compound node containing attribute modifier fields.</param>
    /// <returns>A populated <see cref="AttributeModifier"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown when required fields (<c>type</c>, <c>id</c>, <c>amount</c>, or <c>operation</c>) are missing.</exception>
    public static AttributeModifier Parse(SnbtCompound compound)
    {
        var displayNode = compound.GetNode("display") as SnbtCompound;

        return new AttributeModifier(
            Type: compound.GetString("type"),
            Id: compound.GetString("id"),
            Amount: compound.GetDouble("amount"),
            Operation: compound.GetString("operation"),
            Slot: compound.GetString("slot", AnySlot),
            Display: displayNode != null ? AttributeModifierDisplay.Parse(displayNode) : null
        );
    }

    /// <summary>
    /// Serializes the attribute modifier into an SNBT compound node.
    /// </summary>
    /// <returns>An <see cref="ISnbtNode"/> representing the attribute modifier compound.</returns>
    public ISnbtNode ToSnbt()
    {
        var builder = Snbt.Compound()
            .Put("type", Type)
            .Put("id", Id)
            .Put("amount", Amount)
            .Put("operation", Operation)
            .PutOptional("slot", Slot, AnySlot);

        if (Display != null)
        {
            builder.Put("display", Display.ToSnbt());
        }

        return builder.Build();
    }
}