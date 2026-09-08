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
/// Provides an interactive management hub for BACAP advancements, allowing modification
/// of core properties (Title, Description, Tab, Parent, Tier) as well as attached reward functions,
/// dynamically adapting options based on datapack capabilities and override settings.
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
    /// Executes the search loop to select an advancement and opens its configuration hub.
    /// </summary>
    /// <returns>A completed <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task ExecuteAsync()
    {
        var editablePairs = registry.Values
            .Where(dp => dp.Settings.Type != DatapackType.Reference)
            .SelectMany(dp => dp.Advancements.OfType<BacapAdvancement>().Select(adv => (Datapack: dp, Advancement: adv)))
            .ToList();

        if (editablePairs.Count == 0)
        {
            TuiTheme.ShowWarning("No advancements found across editable addon datapacks.");
            TuiTheme.WaitForKey();
            return;
        }

        var editableAdvancements = editablePairs.Select(p => p.Advancement).ToList();
        var datapackLookup = editablePairs.ToDictionary(p => p.Advancement, p => p.Datapack);

        var allKnownAdvancements = registry.Values
            .SelectMany(dp => dp.Advancements)
            .ToList();

        while (true)
        {
            var selectedAdv = await AdvancementSearcher.PromptSearch(editableAdvancements);
            if (selectedAdv is null)
                break;

            var datapack = datapackLookup[selectedAdv];
            var wasDeleted = await OpenAdvancementEditorAsync(selectedAdv, datapack, allKnownAdvancements);
            if (!wasDeleted)
                continue;

            editableAdvancements.Remove(selectedAdv);
            datapackLookup.Remove(selectedAdv);
            allKnownAdvancements.Remove(selectedAdv);

            if (editableAdvancements.Count == 0)
                break;
        }
    }

    /// <summary>
    /// Displays the comprehensive editing menu for an individual advancement, offering only supported operations.
    /// </summary>
    /// <param name="advancement">The target BACAP advancement being configured.</param>
    /// <param name="datapack">The parent datapack owning the advancement.</param>
    /// <param name="allAdvancements">The global list of known advancements across all datapacks for parent validation.</param>
    /// <returns>
    /// A task containing <see langword="true"/> if the advancement was deleted during the session;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    private static async Task<bool> OpenAdvancementEditorAsync(
        BacapAdvancement advancement,
        Datapack datapack,
        IReadOnlyList<ManagedAdvancement> allAdvancements)
    {

        while (true)
        {
            TuiTheme.RenderHeader($"Edit Advancement: {advancement.TitleText} [{datapack.Id}]");

            var descPreview = Truncate(advancement.DescriptionText, 80);
            var choices = BuildAvailableOptions(datapack.Settings);

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
                    await EditParent(advancement, allAdvancements);
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
    /// Prompts the user to update the display title.
    /// </summary>
    /// <param name="advancement">The target advancement to update.</param>
    private static void EditTitle(BacapAdvancement advancement)
    {
        var newTitle = PromptRequiredText(
            prompt: "Enter advancement title:",
            defaultValue: advancement.TitleText,
            errorMessage: "[red]Title cannot be empty.[/]");

        advancement.TitleText = newTitle;
        SaveAndNotify(advancement,  $"Title updated to '{Markup.Escape(newTitle)}'.");
    }

    /// <summary>
    /// Prompts the user to update the description text.
    /// </summary>
    /// <param name="advancement">The target advancement to update.</param>
    private static void EditDescription(BacapAdvancement advancement)
    {
        var newDesc = PromptRequiredText(
            prompt: "Enter advancement description:",
            defaultValue: advancement.DescriptionText,
            errorMessage: "[red]Description cannot be empty.[/]");

        advancement.DescriptionText = newDesc;
        SaveAndNotify(advancement, "Description updated.");
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

        advancement.Tab = newTab;
        SaveAndNotify(advancement, $"Tab changed to '{newTab.DisplayName}'.");
    }

    /// <summary>
    /// Guides the user through choosing a new parent advancement via interactive search or direct McPath entry.
    /// </summary>
    /// <param name="advancement">The target advancement whose parent is being updated.</param>
    /// <param name="allAdvancements">The global list of known advancements for reference and validation.</param>
    /// <returns>A completed <see cref="Task"/> representing the asynchronous operation.</returns>
    private static async Task EditParent(
        BacapAdvancement advancement,
        IReadOnlyList<ManagedAdvancement> allAdvancements)
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
                var bacapAdvancements = allAdvancements.OfType<BacapAdvancement>().ToList();
                var candidate = await AdvancementSearcher.PromptSearch(bacapAdvancements);
                if (candidate is null)
                    return;

                if (candidate.McPath.Equals(advancement.McPath, StringComparison.OrdinalIgnoreCase))
                {
                    TuiTheme.ShowWarning("An advancement cannot be its own parent.");
                    TuiTheme.WaitForKey();
                    return;
                }

                advancement.Parent = candidate.McPath;
                SaveAndNotify(advancement, $"Parent set to '{candidate.McPath}'.");
                return;
            }
        }

        var newParent = AnsiConsole.Prompt(
            new TextPrompt<string>("Enter parent McPath (e.g. 'minecraft:adventure/root'):")
                .DefaultValue(advancement.Parent ?? string.Empty)
                .Validate(input =>
                {
                    var trimmed = input.Trim();
                    if (trimmed.Equals(advancement.McPath, StringComparison.OrdinalIgnoreCase))
                        return ValidationResult.Error("[red]An advancement cannot be its own parent.[/]");

                    return allAdvancements.Any(a => a.McPath.Equals(trimmed, StringComparison.OrdinalIgnoreCase))
                        ? ValidationResult.Success()
                        : ValidationResult.Error($"[red]Advancement with McPath '{Markup.Escape(trimmed)}' was not found.[/]");
                })
        ).Trim();

        advancement.Parent = newParent;
        SaveAndNotify(advancement, $"Parent set to '{newParent}'.");
    }

    /// <summary>
    /// Prompts the user to select an advancement tier (excluding Root).
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

        advancement.Tier = newTier;
        SaveAndNotify(advancement, $"Tier changed to '{newTier}'.");
    }

    /// <summary>
    /// Requests confirmation and permanently deletes the specified advancement from storage.
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

        AdvancementIoManager.DeleteAdvancement(advancement);
        TuiTheme.ShowSuccess("Advancement deleted successfully.");
        TuiTheme.WaitForKey();
        return true;
    }

    /// <summary>
    /// Persists advancement state changes to disk using the provided settings and notifies the user.
    /// </summary>
    /// <param name="advancement">The advancement instance with updated fields.</param>
    /// <param name="successMessage">The text displayed upon successful persistence.</param>
    private static void SaveAndNotify(BacapAdvancement advancement, string successMessage)
    {
        AdvancementIoManager.SaveAdvancement(advancement);
        TuiTheme.ShowSuccess(successMessage);
        TuiTheme.WaitForKey();
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

    /// <summary>
    /// Formats the display line for the experience reward function status.
    /// </summary>
    /// <param name="advancement">The advancement to inspect.</param>
    /// <returns>A Spectre.Console markup string summarizing experience points.</returns>
    private static string FormatExpSummary(BacapAdvancement advancement)
    {
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