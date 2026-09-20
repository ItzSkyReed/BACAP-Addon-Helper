using UI.Interfaces;

namespace UI.Actions.Common;

/// <summary>
/// A reusable dummy action representing an "Add" option in menus.
/// Intercepted by menu loops to trigger creation workflows.
/// </summary>
/// <param name="title">The display title of the action. Defaults to <c>"[green]+ Add[/]"</c>.</param>
/// <example>
/// <code>
/// var addAction = new AddAction("[green]+ Add New Item[/]");
/// </code>
/// </example>
public class AddAction(string title = "[green]+ Add[/]") : ITuiAction
{
    public string Title { get; } = title;

    public Task ExecuteAsync() => Task.CompletedTask;
}