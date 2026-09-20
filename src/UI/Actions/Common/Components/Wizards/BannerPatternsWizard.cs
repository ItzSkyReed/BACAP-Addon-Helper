using Core.DataComponents.Components;
using Core.DataComponents.Models;
using Core.Items;
using Core.Registries;
using Spectre.Console;
using UI.Styling;

namespace UI.Actions.Common.Components.Wizards;

/// <summary>
/// Wizard for configuring layered banner patterns validated against registries (<c>minecraft:banner_patterns</c>).
/// </summary>
public class BannerPatternsWizard : IComponentWizard
{
    /// <inheritdoc/>
    public string ComponentId => BannerPatternsComponent.ComponentId;

    /// <inheritdoc/>
    public string DisplayTitle => "Banner Patterns (minecraft:banner_patterns)";

    /// <inheritdoc/>
    public void Execute(ItemStack stack, MinecraftData mcData)
    {
        var existing = stack.Components.Get<BannerPatternsComponent>();
        var patterns = existing?.Patterns.ToList() ?? [];

        while (true)
        {
            TuiTheme.RenderHeader("Banner Patterns Configuration");

            if (patterns.Count == 0)
                AnsiConsole.MarkupLine("[grey]No pattern layers configured.[/]\n");
            else
            {
                for (var i = 0; i < patterns.Count; i++)
                    AnsiConsole.MarkupLine($"[grey]Layer {i + 1}:[/] [cyan]{patterns[i].ToSnbt().ToSnbtString()}[/]");
                AnsiConsole.WriteLine();
            }

            var action = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Choose an action:")
                    .AddChoices("+ Add Pattern Layer", "- Remove Last Layer", "[red]Clear All[/]", "Done (Save)")
            );

            switch (action)
            {
                case "+ Add Pattern Layer":
                    var selectedPattern = AnsiConsole.Prompt(
                        new SelectionPrompt<string>()
                            .Title("Select pattern (type to search):")
                            .EnableSearch()
                            .PageSize(15)
                            .AddChoices(mcData.BannerPatterns)
                    );

                    var selectedColor = AnsiConsole.Prompt(
                        new SelectionPrompt<string>()
                            .Title("Select dye color (type to search):")
                            .EnableSearch()
                            .PageSize(16)
                            .AddChoices(mcData.DyeColors.Keys)
                    );

                    var fullPattern = selectedPattern.Contains(':') ? selectedPattern : $"minecraft:{selectedPattern}";
                    patterns.Add(new BannerPatternLayer(fullPattern, selectedColor));
                    break;

                case "- Remove Last Layer":
                    if (patterns.Count > 0) patterns.RemoveAt(patterns.Count - 1);
                    break;

                case "[red]Clear All[/]":
                    patterns.Clear();
                    break;

                case "Done (Save)":
                    if (patterns.Count == 0)
                    {
                        stack.Components.Remove<BannerPatternsComponent>();
                        TuiTheme.ShowSuccess("Banner patterns removed.");
                    }
                    else
                    {
                        stack.Components.Set(new BannerPatternsComponent(patterns));
                        TuiTheme.ShowSuccess($"Saved {patterns.Count} pattern layers.");
                    }
                    TuiTheme.WaitForKey();
                    return;
            }
        }
    }
}