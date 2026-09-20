using BacapGenerator.Advancements.Functions;
using BacapGenerator.Advancements.Models;
using BacapGenerator.Datapacks.Models;
using BacapGenerator.Datapacks.Models.Settings;
using BacapGenerator.Utils;
using Core.Commands.Impl;
using Core.Commands.Models.Interfaces;
using Core.DataComponents.Components;
using Core.Items;
using Core.McFunctions;
using Core.McFunctions.Models;
using Core.SNBT.Interfaces;
using Core.SNBT.Nodes;
using Core.TextComponents;
using Core.TextComponents.Components;
using JetBrains.Annotations;

namespace BacapGenerator.LanguagePacks.Services;

/// <summary>
/// Service responsible for scanning a datapack to extract all declared translation keys across
/// advancements and <c>.mcfunction</c> disk files while filtering ignored and vanilla tokens.
/// </summary>
public static class TranslationKeyDiscoveryService
{
    /// <summary>
    /// Scans a single datapack and returns all unique translation keys in discovery sequence.
    /// </summary>
    /// <param name="datapack">The datapack to inspect.</param>
    /// <returns>An ordered collection of unique translation keys.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="datapack"/> is <see langword="null"/>.</exception>
    /// <example>
    /// <code>
    /// IReadOnlyCollection&lt;string&gt; keys = TranslationKeyDiscoveryService.DiscoverKeys(datapack);
    /// </code>
    /// </example>
    public static IReadOnlyCollection<string> DiscoverKeys(
        Datapack datapack)
    {
        ArgumentNullException.ThrowIfNull(datapack);

        return DiscoverKeys([datapack]);
    }

    /// <summary>
    /// Scans multiple datapacks sequentially and returns all unique translation keys in discovery sequence.
    /// </summary>
    /// <param name="datapacks">The sequence of datapacks to inspect in priority order.</param>
    /// <returns>An ordered collection of unique translation keys.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="datapacks"/> is <see langword="null"/>.</exception>
    /// <example>
    /// <code>
    /// IReadOnlyCollection&lt;string&gt; keys = TranslationKeyDiscoveryService.DiscoverKeys([mainPack, addonPack]);
    /// </code>
    /// </example>
    public static IReadOnlyCollection<string> DiscoverKeys(
        IEnumerable<Datapack> datapacks)
    {
        ArgumentNullException.ThrowIfNull(datapacks);

        var keyMap = new OrderedDictionary<string, bool>(StringComparer.Ordinal);

        foreach (var datapack in datapacks)
        {
            ArgumentNullException.ThrowIfNull(datapack.Settings.LanguagePackSettigs);

            ScanAdvancements(datapack.Advancements.OfType<BacapAdvancement>(), keyMap, datapack.Settings.LanguagePackSettigs);
            ScanDiskFunctions(datapack, keyMap, datapack.Settings.LanguagePackSettigs);
        }

        return keyMap.Keys;
    }

    [PublicAPI]
    private static bool TryRegisterKey(
        string? rawKey,
        OrderedDictionary<string, bool> destination,
        LanguagePackSettings settings)
    {
        if (string.IsNullOrWhiteSpace(rawKey))
            return false;

        if (IsVanillaMinecraftKey(rawKey))
            return false;

        if (!rawKey.ContainsAnyLetter())
            return false;

        return !settings.IgnoredKeysSet.Contains(rawKey) && destination.TryAdd(rawKey, true);
    }

    /// <summary>
    /// Determines whether the specified key matches Mojang's vanilla Minecraft translation patterns.
    /// </summary>
    /// <param name="key">The translation key to test.</param>
    /// <returns><see langword="true"/> if the key is recognized as a vanilla translation key; otherwise, <see langword="false"/>.</returns>
    private static bool IsVanillaMinecraftKey(string key)
    {
        // Any registry translation entry in modern Minecraft follows '<category>.minecraft.<id>'
        // Examples: item.minecraft.diamond, block.minecraft.stone, entity.minecraft.zombie,
        // enchantment.minecraft.fire_aspect, trim_material.minecraft.iron, etc.
        if (key.Contains(".minecraft.", StringComparison.OrdinalIgnoreCase))
            return true;

        // Resource locations with vanilla prefix
        if (key.StartsWith("minecraft.", StringComparison.OrdinalIgnoreCase) ||
            key.StartsWith("minecraft:", StringComparison.OrdinalIgnoreCase))
            return true;

        // Common vanilla UI, chat, and system key prefixes
        return key.StartsWith("gui.", StringComparison.OrdinalIgnoreCase) ||
               key.StartsWith("menu.", StringComparison.OrdinalIgnoreCase) ||
               key.StartsWith("options.", StringComparison.OrdinalIgnoreCase) ||
               key.StartsWith("key.categories.", StringComparison.OrdinalIgnoreCase);
    }

