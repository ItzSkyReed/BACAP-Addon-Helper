using Core.Registries.Exceptions;
using Spectre.Console;
using UI.Styling;

namespace UI.Diagnostics;

/// <summary>
/// Helper class for rendering user-friendly error panels for registry failures.
/// </summary>
public static class RegistryErrorHandler
{
    /// <summary>
    /// Formats and displays a stylized TUI alert panel for a registry load failure.
    /// </summary>
    /// <param name="exception">The encountered registry load exception.</param>
    /// <example>
    /// <code>
    /// try
    /// {
    ///     var data = new MinecraftData(loader);
    /// }
    /// catch (RegistryLoadException ex)
    /// {
    ///     RegistryErrorHandler.RenderError(ex);
    ///     Environment.Exit(1);
    /// }
    /// </code>
    /// </example>
    public static void RenderError(RegistryLoadException exception)
    {
        var escapedPath = exception.TargetPath is not null
            ? Markup.Escape(exception.TargetPath)
            : "Unknown";

        var (description, tip) = exception.Kind switch
        {
            RegistryErrorKind.DirectoryNotFound => (
                $"The configured registry directory does not exist:\n[bold cyan]{escapedPath}[/]",
                "Make sure [yellow]registry_base_path[/] in [white]config.yaml[/] points to an existing folder."
            ),
            RegistryErrorKind.FileNotFound => (
                $"Missing required registry file: [bold red]{exception.FileName}[/]\nTarget path: [bold cyan]{escapedPath}[/]",
                "Extract registries using [yellow]registry-dumper[/] and ensure all files are in place."
            ),
            RegistryErrorKind.InvalidJson => (
                $"Corrupted JSON format in [bold red]{exception.FileName}[/]:\n[grey]{Markup.Escape(exception.Message)}[/]",
                "Check the file for trailing commas or syntax errors."
            ),
            RegistryErrorKind.EmptyPayload => (
                $"Registry file [bold red]{exception.FileName}[/] is completely empty or has no entries.",
                "Ensure the dumper generated valid non-empty arrays/objects."
            ),
            _ => (
                Markup.Escape(exception.Message),
                "Check application configuration."
            )
        };

        var content = $"{description}\n\n[bold white]Tip:[/] {tip}";

        TuiTheme.ShowAlert(
            header: "[bold red] Registry Loading Failed [/]",
            content: content,
            borderColor: Color.Red);
    }
}