using System.ComponentModel;
using System.Globalization;
using BacapGenerator.Models.Datapacks.Settings;

namespace BacapGenerator.Converters;

/// <summary>
/// Converts snake_case and case-insensitive string values to <see cref="DatapackType"/> enum values.
/// </summary>
public sealed class DatapackTypeConverter : TypeConverter
{
    /// <summary>
    /// Determines whether this converter can convert an object of the given source type to <see cref="DatapackType"/>.
    /// </summary>
    /// <param name="context">An <see cref="ITypeDescriptorContext"/> that provides a format context.</param>
    /// <param name="sourceType">A <see cref="Type"/> that represents the source type.</param>
    /// <returns><see langword="true"/> if conversion is supported; otherwise, <see langword="false"/>.</returns>
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) =>
        sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);

    /// <summary>
    /// Converts the specified value object to a <see cref="DatapackType"/>.
    /// </summary>
    /// <param name="context">An <see cref="ITypeDescriptorContext"/> that provides a format context.</param>
    /// <param name="culture">The <see cref="CultureInfo"/> to use as the current culture.</param>
    /// <param name="value">The <see cref="object"/> to convert.</param>
    /// <returns>The converted <see cref="DatapackType"/> value.</returns>
    /// <exception cref="FormatException">Thrown when <paramref name="value"/> cannot be converted to <see cref="DatapackType"/>.</exception>
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

        var normalized = text.Replace("_", string.Empty);
        return Enum.TryParse<DatapackType>(normalized, ignoreCase: true, out var result)
            ? result
            : throw new FormatException($"Value '{text}' cannot be parsed into {nameof(DatapackType)}.");
    }
}