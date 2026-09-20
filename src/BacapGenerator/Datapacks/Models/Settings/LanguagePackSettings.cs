using System.Collections.Frozen;
using System.Reflection;
using BacapGenerator.Common;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;

namespace BacapGenerator.Datapacks.Models.Settings;

/// <summary>
/// Configuration settings and translation discovery filters for a datapack's language resource pack.
/// </summary>
public class LanguagePackSettings
{
    private static readonly FrozenSet<string> DefaultIgnoredKeys = BuildDefaultIgnoredKeys();

    /// <summary>
    /// Gets the optional name of the language pack template to inherit settings from.
    /// </summary>
    [PublicAPI]
    [ConfigurationKeyName("template")]
    public string? Template { get; init; }

    /// <summary>
    /// Gets the optional filesystem path to the language resource pack folder.
    /// </summary>
    [PublicAPI]
    [ConfigurationKeyName("path")]
    public required string Path { get; init; }

    /// <summary>
    /// Gets or sets a value indicating whether default branding tokens and message format templates
    /// harvested from <see cref="DatapackDefaults"/> should be excluded.
    /// Defaults to <see langword="true"/>.
    /// </summary>
    [PublicAPI]
    [ConfigurationKeyName("use_default_ignored_keys")]
    public bool UseDefaultIgnoredKeys { get; init; } = true;

    /// <summary>
    /// Gets the list of user-configured translation keys or literal tokens to bypass during discovery.
    /// </summary>
    [PublicAPI]
    [ConfigurationKeyName("ignored_keys")]
    public List<string>? IgnoredKeys { get; init; }

    /// <summary>
    /// Gets the list of user-configured translation keys or literal tokens to bypass during discovery.
    /// </summary>
    [PublicAPI]
    [ConfigurationKeyName("remove_unused_keys")]
    public bool RemoveUnusedKeys { get; init; }

    /// <summary>
    /// Gets the optional filesystem path to the language resource pack folder.
    /// </summary>
    [PublicAPI]
    [ConfigurationKeyName("base_translation_header")]
    public string? BaseTranslationHeader { get; init; }

    /// <summary>
    /// Gets an immutable frozen lookup set of all effective ignored keys.
    /// </summary>
    [PublicAPI]
    public FrozenSet<string> IgnoredKeysSet => field ??= BuildIgnoredKeysSet();

    /// <summary>
    /// Combines built-in default keys with configured custom exclusions into a single case-insensitive frozen set.
    /// </summary>
    /// <returns>An immutable <see cref="FrozenSet{T}"/> containing all active ignored keys.</returns>
    private FrozenSet<string> BuildIgnoredKeysSet()
    {
        var combined = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (UseDefaultIgnoredKeys)
            combined.UnionWith(DefaultIgnoredKeys);

        if (IgnoredKeys is { Count: > 0 })
            combined.UnionWith(IgnoredKeys);

        return combined.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Discovers default keys by scanning <see cref="DatapackDefaults"/> for message format entries
    /// and pairing them with default external branding tokens.
    /// </summary>
    /// <returns>An immutable <see cref="FrozenSet{T}"/> of standard ignored tokens.</returns>
    private static FrozenSet<string> BuildDefaultIgnoredKeys()
    {
        var keys = new HashSet<string>(StringComparer.Ordinal)
        {
            "GitHub", "Modrinth", "Discord", "CurseForge", "YouTube", "Twitter", // Social media
            // Lines from BACAP
            "To view progress, run:", "Awarded for achieving", "Animals", "Challenges",
            // Rewards
            "Experience"
        };

        var entryFields = typeof(DatapackDefaults)
            .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
            .Where(f => typeof(AdvancementMessageSettingsEntry).IsAssignableFrom(f.FieldType));

        foreach (var field in entryFields)
        {
            if (field.GetValue(null) is AdvancementMessageSettingsEntry { TranslationKey: { } translationKey } &&
                !string.IsNullOrWhiteSpace(translationKey))
            {
                keys.Add(translationKey.Trim());
            }
        }

        foreach (var key in BacapTab.All)
        {
            keys.Add(key.DisplayName);
        }

        return keys.ToFrozenSet(StringComparer.Ordinal);
    }
}