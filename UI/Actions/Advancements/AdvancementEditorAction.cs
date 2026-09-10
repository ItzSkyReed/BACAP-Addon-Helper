using BacapGenerator.Models.Advancements;
using BacapGenerator.Models.Datapacks;
using BacapGenerator.Models.Datapacks.Settings;
using BacapGenerator.Services.IO;
using Spectre.Console;
using UI.Actions.Advancements.Functions;
using UI.Actions.Common;
using UI.Interfaces;
using UI.Styling;

namespace UI.Actions.Advancements;

/// <summary>
/// Provides an interactive TUI management hub for BACAP advancements, allowing modification
/// of core properties (Title, Description, Tab, Parent, Tier) as well as attached reward functions.
/// </summary>
/// <param name="registry">The registry containing loaded datapacks.</param>
public class AdvancementEditorAction(DatapackRegistry registry) : IManageAdvancementsAction
{
    private static readonly IReadOnlyList<BacapAdvancementTier> EditableTiers =
        [..Enum.GetValues<BacapAdvancementTier>().Where(t => t != BacapAdvancementTier.Root)];

    public string Title => "Advancement Editor";

    /// <summary>
    /// Represents configuration categories available within the advancement editor hub.
    /// </summary>
    private enum AdvancementEditOption
    {
        ChangeTitle,
        ChangeDescription,
        ChangeTab,
        ChangeParent,
        ChangeTier,
        ManageExperience,
        ManageItems,
        ManageTrophies,
        Delete,
        Back
    }

    /// <summary>
    /// Executes the search loop to select an editable advancement and opens its configuration hub.
    /// </summary>
    /// <returns>A completed <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task ExecuteAsync()
    {
        var editableAdvancements = registry.Values
            .Where(dp => dp.Settings.Type != DatapackType.Reference)
            .SelectMany(dp => dp.Advancements.OfType<BacapAdvancement>())
            .ToList();

        if (editableAdvancements.Count == 0)
        {
            TuiTheme.ShowWarning("No editable BACAP advancements found across addon datapacks.");
            TuiTheme.WaitForKey();
            return;
        }

        // Only valid advancements can act as parents; broken files (InvalidAdvancement) are discarded
        var allValidAdvancements = registry.Values
            .SelectMany(dp => dp.Advancements.OfType<ValidAdvancement>())
            .ToList();

        while (true)
        {
            var selectedAdv = await AdvancementSearcher.PromptSearch(editableAdvancements);
            if (selectedAdv is null)
                break;

            var wasDeleted = await OpenAdvancementEditorAsync(selectedAdv, allValidAdvancements);
            if (!wasDeleted)
                continue;

            editableAdvancements.Remove(selectedAdv);
            allValidAdvancements.Remove(selectedAdv);

            if (editableAdvancements.Count == 0)
                break;
        }
    }

