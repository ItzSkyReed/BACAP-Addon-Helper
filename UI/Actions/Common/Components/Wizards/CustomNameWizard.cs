using Core.DataComponents.Components;
using Core.Items;
using Core.Registries;
using Core.TextComponents.Components;
using Spectre.Console;
using UI.Styling;

namespace UI.Actions.Common.Components.Wizards;

/// <summary>
/// Wizard for setting or clearing an item's custom display name.
/// </summary>
public class CustomNameWizard : IComponentWizard
{
    /// <inheritdoc/>
    public string ComponentId => CustomNameComponent.ComponentId;

    /// <inheritdoc/>
    public string DisplayTitle => "Custom Name (minecraft:custom_name)";

    /// <inheritdoc/>
    public void Execute(ItemStack stack, MinecraftData mcData)
    {
        TuiTheme.RenderHeader("Custom Name Configuration");

        var currentName = (stack.Components.Get<CustomNameComponent>()?.Value as PlainTextComponent)?.Text ?? string.Empty;

        var input = AnsiConsole.Prompt(
            new TextPrompt<string>("Enter custom name (or leave empty to clear):")
                .DefaultValue(currentName)
                .AllowEmpty()
        ).Trim();

        if (string.IsNullOrWhiteSpace(input))
        {
            stack.Components.Remove<CustomNameComponent>();
            TuiTheme.ShowSuccess("Custom name removed.");
        }
        else
        {
            stack.Components.Set(new CustomNameComponent(input));
            TuiTheme.ShowSuccess($"Custom name set to '{Markup.Escape(input)}'.");
        }

        TuiTheme.WaitForKey();
    }
}