using Core.DataComponents;
using Core.DataComponents.Interfaces;
using Core.Items;
using Core.Registries;
using Pidgin;
using Spectre.Console;
using UI.Styling;

namespace UI.Actions.Common.Components;

/// <summary>
/// Provides an interactive menu for inspecting, adding, editing, and removing Minecraft item data components.
/// Combines popular action wizards with raw SNBT/command-format string parsing.
/// </summary>
public static class ItemComponentsMenu
{
    private sealed record AddPopularAction(string Label);

    private sealed record PasteRawAction;

    private sealed record ClearAllAction;

    /// <summary>
    /// Opens the component editing loop for the specified item stack.
    /// </summary>
    /// <param name="stack">The item stack whose components are being modified.</param>
    /// <param name="mcData">The loaded Minecraft static registries.</param>
    /// <returns>The updated <see cref="ItemStack"/> instance.</returns>
    public static ItemStack Open(ItemStack stack, MinecraftData mcData)
    {
        while (true)
        {
            TuiTheme.RenderHeader($"Components: {stack.Id}");

            // If there are existing components, allow selecting them to inspect/remove
            var choices = stack.Components.Cast<object>().ToList();

            choices.Add(new AddPopularAction("[cyan]+ Wizard: Common Components[/]"));
            choices.Add(new PasteRawAction());
            if (!stack.Components.IsEmpty)
            {
                choices.Add(new ClearAllAction());
            }

            choices.Add(new BackAction());

            var selected = TuiTheme.PromptSelection(
                "Select a component to remove/inspect or choose an action:",
                choices,
                item => item switch
                {
                    BackAction nav => nav.Title,
                    IDataComponent comp => $"[yellow]{comp.Id}[/] [grey]({Markup.Escape(comp.ToSnbt().ToSnbtString())})[/]",
                    AddPopularAction pop => pop.Label,
                    PasteRawAction => "[green]+ Paste Raw Components[/]",
                    ClearAllAction => "[red]- Clear All Components[/]",
                    _ => item.ToString()!
                });

            switch (selected)
            {
                case BackAction:
                    return stack;

                case ClearAllAction:
                    if (AnsiConsole.Confirm("Are you sure you want to remove all components from this item?", defaultValue: false))
                    {
                        stack = stack with { Components = new DataComponentMap() };
                        TuiTheme.ShowSuccess("All components cleared.");
                    }

                    break;

                case IDataComponent comp:
                    if (AnsiConsole.Confirm($"Remove component [yellow]{comp.Id}[/]?", defaultValue: true))
                    {
                        stack.Components.Remove(comp.Id);
                        TuiTheme.ShowSuccess($"Removed {comp.Id}");
                    }

                    break;

                case PasteRawAction:
                    stack = PromptPasteRawComponents(stack);
                    break;

                case AddPopularAction:
                    stack = OpenPopularWizardsMenu(stack, mcData);
                    break;
            }
        }
    }

    /// <summary>
    /// Prompts the user to input raw component definitions either as a bracketed list or a comma-separated list.
    /// </summary>
    /// <param name="stack">The item stack whose components are being modified.</param>
    /// <returns>The modified <see cref="ItemStack"/> with parsed components applied.</returns>
    private static ItemStack PromptPasteRawComponents(ItemStack stack)
    {
        TuiTheme.RenderHeader("Paste Raw Components");
        AnsiConsole.MarkupLine("[grey]Supported formats:[/]");
        AnsiConsole.MarkupLine("[grey]  1. Single: [/][cyan]minecraft:unbreakable={}[/]");
        AnsiConsole.MarkupLine("[grey]  2. Bracketed: [/][cyan][[minecraft:unbreakable={}, custom_name='{\"text\":\"Excalibur\"}']][/]");
        AnsiConsole.MarkupLine("[grey]  3. Removal: [/][cyan]!minecraft:damage[/]\n");

        var input = AnsiConsole.Prompt(
            new TextPrompt<string>("Enter component string (or leave empty to cancel):")
                .AllowEmpty()
        ).Trim();

        if (string.IsNullOrWhiteSpace(input))
        {
            return stack;
        }

        try
        {
            ItemStackParser.ApplyComponents(input, stack.Components);

            TuiTheme.ShowSuccess("Successfully parsed and applied components.");
            TuiTheme.WaitForKey();
        }
        catch (ParseException ex)
        {
            TuiTheme.ShowWarning($"Failed to parse component: {ex.Message}");
            TuiTheme.WaitForKey();
        }
        catch (Exception ex)
        {
            TuiTheme.ShowWarning($"Error applying components: {ex.Message}");
            TuiTheme.WaitForKey();
        }

        return stack;
    }

    /// <summary>
    /// Opens the extensible wizard registry menu.
    /// </summary>
    /// <param name="stack">The current item stack.</param>
    /// <param name="mcData">Loaded Minecraft registry collections.</param>
    /// <returns>The updated item stack.</returns>
    private static ItemStack OpenPopularWizardsMenu(ItemStack stack, MinecraftData mcData)
    {
        return ComponentWizardRegistry.OpenMenu(stack, mcData);
    }
}