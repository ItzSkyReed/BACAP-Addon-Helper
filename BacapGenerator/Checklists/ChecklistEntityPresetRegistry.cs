using System.Collections.Frozen;
using JetBrains.Annotations;

namespace BacapGenerator.Checklists;

/// <summary>
/// Provides predefined entity pools and expands shortcut aliases into concrete entity IDs.
/// </summary>
public static class ChecklistEntityPresetRegistry
{
    /// <summary>
    /// Overworld entity pool for the Mob Universe checklist.
    /// </summary>
    public static readonly FrozenSet<string> AllOverworld =
    [
        "allay", "armadillo", "axolotl", "bat", "bee", "bogged", "breeze",
        "camel", "camel_husk", "cat", "cave_spider", "chicken", "cod",
        "copper_golem", "cow", "creaking", "creeper", "dolphin", "donkey",
        "drowned", "elder_guardian", "evoker", "fox", "frog", "glow_squid",
        "goat", "guardian", "horse", "husk", "iron_golem", "llama",
        "mooshroom", "mule", "nautilus", "ocelot", "panda", "parched",
        "parrot", "phantom", "pig", "pillager", "polar_bear", "pufferfish",
        "rabbit", "ravager", "salmon", "sheep", "silverfish", "skeleton",
        "skeleton_horse", "slime", "sniffer", "snow_golem", "spider",
        "squid", "stray", "sulfur_cube", "tadpole", "trader_llama",
        "tropical_fish", "turtle", "vex", "villager", "vindicator",
        "wandering_trader", "warden", "witch", "wolf", "zombie",
        "zombie_horse", "zombie_nautilus", "zombie_villager"
    ];

    /// <summary>
    /// Nether entity pool for the Mob Universe checklist.
    /// </summary>
    public static readonly FrozenSet<string> AllNether =
    [
        "blaze", "ghast", "happy_ghast", "hoglin", "magma_cube",
        "piglin", "piglin_brute", "strider", "wither",
        "wither_skeleton", "zoglin", "zombified_piglin"
    ];

    /// <summary>
    /// End entity pool for the Mob Universe checklist.
    /// </summary>
    public static readonly FrozenSet<string> AllEnd =
    [
        "ender_dragon", "enderman", "endermite", "shulker"
    ];

    /// <summary>
    /// Breedable animal pool for the Baby Zoo checklist.
    /// </summary>
    public static readonly FrozenSet<string> BabyZoo =
    [
        "armadillo", "axolotl", "bee", "camel", "cat", "chicken", "cow", "dolphin",
        "donkey", "fox", "glow_squid", "goat", "happy_ghast", "hoglin", "horse",
        "llama", "mooshroom", "mule", "nautilus", "ocelot", "panda", "pig",
        "polar_bear", "rabbit", "sheep", "sniffer", "squid", "strider", "turtle", "wolf"
    ];

    private static readonly FrozenDictionary<string, FrozenSet<string>> Presets =
        new Dictionary<string, FrozenSet<string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["all_overworld"] = AllOverworld,
            ["overworld"] = AllOverworld,
            ["all_nether"] = AllNether,
            ["nether"] = AllNether,
            ["all_end"] = AllEnd,
            ["end"] = AllEnd,
            ["baby_zoo"] = BabyZoo,
            ["all_baby_mobs"] = BabyZoo
        }.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Resolves entity tokens, replacing shortcut keywords with their expanded entity lists.
    /// </summary>
    /// <param name="tokens">A collection of raw entity tokens or shortcut keywords.</param>
    /// <returns>A sorted, deduplicated collection of entity names.</returns>
    /// <example>
    /// <code>
    /// var mobs = ChecklistEntityPresetRegistry.ResolveEntities(["@all_end", "cow"]);
    /// </code>
    /// </example>
    [PublicAPI]
    public static IReadOnlyList<string> ResolveEntities(IEnumerable<string> tokens)
    {
        ArgumentNullException.ThrowIfNull(tokens);

        var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var token in tokens)
        {
            if (string.IsNullOrWhiteSpace(token))
                continue;

            var key = token.Trim().TrimStart('@');

            if (Presets.TryGetValue(key, out var presetEntities))
                result.UnionWith(presetEntities);
            else
                result.Add(key.ToLowerInvariant());
        }

        return [.. result.Order(StringComparer.OrdinalIgnoreCase)];
    }
}