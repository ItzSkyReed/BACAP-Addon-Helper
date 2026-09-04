using BacapGenerator.Models.Advancements;
using BacapGenerator.Models.Datapacks;
using BacapGenerator.Utils;
using Core.Registries;
using Spectre.Console;
using UI.Actions.Common;
using UI.Interfaces;
using UI.Styling;

namespace UI.Actions.Advancements;

/// <summary>
/// Action that provides a search interface to find specific BACAP advancements by title or path
/// and displays their detailed information.
/// </summary>
public class AdvancementInfoAction(DatapackRegistry registry, MinecraftData minecraftData) : IManageAdvancementsAction
{
    public string Title => "Advancement Info";

    public Task ExecuteAsync()
    {
        var allAdvancements = registry.Values
            .SelectMany(dp => dp.Advancements.OfType<BacapAdvancement>())
            .ToList();

        if (allAdvancements.Count == 0)
        {
            TuiTheme.ShowWarning("No advancements found across loaded datapacks.");
            TuiTheme.WaitForKey();
            return Task.CompletedTask;
        }

        while (true)
        {
            var selectedAdv = AdvancementSearcher.PromptSearch(allAdvancements);

            if (selectedAdv == null)
                break;

            ShowAdvancementInfo(selectedAdv);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Displays detailed information about a specific advancement.
    /// </summary>
    /// <param name="adv">The advancement to display details for.</param>
    private void ShowAdvancementInfo(BacapAdvancement adv)
    {
        TuiTheme.RenderHeader("Advancement Details");

        var table = TuiTheme.CreateTable("Property", "Value");

        table.AddRow($"[{TuiTheme.TablePropertyColor}]Title[/]", $"[white]{Markup.Escape(adv.TitleText)}[/]");
        table.AddRow($"[{TuiTheme.TablePropertyColor}]Description[/]", $"[white]{Markup.Escape(adv.CleanDescriptionText)}[/]");
        table.AddRow($"[{TuiTheme.TablePropertyColor}]Experience Reward[/]",
            $"[white]{Markup.Escape(adv.ExpRewardFunction.ExperienceAmount.ToString())}[/]");

        var items = adv.ItemRewardFunction.RewardItems;

        var formattedItems = items.Count > 0
            ? string.Join("\n",
                items.Select(item =>
                    $"[green]{item.Count}x[/] " +
                    $"[cyan]{Markup.Escape(minecraftData.Items[MinecraftUtils.StripNamespace(item.Id)].DisplayName)}[/]"))
            : "[grey]None[/]";

        var trophies = adv.TrophyRewardFunction.Trophies;

        var formattedTrophies = trophies.Count > 0
            ? string.Join("\n", trophies.Select(trophy =>
            {
                var cleanId = minecraftData.Items[MinecraftUtils.StripNamespace(trophy.Item.Id)].DisplayName;

                var titleColor = trophy.TitleColor ?? "gold1";
                var titleText = trophy.Title ?? cleanId;

                var mainLine =
                    $"[bold yellow]{trophy.Item.Count}x[/] " +
                    $"[bold {titleColor}]{Markup.Escape(titleText)}[/] " +
                    $"[Grey84]({Markup.Escape(cleanId)})[/]";

                var loreLines = trophy.DescriptionLines
                    .Where(line => !string.IsNullOrWhiteSpace(line))
                    .Select(line => $"   [italic {titleColor}]{Markup.Escape(line)}[/]")
                    .ToList();

                return loreLines.Count == 0
                    ? mainLine
                    : $"{mainLine}\n{string.Join("\n", loreLines)}";
            }))
            : "[grey]None[/]";


        table.AddRow($"[{TuiTheme.TablePropertyColor}]Item Rewards[/]", formattedItems);
        table.AddRow($"[{TuiTheme.TablePropertyColor}]Trophies[/]", formattedTrophies);
        table.AddRow($"[{TuiTheme.TablePropertyColor}]McPath (File)[/]", $"[cyan]{Markup.Escape(adv.McPath)}[/]");
        table.AddRow($"[{TuiTheme.TablePropertyColor}]Tier[/]", $"[{adv.Tier.Color()}]{adv.Tier.DisplayName()}[/]");
        table.AddRow($"[{TuiTheme.TablePropertyColor}]Tab[/]", $"[{adv.Tab.Color}]{Markup.Escape(adv.Tab.DisplayName)}[/]");

        TuiTheme.RenderElement(table);
        TuiTheme.WaitForKey();
    }
}