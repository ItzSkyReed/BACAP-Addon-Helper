using BacapGenerator.Advancements.Models;
using BacapGenerator.Configuration;
using BacapGenerator.Configuration.Exceptions;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;

namespace BacapGenerator.Datapacks.Models.Settings.Scoreboards;

/// <summary>
/// Encapsulates filtering rules used to select specific advancements for scoreboard and point generation.
/// </summary>
public sealed class ScoreboardFilterSettings
{
    /// <summary>
    /// Gets the collection of datapack identifiers to include.
    /// If null or empty, advancements from all datapacks are evaluated.
    /// </summary>
    [PublicAPI]
    [ConfigurationKeyName("datapacks")]
    public List<string>? Datapacks { get; init; }

    /// <summary>
    /// Gets the raw string representation of the hidden filter as defined in configuration.
    /// </summary>
    [PublicAPI]
    [ConfigurationKeyName("hidden")]
    public string? RawHidden { get; init; }

    /// <summary>
    /// Gets the parsed filter mode for hidden advancements.
    /// Defaults to <see cref="ScoreboardHiddenFilter.Exclude"/>.
    /// </summary>
    public ScoreboardHiddenFilter HiddenFilter { get; private set; } = ScoreboardHiddenFilter.Exclude;

    /// <summary>
    /// Gets the raw string representation of the root advancement exclusion toggle.
    /// </summary>
    [PublicAPI]
    [ConfigurationKeyName("exclude_root")]
    public string? RawExcludeRoot { get; init; }

    /// <summary>
    /// Gets a value indicating whether root advancements (<see cref="BacapAdvancementTier.Root"/>) should be excluded.
    /// Defaults to <see langword="false"/>.
    /// </summary>
    public bool ExcludeRoot { get; private set; }

    /// <summary>
    /// Gets the optional whitelist of specific advancement tiers to match.
    /// </summary>
    [PublicAPI]
    [ConfigurationKeyName("tiers")]
    public List<BacapAdvancementTier>? Tiers { get; init; }

    /// <summary>
    /// Determines whether the given advancement satisfies all configured filter rules.
    /// If no explicit datapacks are specified, only advancements from <paramref name="ownerDatapackId"/> are matched.
    /// </summary>
    /// <param name="advancement">The target advancement to test.</param>
    /// <param name="ownerDatapackId">The identifier of the datapack owning this counter configuration.</param>
    /// <returns><see langword="true"/> if the advancement satisfies the conditions; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="advancement"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="ownerDatapackId"/> is null, empty, or whitespace.</exception>
    /// <example>
    /// <code>
    /// bool isMatch = filterSettings.Matches(advancement, "bacaped");
    /// </code>
    /// </example>
    public bool Matches(BacapAdvancement advancement, string ownerDatapackId)
    {
        ArgumentNullException.ThrowIfNull(advancement);
        ArgumentException.ThrowIfNullOrWhiteSpace(ownerDatapackId);

        if (ExcludeRoot && advancement.Tier == BacapAdvancementTier.Root)
            return false;

        if (Datapacks is { Count: > 0 })
        {
            var matchAll = Datapacks.Contains("*") || Datapacks.Contains("@all", StringComparer.OrdinalIgnoreCase);
            if (!matchAll && !Datapacks.Contains(advancement.Datapack.Id, StringComparer.OrdinalIgnoreCase))
                return false;
        }
        else
        {
            if (!string.Equals(advancement.Datapack.Id, ownerDatapackId, StringComparison.OrdinalIgnoreCase))
                return false;
        }

        var isHidden = advancement.Tier == BacapAdvancementTier.Hidden;
        switch (HiddenFilter)
        {
            case ScoreboardHiddenFilter.Exclude when isHidden:
            case ScoreboardHiddenFilter.Only when !isHidden:
                return false;
            case ScoreboardHiddenFilter.All:
            default:
                break;
        }

        return Tiers is not { Count: > 0 } || Tiers.Contains(advancement.Tier);
    }

    /// <summary>
    /// Validates filter configuration settings and explicitly parses filter options.
    /// </summary>
    /// <param name="datapackId">The parent datapack identifier.</param>
    /// <param name="counterId">The owning counter configuration identifier.</param>
    /// <exception cref="DatapackConfigurationException">Thrown when filter constraints or formats are invalid.</exception>
    /// <example>
    /// <code>
    /// filterSettings.Validate("bacaped", "ed_advancements");
    /// </code>
    /// </example>
    public void Validate(string datapackId, string counterId)
    {
        if (Datapacks is not null && Datapacks.Any(string.IsNullOrWhiteSpace))
            throw new DatapackConfigurationException(
                datapackId,
                DatapackErrorKind.InvalidScoreConfiguration,
                $"Filter in counter '{counterId}' of datapack '{datapackId}' contains empty datapack identifiers.",
                "datapacks");

        HiddenFilter = ConfigParser.ParseOptionalEnum(
            RawHidden,
            ScoreboardHiddenFilter.Exclude,
            datapackId,
            DatapackErrorKind.InvalidScoreConfiguration,
            $"Filter in counter '{counterId}' of datapack '{datapackId}' has an invalid 'hidden' filter value: '{RawHidden}'. Allowed values: all, exclude, only.");

        ExcludeRoot = ConfigParser.ParseBool(
            RawExcludeRoot,
            defaultValue: false,
            datapackId,
            DatapackErrorKind.InvalidScoreConfiguration,
            $"Filter in counter '{counterId}' of datapack '{datapackId}' has an invalid boolean value '{RawExcludeRoot}' for 'exclude_root'. Expected: true or false.");
    }
}