using System.Collections.Frozen;
using Core.Registries.Models;
using JetBrains.Annotations;

namespace Core.Registries;

/// <summary>
/// Centralized container for all static Minecraft registry data.
/// Should be registered as a Singleton in the dependency injection container.
/// </summary>
public class MinecraftData
{
    // Dictionary registries (key-value)
    [PublicAPI]
    public FrozenDictionary<string, ItemRegistryEntry> Items { get; }
    [PublicAPI]
    public FrozenDictionary<string, BlockRegistryEntry> Blocks { get; }
    [PublicAPI]
    public FrozenDictionary<string, string> TrimMaterialColors { get; }
    [PublicAPI]
    public FrozenDictionary<string, string> TextColors { get; }
    [PublicAPI]
    public FrozenDictionary<string, int> Enchantments { get; }
    [PublicAPI]
    public FrozenDictionary<string, PotionRegistryEntry> Potions { get; }
    [PublicAPI]
    public FrozenDictionary<string, DyeColorRegistryEntry> DyeColors { get; }

    [PublicAPI]
    public FrozenSet<string> Containers { get; }
    [PublicAPI]
    public FrozenSet<string> Effects { get; }
    [PublicAPI]
    public FrozenSet<string> BannerPatterns { get; }
    [PublicAPI]
    public FrozenSet<string> Trims { get; }

    /// <summary>
    /// Initializes and loads all Minecraft registries into memory.
    /// </summary>
    /// <param name="loader">The registry loader instance.</param>
    public MinecraftData(RegistryLoader loader)
    {
        // Load key-value mappings
        Items = loader.LoadRegistry<ItemRegistryEntry>("items.json");
        Blocks = loader.LoadRegistry<BlockRegistryEntry>("blocks.json");
        TextColors = loader.LoadRegistry<string>("text_colors.json");
        TrimMaterialColors = loader.LoadRegistry<string>("trim_material_color.json");
        Enchantments = loader.LoadRegistry<int>("enchantments.json");
        Potions = loader.LoadRegistry<PotionRegistryEntry>("potion.json");
        DyeColors = loader.LoadRegistry<DyeColorRegistryEntry>("dye_colors.json");


        // Load flat arrays
        Containers = loader.LoadRegistry("containers.json");
        Trims = loader.LoadRegistry("trim_list.json");
        Effects = loader.LoadRegistry("effects.json");
        BannerPatterns = loader.LoadRegistry("banner_patterns.json");
    }
}