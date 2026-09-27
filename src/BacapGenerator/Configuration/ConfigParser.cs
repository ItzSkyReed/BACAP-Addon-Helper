using BacapGenerator.Configuration.Exceptions;

namespace BacapGenerator.Configuration;

/// <summary>
/// Provides utility methods for parsing and validating configuration primitives.
/// </summary>
public static class ConfigParser
{
    /// <summary>
    /// Parses a required string representation into the specified enum type, ignoring case and underscores.
    /// Rejects numeric representations to prevent unintended numeric mappings.
    /// </summary>
    /// <typeparam name="TEnum">The target enumeration type.</typeparam>
    /// <param name="raw">The raw string value from configuration.</param>
    /// <param name="datapackId">The owning datapack identifier for error reporting.</param>
    /// <param name="errorKind">The domain error kind associated with the failure.</param>
    /// <param name="missingErrorMessage">The message to use when the raw value is null or whitespace.</param>
    /// <param name="invalidErrorMessage">The message to use when the raw value cannot be parsed or represents a number.</param>
    /// <param name="propertyName">The name of the configuration property for missing parameter tracking.</param>
    /// <returns>The successfully parsed enum value.</returns>
    /// <exception cref="DatapackConfigurationException">
    /// Thrown when <paramref name="raw"/> is null or whitespace, contains a numeric value, or cannot be resolved to a defined member of <typeparamref name="TEnum"/>.
    /// </exception>
    /// <example>
    /// <code>
    /// var type = ConfigParser.ParseRequiredEnum&lt;DatapackType&gt;(
    ///     rawType,
    ///     datapackId,
    ///     DatapackErrorKind.InvalidDatapackType,
    ///     "Missing type property.",
    ///     "Invalid datapack type.",
    ///     nameof(DatapackType));
    /// </code>
    /// </example>
    public static TEnum ParseRequiredEnum<TEnum>(
        string? raw,
        string datapackId,
        DatapackErrorKind errorKind,
        string missingErrorMessage,
        string invalidErrorMessage,
        string propertyName) where TEnum : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(raw))
            throw new DatapackConfigurationException(datapackId, errorKind, missingErrorMessage, propertyName);

        var trimmed = raw.Trim();
        if (char.IsAsciiDigit(trimmed[0]) || trimmed[0] is '+' or '-')
            throw new DatapackConfigurationException(datapackId, errorKind, invalidErrorMessage, raw);

        var normalized = trimmed.Replace("_", string.Empty);
        if (Enum.TryParse<TEnum>(normalized, ignoreCase: true, out var parsed) && Enum.IsDefined(parsed))
            return parsed;

        throw new DatapackConfigurationException(datapackId, errorKind, invalidErrorMessage, raw);
    }

    /// <summary>
    /// Parses an optional string representation into the specified enum type, ignoring case and underscores.
    /// Returns <paramref name="defaultValue"/> when the raw value is null or whitespace.
    /// Rejects numeric representations to prevent unintended numeric mappings.
    /// </summary>
    /// <typeparam name="TEnum">The target enumeration type.</typeparam>
    /// <param name="raw">The raw string value from configuration.</param>
    /// <param name="defaultValue">The fallback value if <paramref name="raw"/> is null or whitespace.</param>
    /// <param name="datapackId">The owning datapack identifier for error reporting.</param>
    /// <param name="errorKind">The domain error kind associated with the failure.</param>
    /// <param name="invalidErrorMessage">The message to use when the raw value cannot be parsed or represents a number.</param>
    /// <returns>The successfully parsed enum value, or <paramref name="defaultValue"/> if null or whitespace.</returns>
    /// <exception cref="DatapackConfigurationException">
    /// Thrown when <paramref name="raw"/> contains a numeric value or cannot be resolved to a defined member of <typeparamref name="TEnum"/>.
    /// </exception>
    /// <example>
    /// <code>
    /// var filter = ConfigParser.ParseOptionalEnum(
    ///     rawHidden,
    ///     ScoreboardHiddenFilter.Exclude,
    ///     datapackId,
    ///     DatapackErrorKind.InvalidScoreConfiguration,
    ///     "Invalid hidden filter value.");
    /// </code>
    /// </example>
    public static TEnum ParseOptionalEnum<TEnum>(
        string? raw,
        TEnum defaultValue,
        string datapackId,
        DatapackErrorKind errorKind,
        string invalidErrorMessage) where TEnum : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(raw))
            return defaultValue;

        var trimmed = raw.Trim();
        if (char.IsAsciiDigit(trimmed[0]) || trimmed[0] is '+' or '-')
            throw new DatapackConfigurationException(datapackId, errorKind, invalidErrorMessage, raw);

        var normalized = trimmed.Replace("_", string.Empty);
        if (Enum.TryParse<TEnum>(normalized, ignoreCase: true, out var parsed) && Enum.IsDefined(parsed))
            return parsed;

        throw new DatapackConfigurationException(datapackId, errorKind, invalidErrorMessage, raw);
    }

    /// <summary>
    /// Parses an optional string representation into a boolean value.
    /// Returns <paramref name="defaultValue"/> when the raw value is null or whitespace.
    /// </summary>
    /// <param name="raw">The raw string value from configuration.</param>
    /// <param name="defaultValue">The fallback value if <paramref name="raw"/> is null or whitespace.</param>
    /// <param name="datapackId">The owning datapack identifier for error reporting.</param>
    /// <param name="errorKind">The domain error kind associated with the failure.</param>
    /// <param name="invalidErrorMessage">The message to use when the raw value cannot be parsed as a boolean.</param>
    /// <returns>The parsed boolean value, or <paramref name="defaultValue"/> if null or whitespace.</returns>
    /// <exception cref="DatapackConfigurationException">Thrown when the value cannot be parsed as a boolean.</exception>
    /// <example>
    /// <code>
    /// bool excludeRoot = ConfigParser.ParseBool(
    ///     rawExcludeRoot,
    ///     defaultValue: false,
    ///     datapackId,
    ///     DatapackErrorKind.InvalidScoreConfiguration,
    ///     "Invalid boolean value.");
    /// </code>
    /// </example>
    public static bool ParseBool(
        string? raw,
        bool defaultValue,
        string datapackId,
        DatapackErrorKind errorKind,
        string invalidErrorMessage)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return defaultValue;

        return bool.TryParse(raw.Trim(), out var parsed)
            ? parsed
            : throw new DatapackConfigurationException(datapackId, errorKind, invalidErrorMessage, raw);
    }
}