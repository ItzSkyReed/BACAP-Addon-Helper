using BacapGenerator.Models.Advancements;
using BacapGenerator.Models.Datapacks;
using BacapGenerator.Models.Datapacks.Settings;
using BacapGenerator.Services;
using BacapGenerator.Services.IO;
using Spectre.Console;
using UI.Interfaces;
using UI.Styling;

namespace UI.Actions.Advancements;

/// <summary>
/// Action that attempts to recover invalid advancements and writes the generated files to disk.
/// </summary>
public class AdvancementsRecoverAction(
    DatapackRegistry registry) : IManageAdvancementsAction
{
    public string Title => "Recover invalid advancements (generates missing reward functions)";

    public Task ExecuteAsync()
    {
        var readWriteDatapacks = registry.All.Values
            .Where(dp => dp.Settings.Access == DatapackAccess.ReadWrite)
            .ToList();

        if (readWriteDatapacks.Count == 0)
        {
            TuiTheme.ShowWarning("No ReadWrite datapacks found.");
            TuiTheme.WaitForKey();
            return Task.CompletedTask;
        }

        TuiTheme.RenderHeader(Title);


        var recoveredAdvancements = new List<BacapAdvancement>();
        foreach (var dp in readWriteDatapacks)
        {
            recoveredAdvancements.AddRange(AdvancementRecoveryService.RecoverAdvancements(dp));
        }

        if (recoveredAdvancements.Count == 0)
        {
            TuiTheme.ShowWarning("No invalid advancements could be recovered.");
            TuiTheme.WaitForKey();
            return Task.CompletedTask;
        }

        var confirm = AnsiConsole.Confirm($"Successfully recovered [green]{recoveredAdvancements.Count}[/] advancements in memory.\nDo you want to write them to disk?");
        if (!confirm)
            return Task.CompletedTask;

        TuiTheme.Space();

        TuiTheme.RunProgress(
            "Writing recovered files to disk...",
            recoveredAdvancements,
            AdvancementIoManager.SaveAdvancement
        );

        TuiTheme.Space();
        TuiTheme.ShowSuccess($"Successfully recovered and saved {recoveredAdvancements.Count} advancements!");
        TuiTheme.WaitForKey();

        return Task.CompletedTask;
    }
}