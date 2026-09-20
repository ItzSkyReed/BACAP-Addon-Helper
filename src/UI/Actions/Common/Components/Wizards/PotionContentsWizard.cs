using Core.DataComponents.Components;
using Core.Items;
using Core.Registries;
using Spectre.Console;
using UI.Styling;

namespace UI.Actions.Common.Components.Wizards;

/// <summary>
/// Wizard for configuring potion bottle and tipped arrow effects (<c>minecraft:potion_contents</c>).
/// </summary>
public class PotionContentsWizard : IComponentWizard
{
    /// <inheritdoc/>
    public string ComponentId => PotionContentsComponent.ComponentId;

    /// <inheritdoc/>
    public string DisplayTitle => "Potion Contents (minecraft:potion_contents)";

    /// <inheritdoc/>
    public void Execute(ItemStack stack, MinecraftData mcData)
    {
        TuiTheme.RenderHeader("Potion Contents Configuration");

        if (stack.Components.TryGet<PotionContentsComponent>(out var existing))
        {
            if (AnsiConsole.Confirm($"Current potion: [yellow]{existing.Potion}[/]. Remove it?", defaultValue: false))
            {
                stack.Components.Remove<PotionContentsComponent>();
                TuiTheme.ShowSuccess("Potion contents removed.");
                TuiTheme.WaitForKey();
                return;
            }
        }

        var potion = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Select a base potion preset (type to search):")
                .EnableSearch()
                .PageSize(15)
                .AddChoices(mcData.Potions.Keys)
        );

        var fullPotion = potion.Contains(':') ? potion : $"minecraft:{potion}";
        stack.Components.Set(new PotionContentsComponent(fullPotion));

        TuiTheme.ShowSuccess($"Potion preset set to {fullPotion}.");
        TuiTheme.WaitForKey();
    }
}