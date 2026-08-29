using Core.DataComponents.Interfaces;
using Core.DataComponents.Models;
using Core.SNBT;

using Core.SNBT.Interfaces;
using Core.SNBT.Nodes;
using Core.TextComponents;
using JetBrains.Annotations;

namespace Core.DataComponents.Components;

/// <summary>
/// Configures attribute modifiers applied to a player or mob while holding or equipping this item (<c>minecraft:attribute_modifiers</c>).
/// </summary>
/// <param name="Modifiers">The list of attribute modifiers attached to this item.</param>
[UsedImplicitly]
public record AttributeModifiersComponent(
    List<AttributeModifier> Modifiers
) : IListComponent<AttributeModifiersComponent>
{
    /// <inheritdoc/>
    public static string ComponentId => "minecraft:attribute_modifiers";

    /// <summary>
    /// Initializes a new instance of the <see cref="AttributeModifiersComponent"/> record with an array of modifiers.
    /// </summary>
    /// <param name="modifiers">The attribute modifiers to attach.</param>
    public AttributeModifiersComponent(params AttributeModifier[] modifiers)
        : this(new List<AttributeModifier>(modifiers))
    {
    }

    /// <summary>
    /// Parses an <see cref="AttributeModifiersComponent"/> from an SNBT node representation.
    /// </summary>
    /// <param name="list">The SNBT list to parse, which must be an <see cref="SnbtList"/>.</param>
    /// <returns>A populated <see cref="AttributeModifiersComponent"/> instance.</returns>
    /// <example>
    /// <code>
    /// var node = SnbtParser.Parse("[{type: 'generic.attack_damage', id: 'base_attack', amount: 5.0d, operation: 'add_value', slot: 'mainhand'}]");
    /// var component = AttributeModifiersComponent.Parse(node);
    /// </code>
    /// </example>
    public static AttributeModifiersComponent Parse(SnbtList list)
    {

        var modifiers = new List<AttributeModifier>(list.Items.Count);

        foreach (var item in list.Items)
        {
            if (item is not SnbtCompound compound)
                continue;

            AttributeModifierDisplay? display = null;
            if (compound.GetNode("display") is SnbtCompound displayComp)
            {
                var displayType = displayComp.GetString("type", "default");

                // Extracts the raw NBT node and deserializes it via the chat text component parser
                var displayValueNode = displayComp.GetNode("value");
                var displayValue = displayValueNode != null
                    ? TextComponentParser.Parse(displayValueNode)
                    : null;

                display = new AttributeModifierDisplay(displayType, displayValue);
            }

            modifiers.Add(new AttributeModifier(
                Type: compound.GetString("type"),
                Id: compound.GetString("id"),
                Amount: compound.GetDouble("amount"),
                Operation: compound.GetString("operation"),
                Slot: compound.GetString("slot", "any"),
                Display: display
            ));
        }

        return new AttributeModifiersComponent(modifiers);
    }

    /// <summary>
    /// Serializes the attribute modifiers into an SNBT list node.
    /// </summary>
    /// <returns>An <see cref="ISnbtNode"/> representing the list of attribute modifier compounds.</returns>
    public ISnbtNode ToSnbt()
    {
        var listBuilder = Snbt.List();

        foreach (var mod in Modifiers)
        {
            listBuilder.AddCompound(comp =>
            {
                comp.Put("type", mod.Type);
                comp.Put("id", mod.Id);
                comp.Put("amount", mod.Amount);
                comp.Put("operation", mod.Operation);

                // Optional slot parameter; defaults to "any" in game logic
                comp.PutOptional("slot", mod.Slot, "any");

                // Optional tooltip display block
                if (mod.Display != null)
                {
                    comp.PutCompound("display", displayComp =>
                    {
                        displayComp.Put("type", mod.Display.Type);

                        // The 'value' tag is only written when type equals 'override' and a custom text component exists
                        if (mod.Display.Type == "override" && mod.Display.Value != null)
                        {
                            displayComp.Put("value", mod.Display.Value.ToSnbt());
                        }
                    });
                }
            });
        }

        return listBuilder.Build();
    }
}