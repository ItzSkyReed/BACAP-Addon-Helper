using Spectre.Console;
using UI.Interfaces;
using UI.Styling;

namespace UI.Extensions;

/// <summary>
/// Provides extension methods for safe execution of TUI actions within an isolated error boundary.
/// </summary>
public static class TuiActionExtensions
{
    /// <summary>
    /// Executes the action within a global error boundary, gracefully handling user cancellations
    /// and rendering formatted TUI alert panels for unhandled exceptions without crashing the process.
    /// </summary>
    /// <param name="action">The target action to execute.</param>
    /// <returns>A task representing the asynchronous safe execution.</returns>
    /// <example>
    /// <code>
    /// await selectedAction.ExecuteSafelyAsync();
    /// </code>
    /// </example>
    public static async Task ExecuteSafelyAsync(this ITuiAction action)
    {
        ArgumentNullException.ThrowIfNull(action);

        try
        {
            await action.ExecuteAsync();
        }
        catch (OperationCanceledException)
        {
            // User aborted the prompt/flow via Escape or Q
        }
        catch (Exception ex)
        {
            TuiTheme.ShowAlert(
                header: "[bold red] Action Failed [/]",
                content: $"An unexpected error occurred while running [yellow]{Markup.Escape(action.Title)}[/]:\n\n" +
                         $"[red]{Markup.Escape(ex.Message)}[/]\n\n" +
                         $"[grey]Exception: {Markup.Escape(ex.GetType().Name)}[/]",
                borderColor: Color.Red);

            TuiTheme.WaitForKey();
        }
    }
}