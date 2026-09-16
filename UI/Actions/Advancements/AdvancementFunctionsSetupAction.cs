using BacapGenerator.Advancements.Models;
using BacapGenerator.Datapacks;
using BacapGenerator.Datapacks.Models.Settings;
using BacapGenerator.Io;
using Spectre.Console;
using UI.Actions.Advancements.Functions;
using UI.Interfaces;
using UI.Styling;

namespace UI.Actions.Advancements;

/// <summary>
/// Scans editable BACAP advancements lacking mandatory reward function files and guides the user
/// sequentially through Experience, Item, and Trophy setup wizards for whichever files are strictly required,
/// adhering to compatibility addon override configurations.
/// </summary>
/// <param name="registry">The registry containing loaded datapack instances.</param>
public class AdvancementFunctionsSetupAction(DatapackRegistry registry) : IManageAdvancementsAction
{
    public string Title => "Reward Setup (Missing Rewards)";

    /// <summary>
    /// Executes the batch reward configuration pipeline over all eligible advancements with missing required reward files.
    /// </summary>
    /// <returns>A completed <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task ExecuteAsync()
    {
        // 1. Collect only advancements where required reward files are physically missing on disk
        var targetAdvancements = registry.Values
            .Where(dp => dp.Settings.IsRewardModifiableAddon())
            .SelectMany(dp => dp.Advancements.OfType<BacapAdvancement>())
            .Where(adv => adv.Datapack.Settings.HasAnyMissingReward(adv))
            .ToList();

        if (targetAdvancements.Count == 0)
        {
            TuiTheme.ShowWarning("No editable BACAP advancements with missing required reward files were found.");
            TuiTheme.WaitForKey();
            return;
        }

        const string configureChoice = "Configure missing rewards";
        const string skipChoice = "Skip this advancement";
        const string abortChoice = "Stop and exit wizard";

        for (var i = 0; i < targetAdvancements.Count; i++)
        {
            var advancement = targetAdvancements[i];
            var settings = advancement.Datapack.Settings;
            var stepNumber = i + 1;
            var totalSteps = targetAdvancements.Count;

            // Invalidate cached disk information before inspecting requirements
            advancement.ExpRewardFunction.File.Refresh();
            advancement.ItemRewardFunction.File.Refresh();
            advancement.TrophyRewardFunction.File.Refresh();

            // Strictly check whether this specific advancement requires the file
            var missingExp = settings.RequiresExpReward(advancement) && !advancement.ExpRewardFunction.File.Exists;
            var missingItems = settings.RequiresItemReward(advancement) && !advancement.ItemRewardFunction.File.Exists;
            var missingTrophies = settings.RequiresTrophyReward(advancement) && !advancement.TrophyRewardFunction.File.Exists;

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

            // Experience reward wizard
            if (missingExp)
            {
                await new ChangeExpAction(advancement).ExecuteAsync();
                AdvancementIoManager.SaveAdvancement(advancement);
            }

            // Item reward wizard
            if (missingItems)
            {
                RenderAdvancementSummary(advancement, settings, stepNumber, totalSteps);
                await new ChangeItemAction(advancement).ExecuteAsync();
                AdvancementIoManager.SaveAdvancement(advancement);
            }

            // Trophy reward wizard
            if (!missingTrophies)
                continue;

            RenderAdvancementSummary(advancement, settings, stepNumber, totalSteps);
            await new ChangeTrophyAction(advancement).ExecuteAsync();
            AdvancementIoManager.SaveAdvancement(advancement);
        }

        TuiTheme.ShowSuccess("All selected advancements have been processed successfully.");
        TuiTheme.WaitForKey();
    }

    /// <summary>
    /// Flushes residual input buffer and renders an informative summary card for the active advancement.
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
            Console.ReadKey(intercept: true);

        advancement.ExpRewardFunction.File.Refresh();
        advancement.ItemRewardFunction.File.Refresh();
        advancement.TrophyRewardFunction.File.Refresh();

        var expStatus = FormatRewardStatus(
            isSupported: settings.SupportsExpRewards(),
            isRequired: settings.RequiresExpReward(advancement),
            isOverride: advancement.IsOverride,
            exists: advancement.ExpRewardFunction.File.Exists);

        var itemStatus = FormatRewardStatus(
            isSupported: settings.SupportsItemRewards(),
            isRequired: settings.RequiresItemReward(advancement),
            isOverride: advancement.IsOverride,
            exists: advancement.ItemRewardFunction.File.Exists);

        var trophyStatus = FormatRewardStatus(
            isSupported: settings.SupportsTrophyRewards(),
            isRequired: settings.RequiresTrophyReward(advancement),
            isOverride: advancement.IsOverride,
            exists: advancement.TrophyRewardFunction.File.Exists);

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
    /// Formats the UI status label accurately distinguishing between configured, inherited, missing, and disabled states.
    /// </summary>
    /// <param name="isSupported">Whether the reward category is enabled in the datapack settings.</param>
    /// <param name="isRequired">Whether this advancement strictly mandates a local reward function.</param>
    /// <param name="isOverride">Whether this advancement overrides a parent datapack definition.</param>
    /// <param name="exists">Whether the function file currently exists on disk.</param>
    /// <returns>A formatted markup string representing the operational status.</returns>
    private static string FormatRewardStatus(bool isSupported, bool isRequired, bool isOverride, bool exists)
    {
        if (!isSupported)
            return "[Gray70]Disabled[/]";

        if (exists)
            return isOverride ? "[green]Overridden[/]" : "[green]Configured[/]";

        if (isOverride && !isRequired)
            return "[grey]Inherited (Parent)[/]";

        return "[red]Missing[/]";
    }
}