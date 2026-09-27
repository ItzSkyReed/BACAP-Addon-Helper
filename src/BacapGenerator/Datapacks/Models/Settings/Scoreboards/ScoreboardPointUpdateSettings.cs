
using BacapGenerator.Configuration.Exceptions;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;

namespace BacapGenerator.Datapacks.Models.Settings.Scoreboards;
/// <summary>
/// Configuration for generating advancement point assignment mcfunction files based on tier calculations.
/// </summary>
public sealed class ScoreboardPointUpdateSettings
{
    /// <summary>
    /// Gets the unique identifier for this point counter configuration.
    /// </summary>
    [PublicAPI]
    [ConfigurationKeyName("id")]
    public required string Id { get; init; }

    /// <summary>
    /// Gets the target scoreboard objective receiving the points (e.g. "bac_advancements_points").
    /// </summary>
    [PublicAPI]
    [ConfigurationKeyName("scoreboard")]
    public required string Scoreboard { get; init; }

    /// <summary>
    /// Gets the source scoreboard storing points per tier.
    /// Defaults to "bac_points".
    /// </summary>
    [PublicAPI]
    [ConfigurationKeyName("source_scoreboard")]
    public string SourceScoreboard { get; init; } = DatapackDefaults.PointsValuesScoreboard;

    /// <summary>
    /// Gets the scoreboard operation applied (e.g. "+=").
    /// Defaults to "+=".
    /// </summary>
    [PublicAPI]
    [ConfigurationKeyName("operation")]
    public string Operation { get; init; } = "+=";

    /// <summary>
    /// Gets the relative file path for the generated function file (e.g. "update_points.mcfunction").
    /// </summary>
    [PublicAPI]
    [ConfigurationKeyName("file_path")]
    public required string FilePath { get; init; }

    /// <summary>
    /// Gets the advancement selection filter rules for this point counter.
    /// </summary>
    [PublicAPI]
    [ConfigurationKeyName("filter")]
    public ScoreboardFilterSettings Filter { get; init; } = new();

    /// <summary>
    /// Validates the point counter settings.
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
                $"Point counter in datapack '{datapackId}' is missing an '{nameof(Id)}'.",
                nameof(Id));
        }

        if (string.IsNullOrWhiteSpace(Scoreboard))
        {
            throw new DatapackConfigurationException(
                datapackId,
                DatapackErrorKind.InvalidScoreConfiguration,
                $"Point counter '{Id}' in datapack '{datapackId}' must specify a '{nameof(Scoreboard)}'.",
                nameof(Scoreboard));
        }

        if (string.IsNullOrWhiteSpace(SourceScoreboard))
        {
            throw new DatapackConfigurationException(
                datapackId,
                DatapackErrorKind.InvalidScoreConfiguration,
                $"Point counter '{Id}' in datapack '{datapackId}' must specify a '{nameof(SourceScoreboard)}'.",
                nameof(SourceScoreboard));
        }

        if (string.IsNullOrWhiteSpace(Operation))
        {
            throw new DatapackConfigurationException(
                datapackId,
                DatapackErrorKind.InvalidScoreConfiguration,
                $"Point counter '{Id}' in datapack '{datapackId}' must specify an '{nameof(Operation)}'.",
                nameof(Operation));
        }

        if (string.IsNullOrWhiteSpace(FilePath))
        {
            throw new DatapackConfigurationException(
                datapackId,
                DatapackErrorKind.InvalidScoreConfiguration,
                $"Point counter '{Id}' in datapack '{datapackId}' must specify a '{nameof(FilePath)}'.",
                nameof(FilePath));
        }

        if (!FilePath.EndsWith(".mcfunction", StringComparison.OrdinalIgnoreCase))
        {
            throw new DatapackConfigurationException(
                datapackId,
                DatapackErrorKind.InvalidScoreConfiguration,
                $"Point counter '{Id}' in datapack '{datapackId}' specifies '{FilePath}' which must end with '.mcfunction'.",
                nameof(FilePath));
        }

        if (FilePath.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
        {
            throw new DatapackConfigurationException(
                datapackId,
                DatapackErrorKind.InvalidScoreConfiguration,
                $"File path '{FilePath}' in point counter '{Id}' of datapack '{datapackId}' contains invalid filesystem characters.",
                nameof(FilePath));
        }

        Filter.Validate(datapackId, Id);
    }
}