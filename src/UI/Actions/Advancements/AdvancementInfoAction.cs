using BacapGenerator.Advancements.Models;
using BacapGenerator.Datapacks;
using BacapGenerator.Utils;
using Core.Registries;
using Spectre.Console;
using UI.Actions.Common;
using UI.Interfaces;
using UI.Styling;

namespace UI.Actions.Advancements;

/// <summary>
/// Provides an interactive search interface to inspect detailed metadata, display elements,
/// and reward configurations of any valid advancement (both playable BACAP and technical triggers).
/// </summary>
/// <param name="registry">The registry containing loaded datapacks.</param>
/// <param name="minecraftData">The global registry for resolving item display names.</param>
public class AdvancementInfoAction(DatapackRegistry registry, MinecraftData minecraftData) : IManageAdvancementsAction
{
    public string Title => "Advancement Info";

    /// <summary>
    /// Executes the interactive search loop over all valid advancements and opens the inspection view.
    /// </summary>
    /// <returns>A completed <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task ExecuteAsync()
    {
        var allValidAdvancements = registry.Values
            .SelectMany(dp => dp.Advancements.OfType<ValidAdvancement>())
            .ToList();

        if (allValidAdvancements.Count == 0)
        {
            TuiTheme.ShowWarning("No valid advancements found across loaded datapacks.");
            TuiTheme.WaitForKey();
            return;
        }

        while (true)
        {
            var selectedAdv = await AdvancementSearcher.PromptSearch(
                allValidAdvancements,
                headerTitle: "Advancement Inspector");

            if (selectedAdv is null)
                break;

            ShowAdvancementInfo(selectedAdv);
        }
    }

    /// <summary>
    /// Renders a detailed property table for the specified advancement, dynamically tailoring
    /// rows depending on whether the instance is a playable BACAP advancement or a technical trigger.
    /// </summary>
    /// <param name="adv">The valid advancement to inspect.</param>
    private void ShowAdvancementInfo(ValidAdvancement adv)
    {
        TuiTheme.RenderHeader($"Advancement Details: {adv.McPath}");

        var table = TuiTheme.CreateTable("Property", "Value");

        // Common metadata across all valid advancements
        table.AddRow($"[{TuiTheme.TablePropertyColor}]Datapack[/]", $"[cyan]{Markup.Escape(adv.Datapack.Id)}[/]");
        table.AddRow($"[{TuiTheme.TablePropertyColor}]McPath (File)[/]", $"[white]{Markup.Escape(adv.McPath)}[/]");
        table.AddRow($"[{TuiTheme.TablePropertyColor}]Parent[/]", $"[grey]{Markup.Escape(adv.Advancement.Parent ?? "None (Root)")}[/]");

        if (adv is BacapAdvancement bacap)
            RenderBacapDetails(table, bacap);
        else
            RenderTechnicalDetails(table, adv);

        TuiTheme.RenderElement(table);
        TuiTheme.WaitForKey();
    }

