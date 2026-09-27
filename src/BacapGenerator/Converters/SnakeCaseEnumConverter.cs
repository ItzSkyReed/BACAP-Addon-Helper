using System.ComponentModel;
using System.Globalization;

namespace BacapGenerator.Converters;

/// <summary>
/// Converts snake_case and case-insensitive string values to defined enum values.
/// </summary>
public class SnakeCaseEnumConverter : EnumConverter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SnakeCaseEnumConverter"/> class for the specified enum type.
    /// </summary>
    /// <param name="type">The <see cref="Type"/> of the enum to convert to.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="type"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="type"/> is not an <see cref="Enum"/>.</exception>
    public SnakeCaseEnumConverter(Type type) : base(type)
    {
    }

    /// <summary>
    /// Determines whether this converter can convert an object of the given source type to the target enum type.
    /// </summary>
    /// <param name="context">An <see cref="ITypeDescriptorContext"/> that provides a format context.</param>
    /// <param name="sourceType">A <see cref="Type"/> that represents the source type.</param>
    /// <returns><see langword="true"/> if conversion is supported; otherwise, <see langword="false"/>.</returns>
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) =>
        sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);

    /// <summary>
    /// Converts the specified value object to an enum value.
    /// </summary>
    /// <param name="context">An <see cref="ITypeDescriptorContext"/> that provides a format context.</param>
    /// <param name="culture">The <see cref="CultureInfo"/> to use as the current culture.</param>
    /// <param name="value">The <see cref="object"/> to convert.</param>
    /// <returns>The converted enum value as an <see cref="object"/>.</returns>
    /// <exception cref="FormatException">
    /// Thrown when <paramref name="value"/> is an empty string, represents a numeric value, or cannot be mapped to any defined enum member.
    /// </exception>
    /// <exception cref="NotSupportedException">
    /// Thrown when conversion cannot be performed from the provided source type.
    /// </exception>
    /// <example>
    /// <code>
    /// var converter = TypeDescriptor.GetConverter(typeof(DatapackType));
    /// var result = (DatapackType)converter.ConvertFrom("compatibility_addon")!; // DatapackType.CompatibilityAddon
    /// </code>
    /// </example>
    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        if (value is not string text)
            return base.ConvertFrom(context, culture, value);

        var trimmed = text.Trim();

        // Reject empty values or numeric literals (e.g. "0", "1", "-1") to prevent unintended raw integer parsing
        if (trimmed.Length == 0 || char.IsAsciiDigit(trimmed[0]) || trimmed[0] is '+' or '-')
            throw new FormatException($"Cannot convert \"{text}\" to {EnumType.Name}. Empty and numeric values are prohibited.");

        var normalized = trimmed.Replace("_", string.Empty);

        if (Enum.TryParse(EnumType, normalized, ignoreCase: true, out var result) && Enum.IsDefined(EnumType, result))
            return result;

        throw new FormatException($"Requested value \"{text}\" was not found in enum {EnumType.Name}.");
    }
}

/// <summary>
/// Generic variant of <see cref="SnakeCaseEnumConverter"/> for strong typing.
/// </summary>
/// <typeparam name="TEnum">The target enum type.</typeparam>
/// <example>
/// <code>
/// [TypeConverter(typeof(SnakeCaseEnumConverter&lt;DatapackType&gt;))]
/// public enum DatapackType { ... }
/// </code>
/// </example>
public sealed class SnakeCaseEnumConverter<TEnum>() : SnakeCaseEnumConverter(typeof(TEnum))
    where TEnum : struct, Enum;