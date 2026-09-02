using Core.DataComponents.Components;
using Core.DataComponents.Interfaces;
using Core.Items;
using Core.SNBT.Interfaces;
using Core.SNBT.Nodes;
using Core.TextComponents.Components;
using Core.TextComponents.Models;

namespace BacapGenerator.Models.Advancements.Functions.Trophy;

/// <summary>
/// Wrapper for a Trophy reward.
/// Dynamically reads and writes its properties directly from the underlying <see cref="ItemStack"/> components
/// to prevent any data desynchronization.
/// </summary>
public record TrophyReward(ItemStack Item)
{
    /// <summary>
    /// Gets the translation key directly from the item's CustomName component.
    /// </summary>
    public string? TitleTranslationKey
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
    /// Factory method to easily construct a brand new trophy item with all necessary NBT components correctly configured.
    /// </summary>
    /// <param name="itemId">The base Minecraft item id (e.g., "minecraft:cobblestone_stairs").</param>
    /// <param name="titleKey">The translation key for the trophy title.</param>
    /// <param name="titleColor">The hex color string for the title.</param>
    /// <param name="count">The item stack count.</param>
    /// <param name="extraSetComponents">Extra item components to add or overwrite.</param>
    /// <param name="extraRemoveComponents">Component IDs (e.g., "minecraft:enchantments") to explicitly remove (!component).</param>
    /// <returns>A new <see cref="TrophyReward"/> wrapper with the fully configured item stack.</returns>
    public static TrophyReward CreateNew(
        string itemId,
        string titleKey,
        string titleColor = "#B0CCD8",
        int count = 1,
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

        // Add extra components
        if (extraSetComponents is not null)
        {
            foreach (var setComponent in extraSetComponents)
            {
                item.Components.Set(setComponent);
            }
        }

        // Process explicitly removed components
        if (extraRemoveComponents is null)
            return new TrophyReward(item);

        foreach (var componentId in extraRemoveComponents)
            item.Components.Remove(componentId);

        return new TrophyReward(item);
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
    [PublicAPI]
    public void Standardize(IReadOnlyDatapack datapack)
    {
        ArgumentNullException.ThrowIfNull(datapack);

        // Standardize Custom Model Data
        var titleKey = Title;
        if (!string.IsNullOrWhiteSpace(titleKey))
        {
            var expectedModelId = $"{datapack.Settings.MainNamespace}:{BacapUtils.ToSnakeCaseSlug(titleKey)}";

            // Overwrite or add the custom_model_data component to ensure it matches the title
            Item.Components.Set(new CustomModelDataComponent(Strings: [expectedModelId]));
        }

        // Ensure Trophy marker tag is intact (safeguard for manually edited items)
        if (Item.Components.TryGet<CustomDataComponent>(out var customData))
        {
            if (customData.Tag is not { } compound || compound.Tags.ContainsKey("Trophy"))
                return;

            // Rebuild the compound with the missing Trophy tag
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