    /// <summary>
    /// Appends playable BACAP-specific rows (UI elements, tab, tier, and reward functions) to the table.
    /// </summary>
    /// <param name="table">The target Spectre table being constructed.</param>
    /// <param name="bacap">The playable BACAP advancement instance.</param>
    private void RenderBacapDetails(Table table, BacapAdvancement bacap)
    {
        table.AddRow($"[{TuiTheme.TablePropertyColor}]Type[/]", "[green]Playable BACAP Advancement[/]");
        table.AddRow($"[{TuiTheme.TablePropertyColor}]Title[/]", $"[white]{Markup.Escape(bacap.TitleText)}[/]");

        var descText = !string.IsNullOrWhiteSpace(bacap.CleanDescriptionText)
            ? Markup.Escape(bacap.CleanDescriptionText)
            : "[grey]None[/]";
        table.AddRow($"[{TuiTheme.TablePropertyColor}]Description[/]", $"[white]{descText}[/]");

        table.AddRow($"[{TuiTheme.TablePropertyColor}]Tab[/]", $"[{bacap.Tab.Color}]■[/] [{bacap.Tab.Color}]{Markup.Escape(bacap.Tab.DisplayName)}[/] [grey]({bacap.Tab.FolderName})[/]");
        table.AddRow($"[{TuiTheme.TablePropertyColor}]Tier[/]", $"[{bacap.Tier.Color()}]{bacap.Tier.DisplayName()}[/]");

        // Experience reward
        var expAmount = bacap.ExpRewardFunction.ExperienceAmount;
        var expDisplay = expAmount > 0
            ? $"[green]{expAmount} points[/]"
            : "[grey]None[/]";
        table.AddRow($"[{TuiTheme.TablePropertyColor}]Experience Reward[/]", expDisplay);

        // Item rewards
        var items = bacap.ItemRewardFunction.RewardItems;
        var formattedItems = items.Count > 0
            ? string.Join("\n", items.Select(item =>
            {
                var cleanItemId = MinecraftUtils.StripNamespace(item.Id);
                var displayName = ResolveItemDisplayName(cleanItemId);
                return $"[green]{item.Count}x[/] [cyan]{Markup.Escape(displayName)}[/] [grey]({cleanItemId})[/]";
            }))
            : "[grey]None[/]";
        table.AddRow($"[{TuiTheme.TablePropertyColor}]Item Rewards[/]", formattedItems);

        // Trophy rewards
        var trophies = bacap.TrophyRewardFunction.Trophies;
        var formattedTrophies = trophies.Count > 0
            ? string.Join("\n", trophies.Select(trophy =>
            {
                var cleanId = MinecraftUtils.StripNamespace(trophy.Item.Id);
                var fallbackName = ResolveItemDisplayName(cleanId);

                var titleColor = trophy.TitleColor ?? "gold1";
                var titleText = trophy.Title ?? fallbackName;

                var mainLine =
                    $"[bold yellow]{trophy.Item.Count}x[/] " +
                    $"[bold {titleColor}]{Markup.Escape(titleText)}[/] " +
                    $"[Grey84]({Markup.Escape(fallbackName)})[/]";

                var loreLines = trophy.DescriptionLines
                    .Where(line => !string.IsNullOrWhiteSpace(line))
                    .Select(line => $"   [italic {titleColor}]{Markup.Escape(line)}[/]")
                    .ToList();

                return loreLines.Count == 0
                    ? mainLine
                    : $"{mainLine}\n{string.Join("\n", loreLines)}";
            }))
            : "[grey]None[/]";
        table.AddRow($"[{TuiTheme.TablePropertyColor}]Trophies[/]", formattedTrophies);
    }

    /// <summary>
    /// Appends technical metadata rows for trigger advancements lacking UI and reward configurations.
    /// </summary>
    /// <param name="table">The target Spectre table being constructed.</param>
    /// <param name="adv">The technical advancement instance.</param>
    private static void RenderTechnicalDetails(Table table, ValidAdvancement adv)
    {
        table.AddRow($"[{TuiTheme.TablePropertyColor}]Type[/]", "[grey]Technical Trigger (No Display / Rewards)[/]");

        var criteriaCount = adv.Advancement.Criteria.Count;
        var criteriaDisplay = criteriaCount > 0
            ? string.Join(", ", adv.Advancement.Criteria.Keys.Select(k => $"[grey]{Markup.Escape(k)}[/]"))
            : "[grey]None[/]";

        table.AddRow($"[{TuiTheme.TablePropertyColor}]Criteria ({criteriaCount})[/]", criteriaDisplay);
    }

    /// <summary>
    /// Resolves the friendly display name of a Minecraft item from registry data,
    /// falling back to the raw ID if not found.
    /// </summary>
    /// <param name="cleanItemId">The stripped item identifier (without namespace).</param>
    /// <returns>The resolved item display name.</returns>
    private string ResolveItemDisplayName(string cleanItemId)
    {
        return minecraftData.Items.TryGetValue(cleanItemId, out var item)
            ? item.DisplayName
            : cleanItemId;
    }
}