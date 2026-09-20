using Core.DataComponents.Components;
using Core.Items;
using Core.Registries;
using Spectre.Console;
using UI.Styling;

namespace UI.Actions.Common.Components.Wizards;

/// <summary>
/// Wizard for forcing or disabling the visual enchantment glint on an item.
/// </summary>
public class EnchantmentGlintWizard : IComponentWizard
{
    /// <inheritdoc/>
    public string ComponentId => EnchantmentGlintOverrideComponent.ComponentId;

    /// <inheritdoc/>
    public string DisplayTitle => "Enchantment Glint Override (minecraft:enchantment_glint_override)";

    /// <inheritdoc/>
    public void Execute(ItemStack stack, MinecraftData mcData)
    {
        TuiTheme.RenderHeader("Enchantment Glint Override");

        var choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Choose glint behavior:")
                .AddChoices("Force Glint (true)", "Hide Glint (false)", "[red]Remove Override[/]", "Cancel")
        );

        switch (choice)
        {
            case "Force Glint (true)":
                stack.Components.Set(new EnchantmentGlintOverrideComponent(true));
                TuiTheme.ShowSuccess("Glint override set to TRUE (always shiny).");
                break;

            case "Hide Glint (false)":
                stack.Components.Set(new EnchantmentGlintOverrideComponent(false));
                TuiTheme.ShowSuccess("Glint override set to FALSE (never shiny).");
                break;

            case "[red]Remove Override[/]":
                stack.Components.Remove<EnchantmentGlintOverrideComponent>();
                TuiTheme.ShowSuccess("Removed enchantment glint override.");
                break;
        }

        TuiTheme.WaitForKey();
    }
}