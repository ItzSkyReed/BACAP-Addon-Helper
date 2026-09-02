using UI.Actions.Common;
using UI.Interfaces;
using UI.Styling;

namespace UI.Menus;

/// <summary>
/// Sub-menu for managing existing advancements.
/// </summary>
public class ManageAdvancementsMenu(IEnumerable<IManageAdvancementsAction> actions) : IMainMenuAction
{
    public string Title => "Manage Advancements";

    public async Task ExecuteAsync()
    {
        var choices = actions.ToList<ITuiAction>();
        var backAction = new BackAction();
        choices.Add(backAction);

        // Check if only the BackAction is present
        if (choices.Count == 1)
        {
            TuiTheme.ShowWarning("No management actions are currently registered.");
            TuiTheme.WaitForKey();
            return;
        }

        while (true)
        {
            TuiTheme.RenderHeader("Manage Advancements");

            var selected = TuiTheme.PromptSelection(
                "Select an [green]operation[/]:",
                choices,
                action => action.Title);

            if (selected is BackAction)
                break;

            await selected.ExecuteAsync();
        }
    }
}