using BacapGenerator.Models.Advancements;
using BacapGenerator.Models.Datapacks;
using BacapGenerator.Models.Datapacks.Settings;
using BacapGenerator.Services.IO;
using Spectre.Console;
using UI.Interfaces;
using UI.Styling;

namespace UI.Actions.Advancements;

/// <summary>
/// Action that formats and rewrites advancements and reward functions across editable addon datapacks.
/// Standardizes JSON indentation and function formatting on disk.
/// </summary>
/// <param name="registry">The registry containing loaded datapack instances.</param>
public class AdvancementsFormatAction(DatapackRegistry registry) : IManageAdvancementsAction
{
    public string Title => "Format All Advancements";

    /// <summary>
    /// Executes the batch formatting process across all eligible Addon and CompatibilityAddon datapacks.
    /// </summary>
    /// <returns>A completed <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task ExecuteAsync()
    {
        var targetAdvancements = registry.Values
            .Where(dp => dp.Settings.IsRewardModifiableAddon())
            .SelectMany(dp => dp.Advancements.OfType<BacapAdvancement>().Select(adv => (Datapack: dp, Advancement: adv)))
            .Where(item => item.Datapack.Settings.Type == DatapackType.Addon
                           || item.Datapack.Settings.HasAnyExistingReward(item.Advancement))
            .ToList();

        if (targetAdvancements.Count == 0)
        {
            TuiTheme.ShowWarning("No advancements found across datapacks with editable Addon access.");
            TuiTheme.WaitForKey();
            return Task.CompletedTask;
        }

        TuiTheme.RenderHeader(Title);

        var confirm = AnsiConsole.Confirm(
            $"Are you sure you want to format and overwrite [green]{targetAdvancements.Count}[/] advancement files?");
        if (!confirm)
            return Task.CompletedTask;

        TuiTheme.Space();

        TuiTheme.RunProgress(
            "Formatting JSON and function files...",
            targetAdvancements,
            entry => AdvancementIoManager.SaveAdvancement(entry.Advancement)
        );

        TuiTheme.Space();
        TuiTheme.ShowSuccess($"Successfully formatted {targetAdvancements.Count} advancements!");
        TuiTheme.WaitForKey();

        return Task.CompletedTask;
    }
}