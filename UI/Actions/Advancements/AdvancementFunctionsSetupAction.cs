using BacapGenerator.Models.Advancements;
using BacapGenerator.Models.Datapacks;
using BacapGenerator.Models.Datapacks.Settings;
using BacapGenerator.Services.IO;
using Spectre.Console;
using UI.Actions.Advancements.Functions;
using UI.Interfaces;
using UI.Styling;

namespace UI.Actions.Advancements;

/// <summary>
/// Scans editable BACAP advancements lacking reward function files and guides the user
/// sequentially through Experience, Item, and Trophy setup wizards for whichever files are missing,
/// strictly respecting compatibility addon override settings.
/// </summary>
/// <param name="registry">The registry containing loaded datapack instances.</param>
public class AdvancementFunctionsSetupAction(DatapackRegistry registry) : IManageAdvancementsAction
{
    public string Title => "Reward Setup (Missing Rewards)";

    /// <summary>
    /// Executes the batch reward configuration pipeline over all eligible advancements with missing reward files.
    /// </summary>
    /// <returns>A completed <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task ExecuteAsync()
    {
        var targetAdvancements = registry.Values
            .Where(dp => dp.Settings.IsRewardModifiableAddon())
            .SelectMany(dp => dp.Advancements.OfType<BacapAdvancement>().Select(adv => (Datapack: dp, Advancement: adv)))
            .Where(item => item.Datapack.Settings.HasAnyMissingReward(item.Advancement))
            .ToList();

        if (targetAdvancements.Count == 0)
        {
            TuiTheme.ShowWarning("No editable BACAP advancements with missing reward files were found.");
            TuiTheme.WaitForKey();
            return;
        }

        const string configureChoice = "Configure missing rewards";
        const string skipChoice = "Skip this advancement";
        const string abortChoice = "Stop and exit wizard";

        for (var i = 0; i < targetAdvancements.Count; i++)
        {
            var (datapack, advancement) = targetAdvancements[i];
            var settings = datapack.Settings;
            var stepNumber = i + 1;
            var totalSteps = targetAdvancements.Count;

            var missingExp = settings.SupportsExpRewards() && !advancement.ExpRewardFunction.File.Exists;
            var missingItems = settings.SupportsItemRewards() && !advancement.ItemRewardFunction.File.Exists;
            var missingTrophies = settings.SupportsTrophyRewards() && !advancement.TrophyRewardFunction.File.Exists;

            RenderAdvancementSummary(advancement, settings, stepNumber, totalSteps);

            var userChoice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Choose next step:")
                    .AddChoices(configureChoice, skipChoice, abortChoice));

            switch (userChoice)
            {
                case abortChoice:
                    return;

                case skipChoice:
                    continue;
            }

            // Step 1: Experience reward
            if (missingExp)
            {
                RenderAdvancementSummary(advancement, settings, stepNumber, totalSteps);
                await new ChangeExpAction(advancement).ExecuteAsync();
            }

            // Step 2: Item reward
            if (missingItems)
            {
                RenderAdvancementSummary(advancement, settings, stepNumber, totalSteps);
                await new ChangeItemAction(advancement).ExecuteAsync();
            }

            // Step 3: Trophy reward
            if (missingTrophies)
            {
                RenderAdvancementSummary(advancement, settings, stepNumber, totalSteps);
                await new ChangeTrophyAction(advancement).ExecuteAsync();
            }

            AdvancementIoManager.SaveAdvancement(advancement);
        }

        TuiTheme.ShowSuccess("All selected advancements have been processed successfully.");
        TuiTheme.WaitForKey();
    }

    /// <summary>
    /// Clears any residual keypresses and renders the advancement header along with an informative summary card.
    /// </summary>
    /// <param name="advancement">The advancement being displayed.</param>
    /// <param name="settings">The settings of the parent datapack.</param>
    /// <param name="stepNumber">Current index in the batch sequence (1-based).</param>
    /// <param name="totalSteps">Total number of advancements in the batch sequence.</param>
    private static void RenderAdvancementSummary(
        BacapAdvancement advancement,
        DatapackSettings settings,
        int stepNumber,
        int totalSteps)
    {
        while (Console.KeyAvailable)
        {
            Console.ReadKey(intercept: true);
        }

        // Invalidate cached FileInfo metadata
        advancement.ExpRewardFunction.File.Refresh();
        advancement.ItemRewardFunction.File.Refresh();
        advancement.TrophyRewardFunction.File.Refresh();

        var expStatus = FormatStatus(settings.SupportsExpRewards(), advancement.ExpRewardFunction.File.Exists);
        var itemStatus = FormatStatus(settings.SupportsItemRewards(), advancement.ItemRewardFunction.File.Exists);
        var trophyStatus = FormatStatus(settings.SupportsTrophyRewards(), advancement.TrophyRewardFunction.File.Exists);

        TuiTheme.RenderHeader(Markup.Escape($"Reward Setup [{stepNumber}/{totalSteps}]"));

        var infoTable = new Table()
            .Border(TableBorder.None)
            .HideHeaders()
            .AddColumn("Label", c => c.PadRight(2))
            .AddColumn("Value")
            .AddRow($"[{TuiTheme.TablePropertyColor}]Title:[/]", $"[white]{Markup.Escape(advancement.TitleText)}[/]")
            .AddRow($"[{TuiTheme.TablePropertyColor}]Description:[/]", $"[Gray84]{Markup.Escape(advancement.CleanDescriptionText)}[/]")
            .AddRow($"[{TuiTheme.TablePropertyColor}]Tab:[/]", $"[{advancement.Tab.Color}]■[/] [white]{advancement.Tab.DisplayName}[/]")
            .AddRow($"[{TuiTheme.TablePropertyColor}]Tier:[/]", $"[{advancement.Tier.Color()}]{advancement.Tier}[/]")
            .AddRow($"[{TuiTheme.TablePropertyColor}]McPath:[/]", $"[cyan]{Markup.Escape(advancement.McPath)}[/]")
            .AddEmptyRow()
            .AddRow($"[{TuiTheme.TablePropertyColor}]Experience:[/]", expStatus)
            .AddRow($"[{TuiTheme.TablePropertyColor}]Items:[/]", itemStatus)
            .AddRow($"[{TuiTheme.TablePropertyColor}]Trophies:[/]", trophyStatus);

        AnsiConsole.Write(new Panel(infoTable)
            .Header("[yellow]Advancement Summary[/]")
            .BorderColor(Color.Grey));
    }

    /// <summary>
    /// Formats the UI status label according to whether the reward category is active and exists on disk.
    /// </summary>
    /// <param name="isEnabled">Whether the reward category is enabled for this datapack.</param>
    /// <param name="exists">Whether the underlying function file exists.</param>
    /// <returns>A formatted markup string representing the status.</returns>
    private static string FormatStatus(bool isEnabled, bool exists)
    {
        if (!isEnabled)
            return "[Gray70]Disabled[/]";

        return exists ? "[green]Configured[/]" : "[red]Missing[/]";
    }
}