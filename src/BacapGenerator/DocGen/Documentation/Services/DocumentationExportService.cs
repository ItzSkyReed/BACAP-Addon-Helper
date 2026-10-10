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
/// Service responsible for aggregating datapack advancements and generating the final JSON documentation for export.
/// </summary>
public static class DocumentationExportService
{
    private const string PlayerHeadId = "player_head";

    /// <summary>
    /// Generates documentation JSON files for all active primary addons and their compatibilities.
    /// Iterates strictly over primary addon advancements while attaching alternative descriptions
    /// and requirements from active compatibility addons.
    /// </summary>
    /// <param name="registry">The source datapack registry.</param>
    /// <param name="config">The global document generator configuration containing I/O paths.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="registry"/> or <paramref name="config"/> is null.</exception>
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

            // Pre-index compatibility advancements by McPath and section_name
            var compatAdvancementsMap = BuildCompatAdvancementsMap(compatAddons);

            var exportEntries = new List<AdvancementDocEntry>();

            // Always iterate only over primary addon advancements to prevent duplicate entries
            foreach (var adv in primaryAddon.Advancements
                         .OfType<BacapAdvancement>()
                         .OrderBy(adv => adv.McPath))
            {
                requirementsMap.TryGetValue(adv.McPath, out var advRequirements);

                var altDescriptions = BuildAlternativeDescriptions(adv, compatAdvancementsMap);

                var entry = BuildEntry(adv, advRequirements, altDescriptions, registry);

                exportEntries.Add(entry);
            }

            var outputPath = Path.Combine(config.OutputDirectory!, $"{primaryAddon.Id}.json");

