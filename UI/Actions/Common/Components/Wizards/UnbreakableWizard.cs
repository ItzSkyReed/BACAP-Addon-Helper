using Core.DataComponents.Components;
using Core.Items;
using Core.Registries;
using Spectre.Console;
using UI.Styling;

namespace UI.Actions.Common.Components.Wizards;

/// <summary>
/// Wizard for configuring durability immunity and tooltip visibility on an item (<c>minecraft:unbreakable</c>).
/// </summary>
public class UnbreakableWizard : IComponentWizard
{
    /// <inheritdoc/>
    public string ComponentId => UnbreakableComponent.ComponentId;

    /// <inheritdoc/>
    public string DisplayTitle => "Unbreakable (minecraft:unbreakable)";

    /// <inheritdoc/>
    public void Execute(ItemStack stack, MinecraftData mcData)
    {
        TuiTheme.RenderHeader("Unbreakable Configuration");

        if (stack.Components.TryGet<UnbreakableComponent>(out var existing))
        {
            var remove = AnsiConsole.Confirm("Item is currently Unbreakable. Do you want to remove this component?", defaultValue: false);
            if (remove)
            {
                stack.Components.Remove<UnbreakableComponent>();
                TuiTheme.ShowSuccess("Removed Unbreakable component.");
                TuiTheme.WaitForKey();
                return;
            }
        }

        var showTooltip = AnsiConsole.Confirm("Show 'Unbreakable' text in item tooltip?", defaultValue: existing?.ShowInTooltip ?? true);
        stack.Components.Set(new UnbreakableComponent(showTooltip));

        TuiTheme.ShowSuccess($"Item marked as Unbreakable (show_in_tooltip: {showTooltip}).");
        TuiTheme.WaitForKey();
    }
}