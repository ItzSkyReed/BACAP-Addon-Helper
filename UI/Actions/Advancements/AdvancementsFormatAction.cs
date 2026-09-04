using BacapGenerator.Models.Advancements;
using BacapGenerator.Models.Datapacks;
using BacapGenerator.Models.Datapacks.Settings;
using BacapGenerator.Services.IO;
using Spectre.Console;
using UI.Interfaces;
using UI.Styling;

namespace UI.Actions.Advancements;

/// <summary>
/// Action that formats and rewrites all advancements in ReadWrite datapacks.
/// Standardizes JSON indentation and function formatting on the disk.
/// </summary>
public class AdvancementsFormatAction(
    DatapackRegistry registry) : IManageAdvancementsAction
{
    public string Title => "Format All Advancements";

    public Task ExecuteAsync()
    {
        var advancements = registry.Values
            .Where(dp => dp.Settings.Access == DatapackAccess.ReadWrite)
            .SelectMany(dp => dp.Advancements.OfType<BacapAdvancement>())
            .ToList();

        if (advancements.Count == 0)
        {
            TuiTheme.ShowWarning("No advancements found across datapacks with ReadWrite access.");
            TuiTheme.WaitForKey();
            return Task.CompletedTask;
        }

        TuiTheme.RenderHeader(Title);


        var confirm = AnsiConsole.Confirm($"Are you sure you want to format and overwrite [green]{advancements.Count}[/] advancement files?");
        if (!confirm)
            return Task.CompletedTask;

        TuiTheme.Space();

        TuiTheme.RunProgress(
            "Formatting JSON and function files...",
            advancements,
            AdvancementIoManager.SaveAdvancement
        );

        TuiTheme.Space();
        TuiTheme.ShowSuccess($"Successfully formatted {advancements.Count} advancements!");
        TuiTheme.WaitForKey();

        return Task.CompletedTask;
    }
}