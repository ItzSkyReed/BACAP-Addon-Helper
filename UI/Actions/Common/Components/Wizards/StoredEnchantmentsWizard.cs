using Core.DataComponents.Components;
using Core.Items;
using Core.Registries;
using Spectre.Console;
using UI.Styling;

namespace UI.Actions.Common.Components.Wizards;

/// <summary>
/// Wizard for configuring stored enchantments on enchanted books (<c>minecraft:stored_enchantments</c>).
/// </summary>
public class StoredEnchantmentsWizard : IComponentWizard
{
    /// <inheritdoc/>
    public string ComponentId => StoredEnchantmentsComponent.ComponentId;

    /// <inheritdoc/>
    public string DisplayTitle => "Stored Enchantments (minecraft:stored_enchantments)";

    /// <inheritdoc/>
    public void Execute(ItemStack stack, MinecraftData mcData)
    {
        var existing = stack.Components.Get<StoredEnchantmentsComponent>();
        var levels = existing != null ? new Dictionary<string, int>(existing.Levels) : [];

        while (true)
        {
            TuiTheme.RenderHeader("Stored Enchantments Configuration (Enchanted Book)");

            if (levels.Count == 0)
                AnsiConsole.MarkupLine("[grey]No stored enchantments.[/]\n");
            else
            {
                foreach (var (enchId, lvl) in levels)
                    AnsiConsole.MarkupLine($"[cyan]{Markup.Escape(enchId)}[/]: [yellow]{lvl}[/]");
                AnsiConsole.WriteLine();
            }

            var action = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Choose an action:")
                    .AddChoices("+ Add/Edit Stored Enchantment", "- Remove Stored Enchantment", "[red]Clear All[/]", "Done (Save)")
            );

            switch (action)
            {
                case "+ Add/Edit Stored Enchantment":
                    var selectedEnch = AnsiConsole.Prompt(
                        new SelectionPrompt<string>()
                            .Title("Select an enchantment (type to search):")
                            .EnableSearch()
                            .PageSize(15)
                            .AddChoices(mcData.Enchantments.Keys)
                    );

                    var currentLvl = levels.GetValueOrDefault(selectedEnch, 1);
                    var level = AnsiConsole.Prompt(
                        new TextPrompt<int>($"Enter level for [yellow]{selectedEnch}[/]:")
                            .DefaultValue(currentLvl)
                            .Validate(v => v > 0 ? ValidationResult.Success() : ValidationResult.Error("Level must be >= 1"))
                    );

                    var fullId = selectedEnch.Contains(':') ? selectedEnch : $"minecraft:{selectedEnch}";
                    levels[fullId] = level;
                    break;

                case "- Remove Stored Enchantment":
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
                        stack.Components.Remove<StoredEnchantmentsComponent>();
                        TuiTheme.ShowSuccess("Stored enchantments removed.");
                    }
                    else
                    {
                        var showTooltip = AnsiConsole.Confirm("Show in tooltip?", defaultValue: existing?.ShowInTooltip ?? true);
                        stack.Components.Set(new StoredEnchantmentsComponent(levels, showTooltip));
                        TuiTheme.ShowSuccess($"Saved {levels.Count} stored enchantment(s).");
                    }
                    TuiTheme.WaitForKey();
                    return;
            }
        }
    }
}