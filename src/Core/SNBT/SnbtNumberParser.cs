using System.Globalization;
using Core.SNBT.Interfaces;
using Core.SNBT.Nodes;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Core.SNBT;

/// <summary>
/// Provides high-performance, low-allocation parsing of SNBT numeric literals.
/// Fully adheres to Minecraft Java Edition SNBT specification rules for floating-point,
/// integer bases (decimal, hex, binary), signedness suffixes (u, s), and two's complement.
/// </summary>
internal static class SnbtNumberParser
{
    private const int MaxStackAlloc = 256;

    /// <summary>
    /// Parses a raw numeric string into its corresponding strongly-typed <see cref="ISnbtNode"/>.
    /// </summary>
    /// <param name="raw">The raw numeric string to parse (e.g., "1.5f", "0x1A", "10b", "240uB").</param>
    /// <returns>An <see cref="ISnbtNode"/> representing the specific number type, or an <see cref="SnbtString"/> if the input is empty.</returns>
    /// <exception cref="FormatException">Thrown if the numeric format, suffix order, or underscore placement is malformed.</exception>
    /// <exception cref="OverflowException">Thrown if the number exceeds the capacity or range of the target data type.</exception>
    /// <example>
    /// <code>
    /// ISnbtNode byteNode = SnbtNumberParser.Parse("240uB");  // Returns SnbtByte(-16)
    /// ISnbtNode hexNode = SnbtNumberParser.Parse("0xCAFE");  // Returns SnbtInt(51966)
    /// ISnbtNode floatNode = SnbtNumberParser.Parse(".5f");   // Returns SnbtFloat(0.5f)
    /// </code>
    /// </example>
    public static ISnbtNode Parse(string raw)
    {
        if (string.IsNullOrEmpty(raw))
        {
            return new SnbtString(raw);
        }

        switch (raw)
        {
            // Special case: canonical byte zero representation "0b" / "-0b" / "+0b"
            case "0b" or "0B":
            case "-0b" or "-0B":
            case "+0b" or "+0B":
                return new SnbtByte(0);
        }

        var rawSpan = raw.AsSpan();

        // Determine base prefix
        var signOffset = (rawSpan[0] is '+' or '-') ? 1 : 0;
        var afterSign = rawSpan[signOffset..];

        if (afterSign.Length == 0)
            throw new FormatException($"Invalid numeric literal '{raw}'.");

        var isHex = afterSign.StartsWith("0x", StringComparison.OrdinalIgnoreCase);
        var isBin = !isHex && afterSign.StartsWith("0b", StringComparison.OrdinalIgnoreCase) && afterSign.Length > 2;

        // Validate and clean underscores per SNBT specification
        var cleanSpan = raw.Length <= MaxStackAlloc ? stackalloc char[raw.Length] : new char[raw.Length];
        var cleanLen = CleanAndValidateUnderscores(rawSpan, cleanSpan, isHex, isBin);
        ReadOnlySpan<char> span = cleanSpan[..cleanLen];

        // Routing: Hex and Binary are STRICTLY integers. Decimal numbers can be Float/Double.
        if (isHex || isBin)
            return ParseInteger(span, isHex, isBin);

        var last = char.ToLowerInvariant(span[^1]);
        var hasFloatChars = span.Contains('.') || span.Contains('e') || span.Contains('E');

        if (hasFloatChars || last is 'f' or 'd')
            return ParseFloat(span, last);

        // Integer parsing (decimal, hex, binary)
        return ParseInteger(span, isHex, isBin);
    }

    private static int CleanAndValidateUnderscores(ReadOnlySpan<char> source, Span<char> destination, bool isHex, bool isBin)
    {
        var destIdx = 0;
        for (var i = 0; i < source.Length; i++)
        {
            var c = source[i];
            if (c != '_')
            {
                destination[destIdx++] = c;
                continue;
            }

            // Underscore cannot be at the start or end of the literal
            if (i == 0 || i == source.Length - 1)
                throw new FormatException($"Underscore cannot be at the start or end of numeric literal '{source.ToString()}'.");

            // Character before underscore must be a valid digit (or another underscore in a sequence)
            var prev = source[i - 1];
            if (prev != '_' && !IsValidDigit(prev, isHex, isBin))
                throw new FormatException($"Underscore must be placed between sequences of digits in '{source.ToString()}'.");

            // Character after underscore must be a valid digit (or another underscore in a sequence)
            var next = source[i + 1];
            if (next != '_' && !IsValidDigit(next, isHex, isBin))
                throw new FormatException($"Underscore must be placed between sequences of digits in '{source.ToString()}'.");
        }

        return destIdx;
    }

