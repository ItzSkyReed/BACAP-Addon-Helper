using BacapGenerator.Models.Advancements;
using BacapGenerator.Services.IO;
using Spectre.Console;
using UI.Actions.Advancements.Functions.Menus;
using UI.Interfaces;
using UI.Styling;

namespace UI.Actions.Advancements.Functions;

/// <summary>
/// Action that configures or clears item rewards for a specific BACAP advancement.
/// </summary>
public class ChangeItemAction(BacapAdvancement advancement) : IManageAdvancementsAction
{
    public string Title => "Change item reward";

    /// <summary>
    /// Prompts the user whether to assign item rewards, routing to the item editor menu or clearing existing rewards.
    /// </summary>
    /// <returns>A completed <see cref="Task"/>.</returns>
    public async Task ExecuteAsync()
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
            AdvancementIoManager.SaveAdvancement(advancement);

            return;
        }

        // Open the dedicated interactive item management menu
        await ItemRewardMenu.Open(advancement);
    }
}