            using var stream = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None);
            JsonSerializer.Serialize(stream, exportEntries, MinecraftDatapackJsonOptions.Documentation);
        }
    }

    /// <summary>
    /// Indexes compatibility addon advancements by their Minecraft path (<see cref="BacapAdvancement.McPath"/>)
    /// and their configured documentation generator section name.
    /// </summary>
    /// <param name="compatAddons">The list of active compatibility addons linked to the primary addon.</param>
    /// <returns>A dictionary mapping each advancement path to a dictionary of section names and their corresponding compatibility advancements.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="compatAddons"/> is null.</exception>
    private static Dictionary<string, Dictionary<string, BacapAdvancement>> BuildCompatAdvancementsMap(
        IReadOnlyList<Datapack> compatAddons)
    {
        ArgumentNullException.ThrowIfNull(compatAddons);

        var map = new Dictionary<string, Dictionary<string, BacapAdvancement>>(StringComparer.OrdinalIgnoreCase);

        foreach (var compatAddon in compatAddons)
        {
            var sectionName = compatAddon.Settings.DocumentGeneratorSettings?.SectionName;
            if (string.IsNullOrWhiteSpace(sectionName))
                continue;

            foreach (var compatAdv in compatAddon.Advancements.OfType<BacapAdvancement>())
            {
                if (!map.TryGetValue(compatAdv.McPath, out var sections))
                {
                    sections = new Dictionary<string, BacapAdvancement>(StringComparer.OrdinalIgnoreCase);
                    map[compatAdv.McPath] = sections;
                }

                sections[sectionName] = compatAdv;
            }
        }

        return map;
    }

    /// <summary>
    /// Collects alternative descriptions from compatibility addons for a given primary advancement.
    /// </summary>
    /// <param name="primaryAdvancement">The primary addon advancement to inspect.</param>
    /// <param name="compatAdvancementsMap">The pre-indexed lookup of compatibility advancements by path and section name.</param>
    /// <returns>A dictionary of alternative descriptions keyed by section name, or <see langword="null"/> if none were found.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="primaryAdvancement"/> or <paramref name="compatAdvancementsMap"/> is null.</exception>
    private static Dictionary<string, string>? BuildAlternativeDescriptions(
        BacapAdvancement primaryAdvancement,
        Dictionary<string, Dictionary<string, BacapAdvancement>> compatAdvancementsMap)
    {
        ArgumentNullException.ThrowIfNull(primaryAdvancement);
        ArgumentNullException.ThrowIfNull(compatAdvancementsMap);

        if (!compatAdvancementsMap.TryGetValue(primaryAdvancement.McPath, out var sections))
            return null;

        Dictionary<string, string>? result = null;

        foreach (var (sectionName, compatAdv) in sections)
        {
            var compatDesc = compatAdv.CleanDescriptionText;
            if (string.IsNullOrWhiteSpace(compatDesc))
                continue;

            // Exclude if description is identical and we only want actual differences
            if (string.Equals(compatDesc, primaryAdvancement.CleanDescriptionText, StringComparison.Ordinal))
                continue;

            result ??= new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            result[sectionName] = compatDesc;
        }

        return result;
    }

    /// <summary>
    /// Maps a managed internal advancement to the public documentation model.
    /// Filters out empty or null requirement sections before serialization.
    /// </summary>
    /// <param name="advancement">The source advancement model.</param>
    /// <param name="requirements">The raw requirements dictionary loaded from YAML, where values can be null.</param>
    /// <param name="alternativeDescriptions">The dictionary containing alternative descriptions from compatibility addons.</param>
    /// <param name="registry">Datapack registry</param>
    /// <returns>A populated <see cref="AdvancementDocEntry"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="advancement"/> is null.</exception>
    private static AdvancementDocEntry BuildEntry(
        BacapAdvancement advancement,
        Dictionary<string, string?>? requirements,
        Dictionary<string, string>? alternativeDescriptions,
        DatapackRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(advancement);

        return new AdvancementDocEntry
        {
            McPath = advancement.McPath,
            Title = advancement.TitleText,
            Frame = advancement.Advancement.Display!.Frame.ToString().ToLowerInvariant(),
            Description = advancement.CleanDescriptionText,
            IconId = MinecraftUtils.EnsureNamespace(advancement.Advancement.Display!.Icon!.Id),
            Tier = advancement.Tier.TechnicalName(),
            Tab = advancement.Tab.FolderName,
            Parent = BuildParent(advancement, registry),
            Requirements = CleanRequirements(requirements),
            Rewards = BuildRewards(advancement),
            AlternativeDescriptions = alternativeDescriptions is { Count: > 0 } ? alternativeDescriptions : null,
            PlayerHeadData =
                MinecraftUtils.StripNamespace(advancement.Advancement.Display!.Icon!.Id)
                    .Equals(PlayerHeadId, StringComparison.InvariantCultureIgnoreCase)
                && advancement.Advancement.Display!.Icon!.Components.TryGet<ProfileComponent>(out var component)
                    ? BuildPlayerHeadData(component)
                    : null
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
            return null;

        return new RewardsDocEntry
        {
            Experince = exp,
            Items = hasItems ? items : null,
            Trophies = hasTrophies ? trophies : null
        };
    }

    /// <summary>
    /// Builds the aggregated documentation model for an advancement's parent.
    /// </summary>
    /// <param name="advancement">The source advancement containing the parent identifier.</param>
    /// <param name="registry">The registry containing all loaded datapacks to search within.</param>
    /// <returns>
    /// A populated <see cref="ParentAdvancementDocEntry"/> instance if <see cref="BacapAdvancement.Parent"/> is defined;
    /// otherwise, <see langword="null"/>.
    /// </returns>
    private static ParentAdvancementDocEntry? BuildParent(BacapAdvancement advancement, DatapackRegistry registry)
    {
        var parentId = advancement.Parent;
        if (parentId is null)
            return null;

        var parentDoc = new ParentAdvancementDocEntry
        {
            McPath = parentId
        };

        // Reference (0) -> Addon (1) -> CompatibilityAddon (2)
        var sortedDatapacks = registry.Values.OrderBy(d => d.Settings.DatapackType);

        foreach (var datapack in sortedDatapacks)
        {
            if (!datapack.TryGetAdvancement(parentId, out var parentManagedAdv) ||
                parentManagedAdv is not BacapAdvancement parent)
                continue;

            parentDoc.Title = parent.TitleText;
            parentDoc.Description = parent.CleanDescriptionText;
            parentDoc.Tab = parent.Tab.FolderName;
            parentDoc.Tier = parent.Tier.TechnicalName();

            var display = parent.Advancement.Display!;

            parentDoc.Frame = display.Frame.ToString().ToLowerInvariant();
            parentDoc.IconId = MinecraftUtils.EnsureNamespace(display.Icon!.Id);

            if (MinecraftUtils.StripNamespace(display.Icon!.Id).Equals(PlayerHeadId, StringComparison.OrdinalIgnoreCase) &&
                display.Icon!.Components.TryGet<ProfileComponent>(out var component))
                parentDoc.PlayerHeadData = BuildPlayerHeadData(component);


            break;
        }

        return parentDoc;
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
                Id = MinecraftUtils.EnsureNamespace(itemStack.Id),
                CustomName = ExtractCustomName(itemStack),
                Enchantments = ExtractEnchantments(itemStack.Id, itemStack.Components),
                PlayerHeadData = MinecraftUtils.StripNamespace(itemStack.Id).Equals(PlayerHeadId, StringComparison.InvariantCultureIgnoreCase)
                                 && itemStack.Components.TryGet<ProfileComponent>(out var component)
                    ? BuildPlayerHeadData(component)
                    : null
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
                Id = MinecraftUtils.EnsureNamespace(trophy.Item.Id),
                Title = trophy.Title,
                TitleColor = trophy.TitleColor,
                Description = string.Join('\n', trophy.DescriptionLines),
                Enchantments = ExtractEnchantments(trophy.Item.Id, trophy.Item.Components),
                Unbreakable = trophy.Item.Components.TryGet<UnbreakableComponent>(out _),
                PlayerHeadData = MinecraftUtils.StripNamespace(trophy.Item.Id).Equals(PlayerHeadId, StringComparison.InvariantCultureIgnoreCase)
                                 && trophy.Item.Components.TryGet<ProfileComponent>(out var component)
                    ? BuildPlayerHeadData(component)
                    : null
            })
            .ToList();
    }

    /// <summary>
    /// Maps an enumerable sequence of trophy definitions to trophy documentation entries.
    /// </summary>
    /// <param name="profileComponent">The profile component of head.</param>
    /// <returns>A list of populated <see cref="TrophyDocEntry"/> instances.</returns>
    private static PlayerHeadDocEntry BuildPlayerHeadData(ProfileComponent profileComponent)
    {
        var textureBase64 = profileComponent.Properties?[0].Value;
        string? textureHash = null;

        if (!string.IsNullOrEmpty(textureBase64))
            textureHash = MinecraftUtils.ExtractTextureHash(textureBase64);

        return new PlayerHeadDocEntry
        {
            Uuid = profileComponent.Id,
            TextureHash = textureHash
        };
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