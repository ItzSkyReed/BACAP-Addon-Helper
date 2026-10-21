using BacapGenerator.Advancements.Models;
using BacapGenerator.Io;
using Spectre.Console;
using UI.Actions.Advancements.Functions.Menus;
using UI.Interfaces;

namespace UI.Actions.Advancements.Functions;

/// <summary>
/// Action that configures or clears trophy rewards for a specific BACAP advancement.
/// </summary>
/// <param name="advancement">The target advancement to configure.</param>
/// <param name="askConfirmation">
/// If <see langword="true"/>, prompts the user whether to assign trophy rewards before opening the menu.
/// If <see langword="false"/>, opens the trophy editor menu directly.
/// </param>
public class ChangeTrophyAction(BacapAdvancement advancement, bool askConfirmation = true) : IManageAdvancementsAction
{
    public string Title => "Change trophy reward";

    /// <summary>
    /// Routes to the trophy editor menu or optionally prompts the user to assign or clear trophy rewards.
    /// </summary>
    /// <returns>A completed <see cref="Task"/>.</returns>
    public async Task ExecuteAsync()
    {
        if (askConfirmation)
        {
            var hasExistingTrophies = advancement.TrophyRewardFunction.Trophies.Count > 0;

            var promptText = hasExistingTrophies
                ? $"Manage trophy rewards for [green]{Markup.Escape(advancement.TitleText)}[/]?"
                : $"Add a Trophy reward to [green]{Markup.Escape(advancement.TitleText)}[/]?";

            var wantTrophy = await AnsiConsole.ConfirmAsync(promptText, defaultValue: hasExistingTrophies);

            if (!wantTrophy)
            {
                advancement.TrophyRewardFunction.ClearTrophies();
                AdvancementIoManager.SaveRewardFunction(advancement, advancement.TrophyRewardFunction);
                return;
            }
        }

        await TrophyRewardMenu.Open(advancement);
    }
}