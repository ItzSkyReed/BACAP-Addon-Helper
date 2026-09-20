using Core.Items;
using Core.Registries;
using Spectre.Console;
using UI.Actions.Common.Components.Wizards;

namespace UI.Actions.Common.Components;

/// <summary>
/// Central registry hosting all configured component wizards.
/// </summary>
public static class ComponentWizardRegistry
{
    private static readonly List<IComponentWizard> Wizards =
    [
        new UnbreakableWizard(),
        new EnchantmentGlintWizard(),
        new CustomNameWizard(),
        new LoreWizard(),
        new EnchantmentsWizard(),
        new StoredEnchantmentsWizard(),
        new TrimWizard(),
        new ProfileWizard(),
        new PotionContentsWizard(),
        new BannerPatternsWizard()
    ];

    /// <summary>
    /// Opens the selection menu listing all available component wizards.
    /// </summary>
    /// <param name="stack">The item stack being edited.</param>
    /// <param name="mcData">The loaded Minecraft registries.</param>
    /// <returns>The updated <see cref="ItemStack"/> reference.</returns>
    public static ItemStack OpenMenu(ItemStack stack, MinecraftData mcData)
    {
        var prompt = new SelectionPrompt<object>()
            .Title("Select a component to configure:")
            .AddChoices(Wizards)
            .AddChoices(["<- Back"])
            .UseConverter(item => item switch
            {
                IComponentWizard wizard => stack.Components.Contains(wizard.ComponentId)
                    ? $"{wizard.DisplayTitle} [green](Configured)[/]"
                    : wizard.DisplayTitle,
                _ => item.ToString()!
            });

        var selection = AnsiConsole.Prompt(prompt);

        if (selection is IComponentWizard selectedWizard)
        {
            selectedWizard.Execute(stack, mcData);
        }

        return stack;
    }
}