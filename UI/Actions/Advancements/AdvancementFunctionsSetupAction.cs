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
/// sequentially through Experience, Item, and Trophy setup wizards for whichever files are missing.
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
            .Where(dp => dp.Settings.Access == DatapackAccess.ReadWrite)
            .SelectMany(dp => dp.Advancements.OfType<BacapAdvancement>())
            .Where(IsMissingRewards)
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
            var advancement = targetAdvancements[i];
            var stepNumber = i + 1;
            var totalSteps = targetAdvancements.Count;

            var missingExp = !advancement.ExpRewardFunction.File.Exists;
            var missingItems = !advancement.ItemRewardFunction.File.Exists;
            var missingTrophies = !advancement.TrophyRewardFunction.File.Exists;

            RenderAdvancementSummary(advancement, stepNumber, totalSteps);

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
                RenderAdvancementSummary(advancement, stepNumber, totalSteps);
                await new ChangeExpAction(advancement).ExecuteAsync();
            }

            // Step 2: Item reward
            if (missingItems)
            {
                RenderAdvancementSummary(advancement, stepNumber, totalSteps);
                await new ChangeItemAction(advancement).ExecuteAsync();
            }

            // Step 3: Trophy reward
            if (missingTrophies)
            {
                RenderAdvancementSummary(advancement, stepNumber, totalSteps);
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
    /// <param name="stepNumber">Current index in the batch sequence (1-based).</param>
    /// <param name="totalSteps">Total number of advancements in the batch sequence.</param>
    private static void RenderAdvancementSummary(BacapAdvancement advancement, int stepNumber, int totalSteps)
    {
        while (Console.KeyAvailable)
        {
            Console.ReadKey(intercept: true);
        }

        // Invalidate cached FileInfo metadata if underlying types inherit from FileSystemInfo
        advancement.ExpRewardFunction.File.Refresh();
        advancement.ItemRewardFunction.File.Refresh();
        advancement.TrophyRewardFunction.File.Refresh();

        var expStatus = advancement.ExpRewardFunction.File.Exists
            ? "[green]Configured[/]"
            : "[red]Missing[/]";

        var itemStatus = advancement.ItemRewardFunction.File.Exists
            ? "[green]Configured[/]"
            : "[red]Missing[/]";

        var trophyStatus = advancement.TrophyRewardFunction.File.Exists
            ? "[green]Configured[/]"
            : "[red]Missing[/]";

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
    /// Checks whether an advancement is missing at least one reward function file on disk.
    /// </summary>
    /// <param name="advancement">The BACAP advancement to inspect.</param>
    /// <returns>
    /// <see langword="true"/> if the advancement is not a Root advancement and is missing
    /// at least one function file (Experience, Item, or Trophy); otherwise, <see langword="false"/>.
    /// </returns>
    private static bool IsMissingRewards(BacapAdvancement advancement) =>
        advancement.Tier != BacapAdvancementTier.Root
        && (!advancement.ExpRewardFunction.File.Exists
            || !advancement.ItemRewardFunction.File.Exists
            || !advancement.TrophyRewardFunction.File.Exists);
}