using System.ComponentModel;
using System.Globalization;
using BacapGenerator.Datapacks.Models.Settings;

namespace BacapGenerator.Converters;

/// <summary>
/// Converts snake_case and case-insensitive string values to <see cref="DatapackType"/> enum values.
/// </summary>
public sealed class DatapackTypeConverter : TypeConverter
{
    private static readonly string[] AllowedNames = ["reference", "addon", "compatibility_addon"];

    /// <summary>
    /// Determines whether this converter can convert an object of the given source type to <see cref="DatapackType"/>.
    /// </summary>
    /// <param name="context">An <see cref="ITypeDescriptorContext"/> that provides a format context.</param>
    /// <param name="sourceType">A <see cref="Type"/> that represents the source type.</param>
    /// <returns><see langword="true"/> if conversion is supported; otherwise, <see langword="false"/>.</returns>
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) =>
        sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);

    /// <summary>
    /// Converts the specified value object to a valid <see cref="DatapackType"/>.
    /// </summary>
    /// <param name="context">An <see cref="ITypeDescriptorContext"/> that provides a format context.</param>
    /// <param name="culture">The <see cref="CultureInfo"/> to use as the current culture.</param>
    /// <param name="value">The <see cref="object"/> to convert.</param>
    /// <returns>The converted <see cref="DatapackType"/> value.</returns>
    /// <exception cref="FormatException">Thrown when <paramref name="value"/> is empty, numeric, or unmapped.</exception>
    /// <example>
    /// <code>
    /// var converter = new DatapackTypeConverter();
    /// var type = (DatapackType)converter.ConvertFrom("compatibility_addon")!; // DatapackType.CompatibilityAddon
    /// </code>
    /// </example>
    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        if (value is not string text)
            return base.ConvertFrom(context, culture, value);

        var trimmed = text.Trim();

        // Reject empty strings or raw integers to avoid Enum.TryParse creating undefined enum values
        if (trimmed.Length > 0 && !char.IsAsciiDigit(trimmed[0]))
        {
            var normalized = trimmed.Replace("_", string.Empty);

            if (Enum.TryParse<DatapackType>(normalized, ignoreCase: true, out var result) && Enum.IsDefined(result))
                return result;
        }

        var allowed = string.Join(", ", AllowedNames);
        throw new FormatException($"Value '{text}' is not a valid datapack type. Allowed values are: {allowed}.");
    }
}