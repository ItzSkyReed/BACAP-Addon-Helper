using BacapGenerator.Configuration.Exceptions;
using Core.Registries.Exceptions;
using Microsoft.Extensions.Options;
using Spectre.Console;
using UI.Styling;

namespace UI.Diagnostics;

/// <summary>
/// Central dispatcher responsible for rendering readable TUI alerts and diagnosing fatal runtime exceptions.
/// </summary>
public static class AppErrorHandler
{
    /// <summary>
    /// Handles and renders any application exception with contextual troubleshooting tips.
    /// </summary>
    /// <param name="exception">The encountered exception.</param>
    /// <param name="showStackTrace">Whether to display full stack trace details.</param>
    /// <returns>The suggested exit code (e.g. 1 for failure).</returns>
    /// <example>
    /// <code>
    /// try
    /// {
    ///     await app.RunAsync();
    /// }
    /// catch (Exception ex)
    /// {
    ///     return AppErrorHandler.Handle(ex, verbose);
    /// }
    /// </code>
    /// </example>
    public static int Handle(Exception exception, bool showStackTrace = false)
    {
        switch (exception)
        {
            case RegistryLoadException regEx:
                RegistryErrorHandler.RenderError(regEx);
                break;

            case ConfigurationTemplateException tmplEx:
                ConfigurationErrorHandler.RenderTemplateError(tmplEx);
                break;

            case DatapackConfigurationException dpEx:
                RenderDatapackError(dpEx);
                break;

            case OptionsValidationException:
            case InvalidOperationException when exception.Source?.Contains("Configuration") == true:
                ConfigurationErrorHandler.RenderBootstrapError(exception);
                break;

            case FileNotFoundException fnfEx:
                RenderFileNotFoundError(fnfEx);
                break;

            case DirectoryNotFoundException dnfEx:
                RenderDirectoryNotFoundError(dnfEx);
                break;

            default:
                RenderGenericCrash(exception);
                break;
        }

        if (showStackTrace)
        {
            RenderStackTraceTree(exception);
        }

        return 1;
    }

    /// <summary>
    /// Displays a styled TUI alert for datapack structural configuration errors.
    /// </summary>
    /// <param name="ex">The datapack configuration exception.</param>
    private static void RenderDatapackError(DatapackConfigurationException ex)
    {
        var escapedId = Markup.Escape(ex.DatapackId);
        var escapedDetail = ex.Detail is not null ? Markup.Escape(ex.Detail) : "N/A";

        var (description, tip) = ex.Kind switch
        {
            DatapackErrorKind.MissingPath => (
                $"Datapack [bold yellow]'{escapedId}'[/] is missing the [yellow]path[/] attribute.",
                "Specify a relative or absolute filesystem directory in [white]config.yaml[/]."
            ),
            DatapackErrorKind.MissingNamespace => (
                $"Datapack [bold yellow]'{escapedId}'[/] requires both [yellow]main_namespace[/] and [yellow]reward_namespace[/].",
                "Ensure both namespaces are defined without colons (e.g. 'bacaped', 'bacaped_rewards')."
            ),
            DatapackErrorKind.MissingParentDatapack => (
                $"Compatibility addon [bold yellow]'{escapedId}'[/] references unknown parent: [bold red]'{escapedDetail}'[/].",
                "Check that the parent datapack key exists under the [yellow]datapacks[/] section."
            ),
            DatapackErrorKind.InvalidChecklistConfiguration => (
                $"Checklist validation failed for [bold yellow]'{escapedId}'[/] (entry: [bold cyan]'{escapedDetail}'[/]).",
                "Verify that [yellow]id[/], [yellow]trigger_scoreboard[/], and [yellow]storage_name[/] are specified."
            ),
            DatapackErrorKind.MissingLanguagePackPath => (
                $"Language pack for [bold yellow]'{escapedId}'[/] is missing the [yellow]path[/] attribute.",
                "Specify [yellow]language_pack.path[/] in [white]config.yaml[/] pointing to the resource pack root."
            ),
            DatapackErrorKind.InvalidLanguagePackConfiguration => (
                $"Invalid language pack configuration for [bold yellow]'{escapedId}'[/]: [bold red]{escapedDetail}[/].",
                "Ensure path contains valid characters and [yellow]ignored_keys[/] does not contain empty values."
            ),
            DatapackErrorKind.InvalidValidationRuleConfiguration => (
                $"Invalid validation rule configuration in [bold yellow]'{escapedId}'[/]: [bold red]{escapedDetail}[/].",
                "Verify rule severity names and ensure allowed string lists do not contain empty items."
            ),
            _ => (Markup.Escape(ex.Message), "Check datapack definitions in config.yaml.")
        };

        var content = $"{description}\n\n[bold white]Tip:[/] {tip}";
        TuiTheme.ShowAlert($"[bold red] Datapack Settings Error [/]", content, Color.Red);
    }

