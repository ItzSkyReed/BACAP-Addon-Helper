using System.Text.RegularExpressions;
using BacapGenerator.Configuration.Exceptions;
using Microsoft.Extensions.Options;
using Spectre.Console;
using UI.Styling;

namespace UI.Diagnostics;

/// <summary>
/// Helper class for rendering user-friendly error alerts for configuration and template bootstrap failures.
/// </summary>
public static partial class ConfigurationErrorHandler
{
    [GeneratedRegex("at '([^']+)'")]
    private static partial Regex ConfigPathRegex();

    /// <summary>
    /// Displays a styled TUI alert when a configuration value fails type conversion or binding.
    /// </summary>
    /// <param name="ex">The binding exception thrown by the configuration binder.</param>
    /// <example>
    /// <code>
    /// ConfigurationErrorHandler.RenderBindingError(invalidOpEx);
    /// </code>
    /// </example>
    public static void RenderBindingError(InvalidOperationException ex)
    {
        // Extract inner FormatException or fallback to the root message
        var formatException = FindInnerException<FormatException>(ex);
        var rawMessage = formatException?.Message ?? ex.Message;

        // Attempt to extract the configuration key; fallback to a generic placeholder if missing
        var match = ConfigPathRegex().Match(ex.Message);
        var configKey = match.Success ? match.Groups[1].Value : null;

        var locationText = configKey is not null
            ? $"at [yellow]'{Markup.Escape(configKey)}'[/]"
            : "in configuration";

        var tipText = configKey is not null
            ? $"Check [white]config.yaml[/] at [yellow]'{Markup.Escape(configKey)}'[/] and set a valid value."
            : "Check [white]config.yaml[/] and ensure all values match their expected types.";

        var content = $"""
                       Invalid configuration value {locationText}:

                       [bold red]{Markup.Escape(rawMessage)}[/]

                       [bold white]Tip:[/] {tipText}
                       """;

        TuiTheme.ShowAlert(
            header: "[bold red] Configuration Value Error [/]",
            content: content,
            borderColor: Color.Red);
    }

    /// <summary>
    /// Traverses the exception hierarchy (including aggregate branches) to find an exception of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The specific exception type to search for.</typeparam>
    /// <param name="ex">The root exception to inspect.</param>
    /// <returns>The first matched exception instance, or <see langword="null"/> if not found.</returns>
    private static T? FindInnerException<T>(Exception? ex) where T : Exception
    {
        switch (ex)
        {
            case null:
                return null;
            case T match:
                return match;
            case AggregateException agg:
            {
                foreach (var inner in agg.InnerExceptions)
                {
                    var found = FindInnerException<T>(inner);
                    if (found is not null)
                        return found;
                }

                break;
            }
        }

        return FindInnerException<T>(ex.InnerException);
    }

    /// <summary>
    /// Displays a styled TUI alert for template resolution errors.
    /// </summary>
    /// <param name="ex">The configuration template exception.</param>
    public static void RenderTemplateError(ConfigurationTemplateException ex)
    {
        var content = ex.Kind switch
        {
            ConfigurationTemplateErrorKind.TemplateNotFound =>
                $"Template [bold yellow]'{Markup.Escape(ex.TemplateName)}'[/] was not found.\n\n" +
                $"Referenced by: [white]{Markup.Escape(ex.TargetSectionPath)}[/]\n" +
                $"Searched path: [cyan]{Markup.Escape(ex.ExpectedTemplatePath)}[/]\n\n" +
                $"[bold white]Tip:[/] Verify that the template is defined under the [yellow]templates[/] section in [white]config.yaml[/].",

            ConfigurationTemplateErrorKind.CircularDependency =>
                $"Circular template inheritance detected!\n\n" +
                $"Template: [bold yellow]'{Markup.Escape(ex.TemplateName)}'[/]\n" +
                $"Path: [cyan]{Markup.Escape(ex.ExpectedTemplatePath)}[/]\n\n" +
                $"[bold white]Tip:[/] Ensure templates do not reference themselves directly or indirectly.",

            _ => Markup.Escape(ex.Message)
        };

        TuiTheme.ShowAlert("[bold red] Template Resolution Error [/]", content, Color.Red);
    }

    /// <summary>
    /// Displays a styled TUI alert for YAML syntax errors or options validation failures.
    /// </summary>
    /// <param name="ex">The caught exception during host bootstrapping.</param>
    public static void RenderBootstrapError(Exception ex)
    {
        if (ex is OptionsValidationException optEx)
        {
            var failures = string.Join("\n[grey]•[/] ", optEx.Failures.Select(Markup.Escape));
            TuiTheme.ShowAlert(
                header: "[bold red] Configuration Validation Failed [/]",
                content: $"The configuration file contains invalid settings:\n\n[grey]•[/] {failures}",
                borderColor: Color.Red);
            return;
        }

        // Detect YamlException (from YamlDotNet or NetEscapades.Configuration.Yaml)
        var rawMessage = ex.InnerException?.Message ?? ex.Message;

        TuiTheme.ShowAlert(
            header: "[bold red] Configuration Load Error [/]",
            content: $"Failed to parse [yellow]config.yaml[/]:\n\n" +
                     $"[white]{Markup.Escape(rawMessage)}[/]\n\n" +
                     $"[bold white]Tip:[/] Check for invalid indentation, unclosed quotes, or tab characters.",
            borderColor: Color.Red);
    }
}