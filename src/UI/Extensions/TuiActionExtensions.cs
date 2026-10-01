using Spectre.Console;
using UI.Interfaces;
using UI.Styling;
using UI.Diagnostics;

namespace UI.Extensions;

/// <summary>
/// Provides extension methods for safe execution of TUI actions within an isolated error boundary.
/// </summary>
public static class TuiActionExtensions
{
    /// <summary>
    /// Executes the action within a global error boundary, gracefully handling user cancellations
    /// and delegating exception rendering to the central application error handler.
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
            AppErrorHandler.Handle(ex, showStackTrace: false);

            AnsiConsole.MarkupLine($"\n[grey]Action '[white]{Markup.Escape(action.Title)}[/]' aborted due to the error above.[/]");
            TuiTheme.WaitForKey();
        }
    }
}