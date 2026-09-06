using BacapGenerator.Models.Advancements;
using BacapGenerator.Services.IO;
using Spectre.Console;
using UI.Interfaces;
using UI.Styling;

namespace UI.Actions.Advancements.Functions;

/// <summary>
/// Action that configures or clears the experience reward for a specific BACAP advancement.
/// </summary>
public class ChangeExpAction(BacapAdvancement advancement) : IManageAdvancementsAction
{
    public string Title => "Change experience reward";

    /// <summary>
    /// Prompts the user to grant an experience reward and persists the updated advancement state to disk.
    /// Continues prompting until a valid non-negative integer is provided.
    /// </summary>
    /// <returns>A completed <see cref="Task"/>.</returns>
    public Task ExecuteAsync()
    {
        var wantExp = AnsiConsole.Confirm(
            $"Add an Experience reward to [green]{Markup.Escape(advancement.TitleText)}[/]?",
            defaultValue: true
        );

        var amount = 0;

        if (wantExp)
        {
            amount = AnsiConsole.Prompt(
                new TextPrompt<int>("Enter experience amount (or [yellow]0[/] to leave empty):")
                    .DefaultValue(advancement.ExpRewardFunction.ExperienceAmount)
                    .ValidationErrorMessage("[red]Please enter a valid integer number.[/]")
                    .Validate(val => val switch
                    {
                        < 0 => ValidationResult.Error("[red]Experience amount cannot be negative.[/]"),
                        _ => ValidationResult.Success()
                    })
            );
        }

        advancement.ExpRewardFunction.ExperienceAmount = amount;

        AdvancementIoManager.SaveAdvancement(advancement);

        TuiTheme.ShowSuccess(amount == 0
            ? "Experience reward cleared. An empty function file has been generated."
            : $"Experience reward set to {amount} points and successfully saved.");

        TuiTheme.WaitForKey();
        return Task.CompletedTask;
    }
}