    private static bool IsValidDigit(char c, bool isHex, bool isBin)
    {
        if (isBin) return c is '0' or '1';
        return isHex ?
            char.IsAsciiHexDigit(c)
            : char.IsAsciiDigit(c);
    }

    private static ISnbtNode ParseFloat(ReadOnlySpan<char> span, char lastChar)
    {
        var valueSpan = span;
        if (lastChar is 'f' or 'd')
            valueSpan = span[..^1];

        if (valueSpan.Length == 0)
            throw new FormatException($"Invalid floating-point literal '{span.ToString()}'.");

        if (lastChar == 'f')
        {
            var val = float.Parse(valueSpan, NumberStyles.Float, CultureInfo.InvariantCulture);
            return !float.IsFinite(val)
                ? throw new OverflowException($"Float value '{span.ToString()}' overflows single-precision bounds.")
                : new SnbtFloat(val);
        }

        var dVal = double.Parse(valueSpan, NumberStyles.Float, CultureInfo.InvariantCulture);
        return !double.IsFinite(dVal)
            ? throw new OverflowException($"Double value '{span.ToString()}' overflows double-precision bounds.")
            : new SnbtDouble(dVal);
    }

    private static ISnbtNode ParseInteger(ReadOnlySpan<char> span, bool isHex, bool isBin)
    {
        var isNegative = false;
        var startIdx = 0;
        switch (span[0])
        {
            case '-':
                isNegative = true;
                startIdx = 1;
                break;
            case '+':
                startIdx = 1;
                break;
        }

        var valueSpan = span[startIdx..];

        if (isHex || isBin)
            valueSpan = valueSpan[2..]; // Strip 0x or 0b

        if (valueSpan.Length == 0)
            throw new FormatException($"Incomplete numeric literal '{span.ToString()}'.");

        // Validate suffix rules
        var typeSuffix = '\0';
        var signSuffix = '\0';

        var lastChar = char.ToLowerInvariant(valueSpan[^1]);

        switch (lastChar)
        {
            // Signedness suffix alone (e.g. 82u, 30bu) is strictly forbidden per SNBT specification
            case 'u':
                throw new FormatException($"Signedness suffix 'u' must always be followed by a data type suffix in '{span.ToString()}'.");
            // Extract type suffix (b, s, l, i)
            case 'b' or 's' or 'l' or 'i':
            {
                typeSuffix = lastChar;
                valueSpan = valueSpan[..^1];

                // Extract preceding signedness suffix (u, s) if present
                if (valueSpan.Length > 0)
                {
                    var precedingChar = char.ToLowerInvariant(valueSpan[^1]);
                    if (precedingChar is 'u' or 's')
                    {
                        signSuffix = precedingChar;
                        valueSpan = valueSpan[..^1];
                    }
                }

                break;
            }
        }

        if (valueSpan.Length == 0)

            throw new FormatException($"Missing digits in numeric literal '{span.ToString()}'.");

        var isUnsigned = signSuffix == 'u';

        // Unsigned numbers cannot have a negative sign
        if (isUnsigned && isNegative)
            throw new FormatException($"Unsigned integer literal cannot have a negative sign in '{span.ToString()}'.");

        // Parse absolute magnitude into 64-bit integer
        ulong unsignedMagnitude;
        if (isHex)
            unsignedMagnitude = ulong.Parse(valueSpan, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        else if (isBin)
        {
            unsignedMagnitude = 0;
            foreach (var c in valueSpan)
            {
                if (c is not ('0' or '1'))
                    throw new FormatException($"Invalid character '{c}' in binary literal '{span.ToString()}'.");

                checked
                {
                    unsignedMagnitude = (unsignedMagnitude << 1) + (uint)(c - '0');
                }
            }
        }
        else
            unsignedMagnitude = ulong.Parse(valueSpan, NumberStyles.None, CultureInfo.InvariantCulture);

        // Construct AST node with range checks and two's complement handling
        return typeSuffix switch
        {
            'b' => ConstructByte(unsignedMagnitude, isNegative, isUnsigned, span),
            's' => ConstructShort(unsignedMagnitude, isNegative, isUnsigned, span),
            'l' => ConstructLong(unsignedMagnitude, isNegative, isUnsigned, span),
            _ => ConstructInt(unsignedMagnitude, isNegative, isUnsigned, span)
        };
    }

    private static SnbtByte ConstructByte(ulong magnitude, bool isNegative, bool isUnsigned, ReadOnlySpan<char> span) =>
        ConstructIntegerNode<sbyte, byte, SnbtByte>(magnitude, isNegative, isUnsigned, span, "byte", static v => new SnbtByte(v));

    private static SnbtShort ConstructShort(ulong magnitude, bool isNegative, bool isUnsigned, ReadOnlySpan<char> span) =>
        ConstructIntegerNode<short, ushort, SnbtShort>(magnitude, isNegative, isUnsigned, span, "short", static v => new SnbtShort(v));

    private static SnbtInt ConstructInt(ulong magnitude, bool isNegative, bool isUnsigned, ReadOnlySpan<char> span) =>
        ConstructIntegerNode<int, uint, SnbtInt>(magnitude, isNegative, isUnsigned, span, "integer", static v => new SnbtInt(v));

    private static SnbtLong ConstructLong(ulong magnitude, bool isNegative, bool isUnsigned, ReadOnlySpan<char> span) =>
        ConstructIntegerNode<long, ulong, SnbtLong>(magnitude, isNegative, isUnsigned, span, "long", static v => new SnbtLong(v));

    private static TNode ConstructIntegerNode<TSigned, TUnsigned, TNode>(
        ulong magnitude,
        bool isNegative,
        bool isUnsigned,
        ReadOnlySpan<char> span,
        string typeName,
        Func<TSigned, TNode> factory)
        where TSigned : unmanaged, IBinaryInteger<TSigned>, IMinMaxValue<TSigned>
        where TUnsigned : unmanaged, IBinaryInteger<TUnsigned>, IMinMaxValue<TUnsigned>
    {
        // Unsigned branch: [0, TUnsigned.MaxValue]
        if (isUnsigned)
        {
            var maxUnsigned = ulong.CreateTruncating(TUnsigned.MaxValue);
            if (magnitude > maxUnsigned)
                throw new OverflowException(
                    $"Value '{span}' overflows unsigned {typeName} range [0, {maxUnsigned}].");

            // Reinterpret/cast bit-pattern from unsigned to signed representation (e.g. 255u8 -> -1i8)
            var value = (TSigned)(dynamic)magnitude;
            return factory(value);
        }

        // Negative branch: [-TSigned.MinValue, 0]
        // Magnitude of MinValue is e.g. 128 for sbyte, 2147483648 for int, 9223372036854775808 for long
        var minMaxValue = TSigned.MinValue;
        var minMagnitude = ulong.CreateTruncating(Unsafe.As<TSigned, TUnsigned>(ref Unsafe.AsRef(in minMaxValue)));

        if (isNegative)
        {
            if (magnitude > minMagnitude)
                throw new OverflowException(
                    $"Value '{span}' underflows signed {typeName} range [{minMaxValue}, {TSigned.MaxValue}].");

            // Handles edge case MinValue (e.g., -128, -9223372036854775808) safely via unchecked negation
            var negated = unchecked(-TSigned.CreateTruncating(magnitude));
            return factory(negated);
        }

        // Positive signed branch: [0, TSigned.MaxValue]
        var maxSigned = ulong.CreateTruncating(TSigned.MaxValue);
        if (magnitude > maxSigned)
            throw new OverflowException(
                $"Value '{span}' overflows signed {typeName} range [{minMaxValue}, {TSigned.MaxValue}].");

        return factory(TSigned.CreateTruncating(magnitude));
    }
}