using BacapGenerator.Advancements.Models;
using BacapGenerator.Io;
using Spectre.Console;
using UI.Actions.Advancements.Functions.Menus;
using UI.Interfaces;

namespace UI.Actions.Advancements.Functions;

/// <summary>
/// Action that configures or clears trophy rewards for a specific BACAP advancement.
/// </summary>
public class ChangeTrophyAction(BacapAdvancement advancement) : IManageAdvancementsAction
{
    public string Title => "Change trophy reward";

    /// <summary>
    /// Prompts the user whether to assign trophy rewards, routing to the trophy editor menu or clearing existing rewards.
    /// </summary>
    /// <returns>A completed <see cref="Task"/>.</returns>
    public async Task ExecuteAsync()
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

        await TrophyRewardMenu.Open(advancement);

    }
}