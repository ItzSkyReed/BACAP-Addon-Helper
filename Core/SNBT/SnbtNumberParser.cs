using System.Globalization;
using Core.SNBT.Interfaces;
using Core.SNBT.Nodes;

namespace Core.SNBT;

/// <summary>
/// Provides highly optimized, low-allocation parsing of SNBT numeric strings.
/// Supports floating-point numbers, integers, hex/binary bases, and type suffixes.
/// </summary>
internal static class SnbtNumberParser
{
    /// <summary>
    /// Parses a raw numeric string into its corresponding strongly-typed <see cref="ISnbtNode"/>.
    /// </summary>
    /// <param name="raw">The raw numeric string to parse (e.g., "1.5f", "0x1A", "10b").</param>
    /// <returns>An <see cref="ISnbtNode"/> representing the specific number type, or an <see cref="SnbtString"/> if the input is empty or invalid.</returns>
    /// <exception cref="System.FormatException">Thrown if the numeric format is fundamentally malformed during internal parsing.</exception>
    /// <exception cref="System.OverflowException">Thrown if the number exceeds the capacity of the target data type.</exception>
    /// <example>
    /// <code>
    /// ISnbtNode byteNode = SnbtNumberParser.Parse("15b");   // Returns SnbtByte
    /// ISnbtNode hexNode = SnbtNumberParser.Parse("0xFF");   // Returns SnbtInt
    /// ISnbtNode doubleNode = SnbtNumberParser.Parse("1.5"); // Returns SnbtDouble
    /// </code>
    /// </example>
    public static ISnbtNode Parse(string raw)
    {
        if (string.IsNullOrEmpty(raw)) return new SnbtString(raw);

        // Fast, allocation-free underscore removal
        Span<char> cleanSpan = stackalloc char[raw.Length];
        var len = 0;
        foreach (var c in raw)
        {
            if (c != '_')
                cleanSpan[len++] = c;
        }
        ReadOnlySpan<char> span = cleanSpan[..len];

        if (span.Length == 0) return new SnbtString(raw);

        // Check for floating-point indicators
        var hasFloatChars = span.Contains('.') || span.Contains('e') || span.Contains('E');
        var last = char.ToLowerInvariant(span[^1]);

        // If it contains decimals, scientific notation, or float/double suffixes -> parse as Float/Double
        if (hasFloatChars || last == 'f' || last == 'd')
        {
            return ParseFloat(span, last);
        }

        // Otherwise, proceed to integer parsing
        return ParseInteger(span);
    }

    private static ISnbtNode ParseFloat(ReadOnlySpan<char> span, char lastChar)
    {
        var valueSpan = span;
        if (lastChar is 'f' or 'd')
        {
            valueSpan = span[..^1]; // Trim suffix
        }

        if (lastChar == 'f')
            return new SnbtFloat(float.Parse(valueSpan, NumberStyles.Float, CultureInfo.InvariantCulture));

        return new SnbtDouble(double.Parse(valueSpan, NumberStyles.Float, CultureInfo.InvariantCulture));
    }

    private static ISnbtNode ParseInteger(ReadOnlySpan<char> span)
    {
        // Extract the sign
        var isNegative = false;
        var startIdx = 0;
        switch (span.Length)
        {
            case > 0 when span[0] == '-':
                isNegative = true;
                startIdx = 1;
                break;
            case > 0 when span[0] == '+':
                startIdx = 1;
                break;
        }

        var valueSpan = span[startIdx..];

        // Check for base prefixes (0x for Hex, 0b for Binary)
        var isHex = valueSpan.StartsWith("0x", StringComparison.OrdinalIgnoreCase);
        var isBin = valueSpan.StartsWith("0b", StringComparison.OrdinalIgnoreCase);

        // "0b" could simply be the number 0 with a 'b' (Byte) suffix, NOT an empty binary prefix!
        if (isBin && valueSpan.Length == 2)
            isBin = false;

        if (isHex || isBin)
            valueSpan = valueSpan[2..]; // Remove '0x' or '0b'

        // Extract trailing suffixes (type and sign indicators)
        var typeSuffix = '\0';
        var signSuffix = '\0';

        if (valueSpan.Length > 0)
        {
            var last = char.ToLowerInvariant(valueSpan[^1]);
            // Supported type suffixes: b (byte), s (short), l (long), i (int)
            if (last is 'b' or 's' or 'l' or 'i')
            {
                typeSuffix = last;
                valueSpan = valueSpan[..^1];
            }
        }

        if (valueSpan.Length > 0)
        {
            var last = char.ToLowerInvariant(valueSpan[^1]);
            // Supported sign suffixes: u (unsigned), s (signed)
            if (last is 'u' or 's')
            {
                signSuffix = last;
                valueSpan = valueSpan[..^1];
            }
        }

        var isUnsigned = signSuffix == 'u';

        // Parse the numeric value
        long finalValue = 0;
        if (valueSpan.Length > 0) // Protection against empty strings after stripping prefixes/suffixes
        {
            if (isHex)
            {
                if (isUnsigned) finalValue = (long)ulong.Parse(valueSpan, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                else finalValue = long.Parse(valueSpan, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            }
            else if (isBin)
            {
                ulong binResult = 0;
                foreach (var c in valueSpan)
                {
                    // Bitwise left shift and add the current binary digit (0 or 1)
                    binResult = (binResult << 1) + (ulong)(c - '0');
                }
                finalValue = (long)binResult;
            }
            else
            {
                if (isUnsigned) finalValue = (long)ulong.Parse(valueSpan, NumberStyles.Integer, CultureInfo.InvariantCulture);
                else finalValue = long.Parse(valueSpan, NumberStyles.Integer, CultureInfo.InvariantCulture);
            }
        }

        if (isNegative) finalValue = -finalValue;

        // Construct and return the appropriate AST Node based on the type suffix
        return typeSuffix switch
        {
            'b' => new SnbtByte((sbyte)finalValue),
            's' => new SnbtShort((short)finalValue),
            'l' => new SnbtLong(finalValue),
            _ => new SnbtInt((int)finalValue) // Default to Int if no suffix is provided
        };
    }
}