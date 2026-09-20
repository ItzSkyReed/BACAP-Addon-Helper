using UI.Interfaces;
using UI.Styling;

namespace UI.Actions.Common;

/// <summary>
/// A reusable dummy action to represent navigation options like "Back" or "Exit" in menus.
/// It is meant to be intercepted by the menu loop to break out of it.
/// </summary>
/// <param name="title">The display title of the navigation action. Defaults to "[grey]Back[/]".</param>
public class BackAction(string title = TuiTheme.BackOptionString) : ITuiAction
{
    public string Title { get; } = title;

    public Task ExecuteAsync() => Task.CompletedTask;
}