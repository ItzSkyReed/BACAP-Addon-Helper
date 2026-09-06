using Core.DataComponents.Components;
using Core.Items;
using Core.Registries;
using Spectre.Console;
using UI.Styling;

namespace UI.Actions.Common.Components.Wizards;

/// <summary>
/// Wizard for configuring armor trim patterns and materials validated against registries (<c>minecraft:trim</c>).
/// </summary>
public class TrimWizard : IComponentWizard
{
    /// <inheritdoc/>
    public string ComponentId => TrimComponent.ComponentId;

    /// <inheritdoc/>
    public string DisplayTitle => "Armor Trim (minecraft:trim)";

    /// <inheritdoc/>
    public void Execute(ItemStack stack, MinecraftData mcData)
    {
        TuiTheme.RenderHeader("Armor Trim Configuration");

        if (stack.Components.TryGet<TrimComponent>(out var existing))
        {
            if (AnsiConsole.Confirm($"Current trim: [yellow]{existing.PatternId}[/] ([cyan]{existing.MaterialId}[/]). Remove it?", defaultValue: false))
            {
                stack.Components.Remove<TrimComponent>();
                TuiTheme.ShowSuccess("Trim removed.");
                TuiTheme.WaitForKey();
                return;
            }
        }

        var pattern = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Select a trim pattern (type to search):")
                .EnableSearch()
                .PageSize(15)
                .AddChoices(mcData.Trims)
        );

        var material = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Select a trim material (type to search):")
                .EnableSearch()
                .PageSize(15)
                .AddChoices(mcData.TrimMaterialColors.Keys)
        );

        var showTooltip = AnsiConsole.Confirm("Show trim in tooltip?", defaultValue: existing?.ShowInTooltip ?? true);

        var fullPattern = pattern.Contains(':') ? pattern : $"minecraft:{pattern}";
        var fullMaterial = material.Contains(':') ? material : $"minecraft:{material}";

        stack.Components.Set(new TrimComponent(fullPattern, fullMaterial, showTooltip));
        TuiTheme.ShowSuccess($"Trim applied: {pattern} with {material}.");
        TuiTheme.WaitForKey();
    }
}