    private static void ScanAdvancements(
        IEnumerable<BacapAdvancement> advancements,
        OrderedDictionary<string, bool> destination,
        LanguagePackSettings options)
    {
        foreach (var advancement in advancements)
        {
            if (advancement.TitleComponent is { } title)
                CollectFromComponent(title, destination, options);

            if (advancement.DescriptionComponent is { } description)
                CollectFromComponent(description, destination, options);

            if (advancement.ItemRewardFunction.RewardItems is { Count: > 0 } items)
            {
                foreach (var item in items)
                    InspectItemStack(item, destination, options);
            }

            if (advancement.TrophyRewardFunction.Trophies is { Count: > 0 } trophies)
            {
                foreach (var trophy in trophies)
                    InspectItemStack(trophy.Item, destination, options);
            }

            InspectBoundFunction(advancement.MsgFunction, destination, options);
            InspectBoundFunction(advancement.MacroFunction, destination, options);
            InspectBoundFunction(advancement.ExpRewardFunction, destination, options);
            InspectBoundFunction(advancement.ItemRewardFunction, destination, options);
            InspectBoundFunction(advancement.TrophyRewardFunction, destination, options);
        }
    }

    private static void InspectBoundFunction(
        BaseFunction? baseFunction,
        OrderedDictionary<string, bool> destination,
        LanguagePackSettings options)
    {
        if (baseFunction?.Function is { } mcFunction)
            InspectMcFunction(mcFunction, destination, options);
    }

    private static void ScanDiskFunctions(
        Datapack datapack,
        OrderedDictionary<string, bool> destination,
        LanguagePackSettings options)
    {
        if (string.IsNullOrWhiteSpace(datapack.Settings.MainNamespace))
            return;

        var functionDirectory = Path.Combine(
            datapack.DatapackDataPath.FullName,
            datapack.Settings.MainNamespace,
            "function");

        if (!Directory.Exists(functionDirectory))
            return;

        var functionFiles = Directory.EnumerateFiles(functionDirectory, "*.mcfunction", SearchOption.AllDirectories)
            .OrderBy(filePath => filePath, StringComparer.OrdinalIgnoreCase);

        foreach (var filePath in functionFiles)
        {
            var content = File.ReadAllText(filePath);
            var mcFunction = McFunctionParser.Parse(content);
            InspectMcFunction(mcFunction, destination, options);
        }
    }

    private static void InspectMcFunction(
        McFunction mcFunction,
        OrderedDictionary<string, bool> destination,
        LanguagePackSettings options)
    {
        foreach (var line in mcFunction.Lines)
        {
            if (line is ExecutableLine executableLine)
                InspectCommand(executableLine.Command, destination, options);
        }
    }

    private static void InspectCommand(
        ICommand? command,
        OrderedDictionary<string, bool> destination,
        LanguagePackSettings options)
    {
        while (command is ExecuteCommand execute)
        {
            command = execute.RunCommand;
        }

        switch (command)
        {
            case TellrawCommand tellraw:
                CollectFromComponent(tellraw.Message, destination, options);
                break;

            case GiveCommand give:
                InspectItemStack(give.Item, destination, options);
                break;

            case SummonCommand summon:
                InspectSummonCommand(summon, destination, options);
                break;
        }
    }

    private static void InspectSummonCommand(
        SummonCommand summon,
        OrderedDictionary<string, bool> destination,
        LanguagePackSettings options)
    {
        if (summon.Nbt is null)
            return;

        if (IsItemEntity(summon.EntityId))
        {
            if (FindChildCompound(summon.Nbt, "Item") is { } itemCompound)
            {
                var spawnedItem = ItemStack.Parse(itemCompound);
                InspectItemStack(spawnedItem, destination, options);
            }
        }

        if (FindChildNode(summon.Nbt, "CustomName") is not { } customNameNode)
            return;

        var customNameText = TextComponentParser.Parse(customNameNode);
        CollectFromComponent(customNameText, destination, options);
    }

    private static void CollectFromComponent(
        TextComponent? component,
        OrderedDictionary<string, bool> destination,
        LanguagePackSettings options)
    {
        switch (component)
        {
            case null:
                return;

            case TranslatableComponent translatable:
            {
                TryRegisterKey(translatable.Translate, destination, options);

                if (translatable.With is { Count: > 0 } withArgs)
                {
                    foreach (var withComponent in withArgs)
                        CollectFromComponent(withComponent, destination, options);
                }

                break;
            }
        }

        if (component.Extra is not { Count: > 0 } extraList)
            return;

        foreach (var extra in extraList)
            CollectFromComponent(extra, destination, options);
    }

    private static void InspectItemStack(
        ItemStack? item,
        OrderedDictionary<string, bool> destination,
        LanguagePackSettings options)
    {
        if (item is null || item.IsEmpty)
            return;

        if (item.Components.Get<CustomNameComponent>() is { Value: { } customName })
            CollectFromComponent(customName, destination, options);

        if (item.Components.Get<LoreComponent>() is not { Lines: { Count: > 0 } loreLines })
            return;

        foreach (var line in loreLines)
            CollectFromComponent(line, destination, options);
    }

    private static bool IsItemEntity(string entityId)
    {
        return MinecraftUtils.StripNamespace(entityId).Equals("item", StringComparison.OrdinalIgnoreCase);
    }

    private static SnbtCompound? FindChildCompound(SnbtCompound compound, string tagName)
    {
        return FindChildNode(compound, tagName) as SnbtCompound;
    }

    private static ISnbtNode? FindChildNode(SnbtCompound compound, string tagName)
    {
        if (compound.GetNode(tagName) is { } directMatch)
            return directMatch;

        foreach (var (key, value) in compound.Tags)
        {
            if (string.Equals(key, tagName, StringComparison.OrdinalIgnoreCase))
                return value;
        }

        return null;
    }
}