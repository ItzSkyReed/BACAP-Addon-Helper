using System.ComponentModel;
using System.Globalization;
using BacapGenerator.Common;

namespace BacapGenerator.Converters;

/// <summary>
/// Converts case-insensitive string folder names into strongly-typed <see cref="BacapTab"/> instances.
/// </summary>
public sealed class BacapAdvancementTabConverter : TypeConverter
{
    /// <summary>
    /// Determines whether this converter can convert an object of the specified source type.
    /// </summary>
    /// <param name="context">Format context information.</param>
    /// <param name="sourceType">The source type to evaluate.</param>
    /// <returns><see langword="true"/> if conversion is supported; otherwise, <see langword="false"/>.</returns>
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) =>
        sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);

    /// <summary>
    /// Converts the specified string value into a <see cref="BacapTab"/>.
    /// </summary>
    /// <param name="context">Format context information.</param>
    /// <param name="culture">Culture information.</param>
    /// <param name="value">The folder name string to convert.</param>
    /// <returns>The resolved <see cref="BacapTab"/> instance.</returns>
    /// <exception cref="FormatException">Thrown when the string cannot be resolved to any known tab.</exception>
    /// <example>
    /// <code>
    /// var converter = new BacapAdvancementTabConverter();
    /// var tab = (BacapTab)converter.ConvertFrom("adventure")!; // BacapTab.Adventure
    /// </code>
    /// </example>
    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        if (value is string folderName)
        {
            return BacapTab.TryFromFolderName(folderName, out var tab)
                ? tab
                : throw new FormatException($"Unknown BACAP advancement tab folder name: '{folderName}'.");
        }

        return base.ConvertFrom(context, culture, value);
    }
}