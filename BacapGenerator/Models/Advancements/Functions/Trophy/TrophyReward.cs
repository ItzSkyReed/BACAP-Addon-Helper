using BacapGenerator.Models.Interfaces;
using BacapGenerator.Utils;
using Core.DataComponents.Components;
using Core.DataComponents.Interfaces;
using Core.Items;
using Core.SNBT.Interfaces;
using Core.SNBT.Nodes;
using Core.TextComponents.Components;
using Core.TextComponents.Models;
using JetBrains.Annotations;

namespace BacapGenerator.Models.Advancements.Functions.Trophy;

/// <summary>
/// Wrapper for a Trophy reward.
/// Dynamically reads and writes its properties directly from the underlying <see cref="ItemStack"/> components
/// to prevent any data desynchronization.
/// </summary>
/// <param name="Item">The underlying item stack containing components and metadata.</param>
/// <param name="DeliveryType">The delivery mechanism used to grant the trophy.</param>
public record TrophyReward(ItemStack Item, TrophyDeliveryType DeliveryType = TrophyDeliveryType.Inventory)
{
    /// <summary>
    /// Gets the translation key directly from the item's CustomName component.
    /// </summary>
    public string? Title
    {
        get
        {
            if (Item.Components.TryGet<CustomNameComponent>(out var customName))
            {
                return customName.Value switch
                {
                    TranslatableComponent tc => tc.Translate,
                    PlainTextComponent ptc => ptc.Text,
                    _ => null
                };
            }

            return null;
        }
    }

    /// <summary>
    /// Gets the sanitized description lines from the item's Lore component.
    /// </summary>
    public IReadOnlyList<string> DescriptionLines
    {
        get
        {
            if (!Item.Components.TryGet<LoreComponent>(out var loreComponent))
                return [];

            return loreComponent.Lines
                .Select(line => line switch
                {
                    TranslatableComponent tc => tc.Translate,
                    PlainTextComponent ptc => ptc.Text,
                    _ => string.Empty
                })
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .Where(line => !IsGeneratedLine(line))
                .Where(line => line != Title)
                .ToList();
        }
    }

    /// <summary>
    /// Gets the title color directly from the item's CustomName component.
    /// </summary>
    public string? TitleColor
    {
        get
        {
            if (Item.Components.TryGet<CustomNameComponent>(out var customName) &&
                customName.Value.Style?.Color != null)
            {
                return customName.Value.Style.Color;
            }

            return null;
        }
    }

    /// <summary>
    /// Factory method to construct a brand new trophy item with all necessary NBT components correctly configured.
    /// </summary>
    /// <param name="datapack">The datapack context of the trophy.</param>
    /// <param name="itemId">The base Minecraft item ID (e.g., "minecraft:cobblestone_stairs").</param>
    /// <param name="titleKey">The translation key for the trophy title.</param>
    /// <param name="titleColor">The hex color string for the title.</param>
    /// <param name="count">The item stack count.</param>
    /// <param name="deliveryType">The delivery mechanism for granting the trophy.</param>
    /// <param name="extraSetComponents">Extra item components to add or overwrite.</param>
    /// <param name="extraRemoveComponents">Component IDs (e.g., "minecraft:enchantments") to explicitly remove.</param>
    /// <returns>A new <see cref="TrophyReward"/> wrapper with the fully configured item stack.</returns>
    public static TrophyReward CreateNew(
        IReadOnlyDatapack datapack,
        string itemId,
        string titleKey,
        string titleColor,
        int count = 1,
        TrophyDeliveryType deliveryType = TrophyDeliveryType.Inventory,
        IEnumerable<IDataComponent>? extraSetComponents = null,
        IEnumerable<string>? extraRemoveComponents = null)
    {
        var item = new ItemStack(itemId, count);

        // Mark as Trophy (custom_data)
        var customDataTags = new Dictionary<string, ISnbtNode>
        {
            ["Trophy"] = new SnbtBool(true)
        };
        item.Components.Set(new CustomDataComponent(new SnbtCompound(customDataTags)));

        // Set Custom Name
        var style = new TextStyle(Color: titleColor, Bold: true, Italic: false);
        var customNameTags = new TranslatableComponent(titleKey, Style: style);
        item.Components.Set(new CustomNameComponent(customNameTags));

        // Set custom_model_data
        var customModelData = new CustomModelDataComponent(Strings: [$"{datapack.Settings.MainNamespace}:{BacapUtils.ToSnakeCaseSlug(titleKey)}"]);
        item.Components.Set(customModelData);

        // Add extra components
        if (extraSetComponents is not null)
        {
            foreach (var setComponent in extraSetComponents)
                item.Components.Set(setComponent);
        }

        // Process explicitly removed components
        if (extraRemoveComponents is null)
            return new TrophyReward(item, deliveryType);

        foreach (var componentId in extraRemoveComponents)
            item.Components.Remove(componentId);

        return new TrophyReward(item, deliveryType);
    }

    private static bool IsGeneratedLine(string line)
    {
        return line.Trim().Equals(
            "Awarded for achieving",
            StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Updates the trophy item components to match the current datapack standards.
    /// Injects missing components like custom_model_data or base tags.
    /// </summary>
    /// <param name="datapack">The datapack context for configuration.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="datapack"/> is null.</exception>
    [PublicAPI]
    public void Standardize(IReadOnlyDatapack datapack)
    {
        ArgumentNullException.ThrowIfNull(datapack);

        // Standardize Custom Model Data
        var titleKey = Title;
        if (!string.IsNullOrWhiteSpace(titleKey))
        {
            var expectedModelId = $"{datapack.Settings.MainNamespace}:{BacapUtils.ToSnakeCaseSlug(titleKey)}";
            Item.Components.Set(new CustomModelDataComponent(Strings: [expectedModelId]));
        }

        // Ensure Trophy marker tag is intact (safeguard for manually edited items)
        if (Item.Components.TryGet<CustomDataComponent>(out var customData))
        {
            if (customData.Tag is not { } compound || compound.Tags.ContainsKey("Trophy"))
                return;

            var newTags = new Dictionary<string, ISnbtNode>(compound.Tags)
            {
                ["Trophy"] = new SnbtBool(true)
            };
            Item.Components.Set(new CustomDataComponent(new SnbtCompound(newTags)));
        }
        else
        {
            var customDataTags = new Dictionary<string, ISnbtNode> { ["Trophy"] = new SnbtBool(true) };
            Item.Components.Set(new CustomDataComponent(new SnbtCompound(customDataTags)));
        }

        // TODO: In the future, add lore standardization here
        // ( automatically appending the "Awarded for achieving" line if missing).
    }
}