using BacapGenerator.Models.Advancements;
using BacapGenerator.Models.Datapacks;
using BacapGenerator.Models.Datapacks.Settings;
using BacapGenerator.Services.IO;
using Spectre.Console;
using UI.Actions.Common;
using UI.Interfaces;
using UI.Styling;

namespace UI.Actions.Advancements;

/// <summary>
/// Action that provides a search interface to find specific BACAP advancements by title or path
/// and displays their detailed information.
/// </summary>
public class AdvancementDeleteAction(DatapackRegistry registry) : IManageAdvancementsAction
{
    public string Title => "Delete Advancement";

    public Task ExecuteAsync()
    {
        var allAdvancements = registry.Values
            .Where(dp => dp.Settings.Access == DatapackAccess.ReadWrite)
            .SelectMany(dp => dp.Advancements.OfType<BacapAdvancement>())
            .ToList();

        if (allAdvancements.Count == 0)
        {
            TuiTheme.ShowWarning("No advancements found across datapacks with ReadWrite access.");
            TuiTheme.WaitForKey();
            return Task.CompletedTask;
        }

        while (true)
        {
            var selectedAdv = AdvancementSearcher.PromptSearch(allAdvancements);

            if (selectedAdv == null)
                break;

            var confirm = AnsiConsole.Confirm($"Are you sure you want to delete [green]{selectedAdv.TitleText}[/] advancement?");
            if (!confirm)
                return Task.CompletedTask;

            DeleteAdvancement(selectedAdv);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Displays detailed information about a specific advancement.
    /// </summary>
    /// <param name="adv">The advancement to display details for.</param>
    private static void DeleteAdvancement(BacapAdvancement adv)
    {

        AdvancementIoManager.DeleteAdvancement(adv);

        TuiTheme.ShowSuccess("Advancement deleted successfully.");
        TuiTheme.WaitForKey();
    }
}