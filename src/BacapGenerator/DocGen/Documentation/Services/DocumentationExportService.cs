using System.Text.Json;
using BacapGenerator.Advancements.Functions.Trophy;
using BacapGenerator.Advancements.Models;
using BacapGenerator.Configuration;
using BacapGenerator.Datapacks;
using BacapGenerator.Datapacks.Models;
using BacapGenerator.Datapacks.Models.Settings;
using BacapGenerator.DocGen.Documentation.Models;
using BacapGenerator.Io;
using BacapGenerator.Utils;
using Core.DataComponents;
using Core.DataComponents.Components;
using Core.Items;
using Core.Serialization;
using Core.TextComponents.Components;
using JetBrains.Annotations;

namespace BacapGenerator.DocGen.Documentation.Services;

/// <summary>
/// Service responsible for aggregating datapack advancements and generating the final JSON documentation for web export.
/// </summary>
public static class DocumentationExportService
{
    /// <summary>
    /// Generates documentation JSON files for all active primary addons and their compatibilities.
    /// </summary>
    /// <param name="registry">The source datapack registry.</param>
    /// <param name="config">The global document generator configuration containing I/O paths.</param>
    /// <exception cref="ArgumentNullException">Thrown when arguments are null.</exception>
    [PublicAPI]
    public static void GenerateExportFiles(DatapackRegistry registry, DocumentGeneratorConfig config)
    {
        ArgumentNullException.ThrowIfNull(registry);
        ArgumentNullException.ThrowIfNull(config);
        config.Validate();

        var primaryAddons = registry.Values
            .Where(dp => dp.Settings is { DatapackType: DatapackType.Addon, DocumentGeneratorSettings.Enabled: true })
            .ToList();

        Directory.CreateDirectory(config.OutputDirectory!);

        foreach (var primaryAddon in primaryAddons)
        {
            var requirementsPath = Path.Combine(config.RequirementsDirectory!, $"{primaryAddon.Id}.yaml");
            var requirementsMap = RequirementsIoManager.ReadRequirements(requirementsPath);

            var compatAddons = registry.Values
                .Where(dp => dp.Settings.DatapackType == DatapackType.CompatibilityAddon
                             && string.Equals(dp.Settings.ParentDatapackId, primaryAddon.Id, StringComparison.OrdinalIgnoreCase)
                             && dp.Settings.DocumentGeneratorSettings?.Enabled == true)
                .ToList();

            var groupDatapacks = new List<Datapack> { primaryAddon };
            groupDatapacks.AddRange(compatAddons);

            var exportEntries = new List<AdvancementDocEntry>();

            foreach (var adv in groupDatapacks.SelectMany(dp => dp.Advancements)
                         .OfType<BacapAdvancement>().OrderBy(adv => adv.McPath))
            {
                requirementsMap.TryGetValue(adv.McPath, out var advRequirements);

                var entry = BuildEntry(adv, advRequirements);

                exportEntries.Add(entry);
            }

            // Writing JSON файл (for example: docs/generated/bacaped.json)
            var outputPath = Path.Combine(config.OutputDirectory!, $"{primaryAddon.Id}.json");

            using var stream = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None);
            JsonSerializer.Serialize(stream, exportEntries, MinecraftDatapackJsonOptions.Documentation);
        }
    }

    /// <summary>
    /// Maps a managed internal advancement to the public documentation model.
    /// Filters out empty or null requirement sections before serialization.
    /// </summary>
    /// <param name="advancement">The source advancement model.</param>
    /// <param name="requirements">The raw requirements dictionary loaded from YAML, where values can be null.</param>
    /// <returns>A populated <see cref="AdvancementDocEntry"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="advancement"/> is null.</exception>
    private static AdvancementDocEntry BuildEntry(BacapAdvancement advancement, Dictionary<string, string?>? requirements)
    {
        ArgumentNullException.ThrowIfNull(advancement);

        return new AdvancementDocEntry
        {
            McPath = advancement.McPath,
            Title = advancement.TitleText,
            Description = advancement.CleanDescriptionText,
            Tier = advancement.Tier.TechnicalName(),
            Tab = advancement.Tab.FolderName,
            Parent = advancement.Parent,
            Requirements = CleanRequirements(requirements),
            Rewards = BuildRewards(advancement)
        };
    }

    /// <summary>
    /// Builds the aggregated documentation model for advancement rewards.
    /// </summary>
    /// <param name="advancement">The source advancement containing reward functions.</param>
    /// <returns>A populated <see cref="RewardsDocEntry"/> instance if any reward is present; otherwise, <see langword="null"/>.</returns>
    private static RewardsDocEntry? BuildRewards(BacapAdvancement advancement)
    {
        var items = BuildItemRewards(advancement.ItemRewardFunction.RewardItems);
        var trophies = BuildTrophies(advancement.TrophyRewardFunction.Trophies);
        var exp = advancement.ExpRewardFunction.ExperienceAmount != 0
            ? advancement.ExpRewardFunction.ExperienceAmount
            : (int?)null;

        var hasItems = items.Count > 0;
        var hasTrophies = trophies.Count > 0;

        if (!exp.HasValue && !hasItems && !hasTrophies)
        {
            return null;
        }

        return new RewardsDocEntry
        {
            Experince = exp,
            Items = hasItems ? items : null,
            Trophies = hasTrophies ? trophies : null
        };
    }

    /// <summary>
    /// Maps an enumerable sequence of raw item stacks to item reward documentation entries.
    /// </summary>
    /// <param name="rewardItems">The sequence of reward items to map.</param>
    /// <returns>A list of populated <see cref="ItemRewardDocEntry"/> instances.</returns>
    private static List<ItemRewardDocEntry> BuildItemRewards(IEnumerable<ItemStack> rewardItems)
    {
        return rewardItems
            .Select(itemStack => new ItemRewardDocEntry
            {
                Count = itemStack.Count,
                Id = itemStack.Id,
                CustomName = ExtractCustomName(itemStack),
                Enchantments = ExtractEnchantments(itemStack.Id, itemStack.Components)
            })
            .ToList();
    }

    /// <summary>
    /// Maps an enumerable sequence of trophy definitions to trophy documentation entries.
    /// </summary>
    /// <param name="trophies">The sequence of trophies to map.</param>
    /// <returns>A list of populated <see cref="TrophyDocEntry"/> instances.</returns>
    private static List<TrophyDocEntry> BuildTrophies(IEnumerable<TrophyReward> trophies)
    {
        return trophies
            .Select(trophy => new TrophyDocEntry
            {
                Count = trophy.Item.Count,
                Id = trophy.Item.Id,
                Title = trophy.Title,
                TitleColor = trophy.TitleColor,
                Description = string.Join('\n', trophy.DescriptionLines),
                Enchantments = ExtractEnchantments(trophy.Item.Id, trophy.Item.Components),
                Unbreakable = trophy.Item.Components.TryGet<UnbreakableComponent>(out _)
            })
            .ToList();
    }

    /// <summary>
    /// Extracts custom display name text from an item's components.
    /// </summary>
    /// <param name="itemStack">The item stack to inspect.</param>
    /// <returns>The localized or plain text name if present; otherwise, <see langword="null"/>.</returns>
    private static string? ExtractCustomName(ItemStack itemStack)
    {
        return itemStack.Components.Get<CustomNameComponent>()?.Value switch
        {
            TranslatableComponent tc => tc.Translate,
            PlainTextComponent ptc => ptc.Text,
            _ => null
        };
    }

    /// <summary>
    /// Extracts regular or stored enchantment levels from item components based on item identifier.
    /// </summary>
    /// <param name="itemId">The identifier of the item.</param>
    /// <param name="components">The component container attached to the item.</param>
    /// <returns>A dictionary containing enchantment IDs and levels, or <see langword="null"/> if not applicable.</returns>
    private static Dictionary<string, int>? ExtractEnchantments(string itemId, DataComponentMap components)
    {
        var isEnchantedBook = MinecraftUtils.StripNamespace(itemId)
            .Equals("enchanted_book", StringComparison.InvariantCultureIgnoreCase);

        return !isEnchantedBook
            ? components.Get<EnchantmentsComponent>()?.Levels
            : components.Get<StoredEnchantmentsComponent>()?.Levels;
    }

    /// <summary>
    /// Sanitizes the raw requirements map by trimming values and discarding null or whitespace entries.
    /// </summary>
    /// <param name="requirements">The raw requirements dictionary loaded from YAML.</param>
    /// <returns>A cleaned dictionary of requirements, or <see langword="null"/> if empty or invalid.</returns>
    private static Dictionary<string, string>? CleanRequirements(Dictionary<string, string?>? requirements)
    {
        if (requirements is not { Count: > 0 })
            return null;

        var cleanRequirements = requirements
            .Where(kvp => !string.IsNullOrWhiteSpace(kvp.Value))
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value!.Trim());

        return cleanRequirements.Count > 0
            ? cleanRequirements
            : null;
    }
}