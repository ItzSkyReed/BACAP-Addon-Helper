using BacapGenerator.Configuration.Exceptions;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;

namespace BacapGenerator.Datapacks.Models.Settings.Scoreboards;

/// <summary>
/// Configuration for generating advancement score counter mcfunction files.
/// </summary>
public sealed class ScoreboardScoreUpdateSettings
{
    /// <summary>
    /// Gets the unique identifier for this score counter configuration.
    /// </summary>
    [PublicAPI]
    [ConfigurationKeyName("id")]
    public required string Id { get; init; }

    /// <summary>
    /// Gets the target Minecraft scoreboard objective name (e.g. "bacaped_all_advancements").
    /// </summary>
    [PublicAPI]
    [ConfigurationKeyName("scoreboard")]
    public required string Scoreboard { get; init; }

    /// <summary>
    /// Gets the relative file path for the generated function file (e.g. "update_score_all.mcfunction").
    /// </summary>
    [PublicAPI]
    [ConfigurationKeyName("file_path")]
    public required string FilePath { get; init; }

    /// <summary>
    /// Gets the amount added to the scoreboard per completed advancement.
    /// Defaults to 1.
    /// </summary>
    [PublicAPI]
    [ConfigurationKeyName("amount")]
    public int Amount { get; init; } = 1;

    /// <summary>
    /// Gets the advancement selection filter rules for this counter.
    /// </summary>
    [PublicAPI]
    [ConfigurationKeyName("filter")]
    public ScoreboardFilterSettings Filter { get; init; } = new();

    /// <summary>
    /// Validates the score counter settings.
    /// </summary>
    /// <param name="datapackId">The parent datapack identifier.</param>
    /// <exception cref="DatapackConfigurationException">Thrown when validation constraints are violated.</exception>
    public void Validate(string datapackId)
    {
        if (string.IsNullOrWhiteSpace(Id))
        {
            throw new DatapackConfigurationException(
                datapackId,
                DatapackErrorKind.InvalidScoreConfiguration,
                $"Score counter in datapack '{datapackId}' is missing an '{nameof(Id)}'.",
                nameof(Id));
        }

        if (string.IsNullOrWhiteSpace(Scoreboard))
        {
            throw new DatapackConfigurationException(
                datapackId,
                DatapackErrorKind.InvalidScoreConfiguration,
                $"Score counter '{Id}' in datapack '{datapackId}' must specify a '{nameof(Scoreboard)}'.",
                nameof(Scoreboard));
        }

        if (string.IsNullOrWhiteSpace(FilePath))
        {
            throw new DatapackConfigurationException(
                datapackId,
                DatapackErrorKind.InvalidScoreConfiguration,
                $"Score counter '{Id}' in datapack '{datapackId}' must specify a '{nameof(FilePath)}'.",
                nameof(FilePath));
        }

        if (!FilePath.EndsWith(".mcfunction", StringComparison.OrdinalIgnoreCase))
        {
            throw new DatapackConfigurationException(
                datapackId,
                DatapackErrorKind.InvalidScoreConfiguration,
                $"Score counter '{Id}' in datapack '{datapackId}' specifies '{FilePath}' which must end with '.mcfunction'.",
                nameof(FilePath));
        }

        if (FilePath.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
        {
            throw new DatapackConfigurationException(
                datapackId,
                DatapackErrorKind.InvalidScoreConfiguration,
                $"File path '{FilePath}' in score counter '{Id}' of datapack '{datapackId}' contains invalid filesystem characters.",
                nameof(FilePath));
        }

        Filter.Validate(datapackId, Id);
    }
}