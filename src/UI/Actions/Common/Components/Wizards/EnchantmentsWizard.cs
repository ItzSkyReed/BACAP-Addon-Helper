using Core.DataComponents.Components;
using Core.Items;
using Core.Registries;
using Spectre.Console;
using UI.Styling;

namespace UI.Actions.Common.Components.Wizards;

/// <summary>
/// Wizard for configuring active gameplay enchantments with registry validation (<c>minecraft:enchantments</c>).
/// </summary>
public class EnchantmentsWizard : IComponentWizard
{
    /// <inheritdoc/>
    public string ComponentId => EnchantmentsComponent.ComponentId;

    /// <inheritdoc/>
    public string DisplayTitle => "Enchantments (minecraft:enchantments)";

    /// <inheritdoc/>
    public void Execute(ItemStack stack, MinecraftData mcData)
    {
        var existing = stack.Components.Get<EnchantmentsComponent>();
        var levels = existing != null ? new Dictionary<string, int>(existing.Levels) : [];

        while (true)
        {
            TuiTheme.RenderHeader("Enchantments Configuration");

            if (levels.Count == 0)
                AnsiConsole.MarkupLine("[grey]No enchantments added.[/]\n");
            else
            {
                foreach (var (enchId, lvl) in levels)
                    AnsiConsole.MarkupLine($"[cyan]{Markup.Escape(enchId)}[/]: [yellow]{lvl}[/]");
                AnsiConsole.WriteLine();
            }

            var action = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Choose an action:")
                    .AddChoices("+ Add/Edit Enchantment", "- Remove Enchantment", "[red]Clear All[/]", "Done (Save)")
            );

            switch (action)
            {
                case "+ Add/Edit Enchantment":
                    var selectedEnch = AnsiConsole.Prompt(
                        new SelectionPrompt<string>()
                            .Title("Select an enchantment (type to search):")
                            .EnableSearch()
                            .PageSize(15)
                            .AddChoices(mcData.Enchantments.Keys)
                    );

                    var maxLvl = mcData.Enchantments.GetValueOrDefault(selectedEnch, 1);
                    var currentLvl = levels.GetValueOrDefault(selectedEnch, 1);

                    var level = AnsiConsole.Prompt(
                        new TextPrompt<int>($"Enter level for [yellow]{selectedEnch}[/] (standard max is {maxLvl}):")
                            .DefaultValue(currentLvl)
                            .Validate(v => v > 0 ? ValidationResult.Success() : ValidationResult.Error("Level must be >= 1"))
                    );

                    var fullEnchId = selectedEnch.Contains(':') ? selectedEnch : $"minecraft:{selectedEnch}";
                    levels[fullEnchId] = level;
                    break;

                case "- Remove Enchantment":
                    if (levels.Count == 0) break;
                    var toRemove = AnsiConsole.Prompt(new SelectionPrompt<string>().Title("Select to remove:").AddChoices(levels.Keys));
                    levels.Remove(toRemove);
                    break;

                case "[red]Clear All[/]":
                    levels.Clear();
                    break;

                case "Done (Save)":
                    if (levels.Count == 0)
                    {
                        stack.Components.Remove<EnchantmentsComponent>();
                        TuiTheme.ShowSuccess("All enchantments removed.");
                    }
                    else
                    {
                        stack.Components.Set(new EnchantmentsComponent(levels));
                        TuiTheme.ShowSuccess($"Saved {levels.Count} enchantment(s).");
                    }
                    TuiTheme.WaitForKey();
                    return;
            }
        }
    }
}