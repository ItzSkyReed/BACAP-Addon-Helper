using UI.Actions.Common;
using UI.Interfaces;
using UI.Styling;

namespace UI.Menus;

/// <summary>
/// The root menu of the application.
/// </summary>
public class MainMenuAction(IEnumerable<IMainMenuAction> menuActions) : ITuiAction
{
    public string Title => "Main Menu";

    public async Task ExecuteAsync()
    {
        // Add a "Quit" option dynamically
        var choices = menuActions.ToList<ITuiAction>();
        choices.Add(new BackAction("[grey]Exit[/]"));

        while (true)
        {
            TuiTheme.RenderHeader("BACAP Manager");

            var selected = TuiTheme.PromptSelection(
                "Choose actions",
                choices,
                action => action.Title);

            if (selected is BackAction)
                break;

            await selected.ExecuteAsync();
        }
    }
}