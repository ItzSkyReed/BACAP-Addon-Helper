using BacapGenerator.Advancements.Models;
using BacapGenerator.Io;
using Spectre.Console;
using UI.Actions.Advancements.Functions.Menus;
using UI.Interfaces;

namespace UI.Actions.Advancements.Functions;

/// <summary>
/// Action that configures or clears item rewards for a specific BACAP advancement.
/// </summary>
/// <param name="advancement">The target advancement to configure.</param>
/// <param name="askConfirmation">
/// If <see langword="true"/>, prompts the user whether to assign item rewards before opening the menu.
/// If <see langword="false"/>, opens the item editor menu directly.
/// </param>
public class ChangeItemAction(BacapAdvancement advancement, bool askConfirmation = true) : IManageAdvancementsAction
{
    public string Title => "Change item reward";

    /// <summary>
    /// Routes to the item editor menu or optionally prompts the user to assign or clear item rewards.
    /// </summary>
    /// <returns>A completed <see cref="Task"/>.</returns>
    public async Task ExecuteAsync()
    {
        if (askConfirmation)
        {
            var hasExistingItems = advancement.ItemRewardFunction.RewardItems.Count > 0;

            var promptText = hasExistingItems
                ? $"Manage item rewards for [green]{Markup.Escape(advancement.TitleText)}[/]?"
                : $"Add an Item reward to [green]{Markup.Escape(advancement.TitleText)}[/]?";

            var wantItem = await AnsiConsole.ConfirmAsync(promptText, defaultValue: hasExistingItems);

            if (!wantItem)
            {
                // Clear existing rewards and save an empty function file
                advancement.ItemRewardFunction.ClearRewardItems();
                AdvancementIoManager.SaveRewardFunction(advancement, advancement.ItemRewardFunction);
                return;
            }
        }

        // Open the dedicated interactive item management menu
        await ItemRewardMenu.Open(advancement);
    }
}