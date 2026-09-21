using BacapGenerator.Configuration.Exceptions;
using Microsoft.Extensions.Options;
using Spectre.Console;
using UI.Styling;

namespace UI.Diagnostics;

/// <summary>
/// Helper class for rendering user-friendly error alerts for configuration and template bootstrap failures.
/// </summary>
public static class ConfigurationErrorHandler
{
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