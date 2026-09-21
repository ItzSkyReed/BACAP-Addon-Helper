using UI.Actions.Common;
using UI.Extensions;
using UI.Interfaces;
using UI.Styling;

namespace UI.Menus;

/// <summary>
/// Sub-menu for managing existing advancements.
/// </summary>
public class ManageDatapacksMenu(IEnumerable<IManageDatapacksAction> actions) : IMainMenuAction
{
    public string Title => "Manage Datapacks";

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
            TuiTheme.RenderHeader("Manage Datapacks");

            var selected = await TuiTheme.PromptSelectionOrDefaultAsync(
                "Select an [green]operation[/] (press [bold]Q[/] to return):",
                choices,
                action => action.Title);

            if (selected is null or BackAction)
                break;

            await selected.ExecuteSafelyAsync();
        }
    }
}