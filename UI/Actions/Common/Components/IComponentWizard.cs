using Core.Items;
using Core.Registries;

namespace UI.Actions.Common.Components;

/// <summary>
/// Defines a contract for interactive wizards that guide the user through configuring a specific Minecraft item data component.
/// </summary>
public interface IComponentWizard
{
    /// <summary>
    /// Gets the resource location identifier of the component handled by this wizard (e.g. <c>minecraft:unbreakable</c>).
    /// </summary>
    string ComponentId { get; }

    /// <summary>
    /// Gets the formatted title displayed in the interactive selection menu.
    /// </summary>
    string DisplayTitle { get; }

    /// <summary>
    /// Executes the wizard prompts to inspect, update, or remove the target component from the item stack.
    /// </summary>
    /// <param name="stack">The item stack whose components are being modified.</param>
    /// <param name="mcData">The centralized Minecraft static registry container.</param>
    void Execute(ItemStack stack, MinecraftData mcData);
}