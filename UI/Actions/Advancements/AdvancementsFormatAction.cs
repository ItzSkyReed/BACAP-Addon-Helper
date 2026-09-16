using BacapGenerator.Advancements.Models;
using BacapGenerator.Datapacks;
using BacapGenerator.Datapacks.Models;
using BacapGenerator.Datapacks.Models.Settings;
using BacapGenerator.Io;
using Spectre.Console;
using UI.Interfaces;
using UI.Styling;

namespace UI.Actions.Advancements;

/// <summary>
/// Action that formats and rewrites all valid advancements across editable addon datapacks.
/// Standardizes JSON indentation for advancements and McFunction syntax for associated rewards.
/// </summary>
/// <param name="registry">The registry containing loaded datapack instances.</param>
public class AdvancementsFormatAction(DatapackRegistry registry) : IManageAdvancementsAction
{
    public string Title => "Format All Advancements";

    /// <summary>
    /// Executes the batch formatting process across all valid advancements in editable datapacks.
    /// Skips Reference (read-only) datapacks and broken (invalid) advancement files.
    /// </summary>
    /// <returns>A completed <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task ExecuteAsync()
    {
        var targetAdvancements = registry.Values
            .Where(dp => dp.Settings.Type != DatapackType.Reference)
            .SelectMany(dp => dp.Advancements.OfType<ValidAdvancement>())
            .ToList();

        if (targetAdvancements.Count == 0)
        {
            TuiTheme.ShowWarning("No valid advancements found across editable addon datapacks.");
            TuiTheme.WaitForKey();
            return Task.CompletedTask;
        }

        TuiTheme.RenderHeader(Title);

        var confirm = AnsiConsole.Confirm(
            $"Are you sure you want to format and overwrite [green]{targetAdvancements.Count}[/] advancement file(s)?");
        if (!confirm)
            return Task.CompletedTask;

        TuiTheme.Space();

        TuiTheme.RunProgress(
            "Formatting JSON and function files...",
            targetAdvancements,
            advancement => AdvancementIoManager.SaveAdvancement(advancement)
        );

        TuiTheme.Space();
        TuiTheme.ShowSuccess($"Successfully formatted {targetAdvancements.Count} advancement(s)!");
        TuiTheme.WaitForKey();

        return Task.CompletedTask;
    }
}