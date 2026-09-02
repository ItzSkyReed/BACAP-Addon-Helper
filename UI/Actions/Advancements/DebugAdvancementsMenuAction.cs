using UI.Actions.Common;
using UI.Interfaces;
using UI.Styling;

namespace UI.Actions.Advancements;

/// <summary>
/// Sub-menu action that groups together various debug utilities for advancements.
/// </summary>
/// <param name="debugActions">The collection of debug actions injected by the DI container.</param>
public class DebugAdvancementsMenuAction(IEnumerable<IDebugAdvancementsAction> debugActions) : IManageAdvancementsAction
{
    /// <inheritdoc/>
    public string Title => "Debug Menu";

    /// <summary>
    /// Executes the debug sub-menu, allowing the user to select specific debug actions.
    /// </summary>
    public async Task ExecuteAsync()
    {
        var choices = debugActions.ToList<ITuiAction>();
        choices.Add(new BackAction());
        if (choices.Count == 1)
        {
            TuiTheme.ShowWarning("No debug actions are currently registered.");
            TuiTheme.WaitForKey();
            return;
        }

        while (true)
        {
            TuiTheme.RenderHeader("Manage Advancements -> Debug");

            var selected = TuiTheme.PromptSelection(
                "Select a [green]debug action[/]:",
                choices,
                action => action.Title);

            if (selected is BackAction)
                break;

            await selected.ExecuteAsync();
        }
    }
}