    /// <summary>
    /// Displays a styled TUI alert for missing filesystem files.
    /// </summary>
    /// <param name="ex">The file not found exception.</param>
    private static void RenderFileNotFoundError(FileNotFoundException ex)
    {
        var path = ex.FileName is not null ? Markup.Escape(ex.FileName) : "Unknown path";
        var content = $"Required file could not be found:\n[bold cyan]{path}[/]\n\n" +
                      $"[bold white]Tip:[/] Check if the file was deleted or moved, and verify relative paths in [yellow]config.yaml[/].";

        TuiTheme.ShowAlert("[bold red] File Not Found [/]", content, Color.Red);
    }

    /// <summary>
    /// Displays a styled TUI alert for missing directories.
    /// </summary>
    /// <param name="ex">The directory not found exception.</param>
    private static void RenderDirectoryNotFoundError(DirectoryNotFoundException ex)
    {
        var content = $"Target directory does not exist:\n[bold cyan]{Markup.Escape(ex.Message)}[/]\n\n" +
                      $"[bold white]Tip:[/] Ensure all referenced folders exist or create them before executing.";

        TuiTheme.ShowAlert("[bold red] Directory Not Found [/]", content, Color.Red);
    }

    /// <summary>
    /// Displays an unexpected runtime error panel.
    /// </summary>
    /// <param name="ex">The unhandled exception.</param>
    private static void RenderGenericCrash(Exception ex)
    {
        var content = $"An unexpected error halted execution:\n\n" +
                      $"[bold white]{Markup.Escape(ex.GetType().Name)}:[/] {Markup.Escape(ex.Message)}\n\n" +
                      $"[grey]Report this issue to the developer[/]";

        TuiTheme.ShowAlert("[bold red] Unexpected Fatal Error [/]", content, Color.Red);
    }

    /// <summary>
    /// Visualizes the exception and its inner causes as an interactive Spectre.Console Tree.
    /// </summary>
    /// <param name="exception">The root exception to trace.</param>
    private static void RenderStackTraceTree(Exception exception)
    {
        var tree = new Tree("[bold red]Exception Details[/]");
        var current = exception;

        while (current != null)
        {
            var node = tree.AddNode($"[bold yellow]{Markup.Escape(current.GetType().FullName ?? current.GetType().Name)}[/]: {Markup.Escape(current.Message)}");

            if (!string.IsNullOrWhiteSpace(current.StackTrace))
            {
                var stackLines = current.StackTrace.Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                var stackNode = node.AddNode("[grey]Stack Trace[/]");
                foreach (var line in stackLines)
                {
                    stackNode.AddNode($"[grey]{Markup.Escape(line)}[/]");
                }
            }

            current = current.InnerException;
        }

        AnsiConsole.WriteLine();
        AnsiConsole.Write(tree);
        AnsiConsole.WriteLine();
    }
}