    /// <summary>
    /// Displays the comprehensive editing menu for an individual advancement.
    /// </summary>
    /// <param name="advancement">The target BACAP advancement being configured.</param>
    /// <param name="allValidAdvancements">The global list of known valid advancements for parent validation.</param>
    /// <returns>
    /// A task containing <see langword="true"/> if the advancement was deleted during the session;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    private static async Task<bool> OpenAdvancementEditorAsync(
        BacapAdvancement advancement,
        IReadOnlyList<ValidAdvancement> allValidAdvancements)
    {
        while (true)
        {
            TuiTheme.RenderHeader($"Edit Advancement: {advancement.TitleText} [{advancement.Datapack.Id}]");

            var descPreview = Truncate(advancement.DescriptionText, 80);
            var choices = BuildAvailableOptions(advancement.Datapack.Settings);

            var option = AnsiConsole.Prompt(
                new SelectionPrompt<AdvancementEditOption>()
                    .Title("Choose an advancement property or function to configure:")
                    .AddChoices(choices)
                    .UseConverter(opt => opt switch
                    {
                        AdvancementEditOption.ChangeTitle =>
                            $"Title [grey]({Markup.Escape(advancement.TitleText)})[/]",

                        AdvancementEditOption.ChangeDescription =>
                            $"Description [grey]({Markup.Escape(descPreview)})[/]",

                        AdvancementEditOption.ChangeTab =>
                            $"Tab [{advancement.Tab.Color}]■[/] [white]{advancement.Tab.DisplayName}[/] [grey]({advancement.Tab.FolderName})[/]",

                        AdvancementEditOption.ChangeParent =>
                            $"Parent [grey]({Markup.Escape(advancement.Parent ?? "None")})[/]",

                        AdvancementEditOption.ChangeTier =>
                            $"Tier [yellow]{advancement.Tier}[/]",

                        AdvancementEditOption.ManageExperience =>
                            $"Experience Reward [grey]—[/] {FormatExpSummary(advancement)}",

                        AdvancementEditOption.ManageItems =>
                            $"Item Rewards [grey]—[/] {FormatItemSummary(advancement)}",

                        AdvancementEditOption.ManageTrophies =>
                            $"Trophy Rewards [grey]—[/] {FormatTrophySummary(advancement)}",

                        AdvancementEditOption.Delete =>
                            "[red]Delete Advancement[/]",

                        AdvancementEditOption.Back =>
                            TuiTheme.BackOptionString,

                        _ => opt.ToString()
                    }));

            switch (option)
            {
                case AdvancementEditOption.ChangeTitle:
                    EditTitle(advancement);
                    break;

                case AdvancementEditOption.ChangeDescription:
                    EditDescription(advancement);
                    break;

                case AdvancementEditOption.ChangeTab:
                    EditTab(advancement);
                    break;

                case AdvancementEditOption.ChangeParent:
                    await EditParent(advancement, allValidAdvancements);
                    break;

                case AdvancementEditOption.ChangeTier:
                    EditTier(advancement);
                    break;

                case AdvancementEditOption.ManageExperience:
                    await new ChangeExpAction(advancement).ExecuteAsync();
                    AdvancementIoManager.SaveAdvancement(advancement);
                    break;

                case AdvancementEditOption.ManageItems:
                    await new ChangeItemAction(advancement).ExecuteAsync();
                    AdvancementIoManager.SaveAdvancement(advancement);
                    break;

                case AdvancementEditOption.ManageTrophies:
                    await new ChangeTrophyAction(advancement).ExecuteAsync();
                    AdvancementIoManager.SaveAdvancement(advancement);
                    break;

                case AdvancementEditOption.Delete:
                    if (TryDeleteAdvancement(advancement))
                        return true;
                    break;

                case AdvancementEditOption.Back:
                    return false;
            }
        }
    }

    /// <summary>
    /// Constructs the list of permitted menu choices according to datapack capabilities.
    /// </summary>
    /// <param name="settings">The parent datapack settings.</param>
    /// <returns>A list of eligible <see cref="AdvancementEditOption"/> entries.</returns>
    private static List<AdvancementEditOption> BuildAvailableOptions(DatapackSettings settings)
    {
        var options = new List<AdvancementEditOption>(10)
        {
            AdvancementEditOption.ChangeTitle,
            AdvancementEditOption.ChangeDescription,
            AdvancementEditOption.ChangeTab,
            AdvancementEditOption.ChangeParent,
            AdvancementEditOption.ChangeTier
        };

        if (settings.SupportsExpRewards())
            options.Add(AdvancementEditOption.ManageExperience);

        if (settings.SupportsItemRewards())
            options.Add(AdvancementEditOption.ManageItems);

        if (settings.SupportsTrophyRewards())
            options.Add(AdvancementEditOption.ManageTrophies);

        options.Add(AdvancementEditOption.Delete);
        options.Add(AdvancementEditOption.Back);

        return options;
    }

    /// <summary>
    /// Prompts the user to update the display title and saves changes.
    /// </summary>
    /// <param name="advancement">The target advancement to update.</param>
    private static void EditTitle(BacapAdvancement advancement)
    {
        var newTitle = PromptRequiredText(
            prompt: "Enter advancement title:",
            defaultValue: advancement.TitleText,
            errorMessage: "[red]Title cannot be empty.[/]");

        TryApplyMutation(
            advancement,
            () => advancement.TitleText = newTitle,
            $"Title updated to '{Markup.Escape(newTitle)}'.");
    }

    /// <summary>
    /// Prompts the user to update the description text and saves changes.
    /// </summary>
    /// <param name="advancement">The target advancement to update.</param>
    private static void EditDescription(BacapAdvancement advancement)
    {
        var newDesc = PromptRequiredText(
            prompt: "Enter advancement description:",
            defaultValue: advancement.DescriptionText,
            errorMessage: "[red]Description cannot be empty.[/]");

        TryApplyMutation(
            advancement,
            () => advancement.DescriptionText = newDesc,
            "Description updated.");
    }

    /// <summary>
    /// Prompts the user to select an advancement tab from predefined registry tabs.
    /// </summary>
    /// <param name="advancement">The target advancement to update.</param>
    private static void EditTab(BacapAdvancement advancement)
    {
        var newTab = AnsiConsole.Prompt(
            new SelectionPrompt<BacapAdvancementTab>()
                .Title("Select advancement tab:")
                .EnableSearch()
                .AddChoices(BacapAdvancementTab.All)
                .UseConverter(tab => $"[{tab.Color}]■[/] [white]{tab.DisplayName}[/] [grey]({tab.FolderName})[/]")
        );

        TryApplyMutation(
            advancement,
            () => advancement.Tab = newTab,
            $"Tab changed to '{newTab.DisplayName}'.");
    }

    /// <summary>
    /// Guides the user through choosing a new parent advancement via interactive search or direct McPath entry.
    /// </summary>
    /// <param name="advancement">The target advancement whose parent is being updated.</param>
    /// <param name="allValidAdvancements">The global list of known valid advancements for reference and validation.</param>
    /// <returns>A completed <see cref="Task"/> representing the asynchronous operation.</returns>
    private static async Task EditParent(
        BacapAdvancement advancement,
        IReadOnlyList<ValidAdvancement> allValidAdvancements)
    {
        TuiTheme.RenderHeader($"Change Parent: {advancement.TitleText}");
        AnsiConsole.MarkupLine($"Current parent: [yellow]{Markup.Escape(advancement.Parent ?? "None")}[/]\n");

        const string searchChoice = "Search advancement via picker";
        const string manualChoice = "Enter McPath manually";

        var method = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("How do you want to select the parent?")
                .AddChoices(searchChoice, manualChoice, TuiTheme.BackOptionString));

        switch (method)
        {
            case TuiTheme.BackOptionString:
                return;

            case searchChoice:
            {
                // Can be any valid advancement (including technical root advancements like 'minecraft:adventure/root')
                var candidate = await AdvancementSearcher.PromptSearch(allValidAdvancements);
                if (candidate is null)
                    return;

                if (candidate.McPath.Equals(advancement.McPath, StringComparison.OrdinalIgnoreCase))
                {
                    TuiTheme.ShowWarning("An advancement cannot be its own parent.");
                    TuiTheme.WaitForKey();
                    return;
                }

                TryApplyMutation(
                    advancement,
                    () => advancement.Parent = candidate.McPath,
                    $"Parent set to '{candidate.McPath}'.");
                return;
            }

            case manualChoice:
            {
                var newParent = AnsiConsole.Prompt(
                    new TextPrompt<string>("Enter parent McPath (e.g. 'minecraft:adventure/root'):")
                        .DefaultValue(advancement.Parent ?? string.Empty)
                        .Validate(input =>
                        {
                            var trimmed = input.Trim();
                            if (trimmed.Equals(advancement.McPath, StringComparison.OrdinalIgnoreCase))
                                return ValidationResult.Error("[red]An advancement cannot be its own parent.[/]");

                            return allValidAdvancements.Any(a => a.McPath.Equals(trimmed, StringComparison.OrdinalIgnoreCase))
                                ? ValidationResult.Success()
                                : ValidationResult.Error($"[red]Valid advancement with McPath '{Markup.Escape(trimmed)}' was not found.[/]");
                        })
                ).Trim();

                TryApplyMutation(
                    advancement,
                    () => advancement.Parent = newParent,
                    $"Parent set to '{newParent}'.");
                return;
            }
        }
    }

    /// <summary>
    /// Prompts the user to select an advancement tier and saves changes.
    /// </summary>
    /// <param name="advancement">The target advancement to update.</param>
    private static void EditTier(BacapAdvancement advancement)
    {
        var newTier = AnsiConsole.Prompt(
            new SelectionPrompt<BacapAdvancementTier>()
                .Title("Select new advancement tier:")
                .AddChoices(EditableTiers)
                .UseConverter(t => $"[yellow]{t}[/]")
        );

        TryApplyMutation(
            advancement,
            () => advancement.Tier = newTier,
            $"Tier changed to '{newTier}'.");
    }

    /// <summary>
    /// Executes a mutation action on the advancement, persists changes, and displays feedback.
    /// Catches and presents domain validation errors without crashing the TUI.
    /// </summary>
    /// <param name="advancement">The target advancement instance.</param>
    /// <param name="mutation">The mutation delegate to execute.</param>
    /// <param name="successMessage">The text displayed upon successful persistence.</param>
    /// <returns><see langword="true"/> if the mutation was successfully applied; otherwise, <see langword="false"/>.</returns>
    private static bool TryApplyMutation(BacapAdvancement advancement, Action mutation, string successMessage)
    {
        try
        {
            mutation();
            AdvancementIoManager.SaveAdvancement(advancement);
            TuiTheme.ShowSuccess(successMessage);
            TuiTheme.WaitForKey();
            return true;
        }
        catch (Exception ex)
        {
            TuiTheme.ShowError($"Failed to update advancement: {ex.Message}");
            TuiTheme.WaitForKey();
            return false;
        }
    }

    /// <summary>
    /// Requests confirmation and permanently deletes the specified advancement from storage and memory.
    /// </summary>
    /// <param name="advancement">The advancement to delete.</param>
    /// <returns><see langword="true"/> if deletion was confirmed and completed; otherwise, <see langword="false"/>.</returns>
    private static bool TryDeleteAdvancement(BacapAdvancement advancement)
    {
        var confirmed = AnsiConsole.Confirm(
            $"Are you sure you want to delete [red]{Markup.Escape(advancement.TitleText)}[/]?",
            defaultValue: false);

        if (!confirmed)
            return false;

        try
        {
            AdvancementIoManager.DeleteAdvancement(advancement);

            // Synchronize parent datapack's in-memory collection
            if (advancement.Datapack is Datapack mutableDatapack)
            {
                mutableDatapack.Advancements.Remove(advancement);
            }

            TuiTheme.ShowSuccess("Advancement deleted successfully.");
            TuiTheme.WaitForKey();
            return true;
        }
        catch (Exception ex)
        {
            TuiTheme.ShowError($"Failed to delete advancement: {ex.Message}");
            TuiTheme.WaitForKey();
            return false;
        }
    }

    /// <summary>
    /// Prompts the user for a non-whitespace string, requiring re-entry upon validation failure.
    /// </summary>
    /// <param name="prompt">The prompt label displayed above the input field.</param>
    /// <param name="defaultValue">The fallback default value.</param>
    /// <param name="errorMessage">The markup-formatted validation error displayed when input is empty.</param>
    /// <returns>A trimmed, non-empty string provided by the user.</returns>
    private static string PromptRequiredText(string prompt, string defaultValue, string errorMessage) =>
        AnsiConsole.Prompt(
            new TextPrompt<string>(prompt)
                .DefaultValue(defaultValue)
                .Validate(input => !string.IsNullOrWhiteSpace(input)
                    ? ValidationResult.Success()
                    : ValidationResult.Error(errorMessage))
        ).Trim();

    private static string FormatExpSummary(BacapAdvancement advancement)
    {
        // Если это оверрайд и локальный файл награды пока не создан пользователем
        if (advancement is { IsOverride: true, ExpRewardFunction.File.Exists: false })
            return "[grey]Inherited from parent[/]";

        var exp = advancement.ExpRewardFunction.ExperienceAmount;
        return exp > 0 ? $"[green]{exp} points[/]" : "[grey]None (Empty)[/]";
    }

    /// <summary>
    /// Formats the display line for the item reward function status.
    /// </summary>
    /// <param name="advancement">The advancement to inspect.</param>
    /// <returns>A Spectre.Console markup string summarizing reward item count.</returns>
    private static string FormatItemSummary(BacapAdvancement advancement)
    {
        var count = advancement.ItemRewardFunction.RewardItems.Count;
        return count > 0 ? $"[green]{count} item(s)[/]" : "[grey]None (Empty)[/]";
    }

    /// <summary>
    /// Formats the display line for the trophy reward function status.
    /// </summary>
    /// <param name="advancement">The advancement to inspect.</param>
    /// <returns>A Spectre.Console markup string summarizing trophy count.</returns>
    private static string FormatTrophySummary(BacapAdvancement advancement)
    {
        var count = advancement.TrophyRewardFunction.Trophies.Count;
        return count > 0 ? $"[green]{count} trophy(ies)[/]" : "[grey]None (Empty)[/]";
    }

    /// <summary>
    /// Truncates a string to a single line up to a specified maximum length.
    /// </summary>
    /// <param name="value">The source string to truncate.</param>
    /// <param name="maxLength">The maximum character length allowed.</param>
    /// <returns>The normalized and conditionally truncated string ending with an ellipsis.</returns>
    private static string Truncate(string value, int maxLength)
    {
        var normalized = value.Replace("\r", string.Empty).Replace('\n', ' ');
        return normalized.Length <= maxLength ? normalized : $"{normalized[..maxLength]}...";
    }
}