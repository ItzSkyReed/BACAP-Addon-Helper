using System.Collections.Frozen;

namespace BacapGenerator.Utils;

/// <summary>
/// Provides extension methods for string manipulation and formatting.
/// </summary>
public static class StringExtensions
{
    public static readonly FrozenSet<string> DefaultMinorWords = new[]
    {
        "a", "an", "the",
        "and", "but", "or", "nor", "for", "yet", "so", "vs",
        "as", "at", "by", "in", "of", "off", "on", "per", "to", "up", "via", "with", "from", "into"
    }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Hardcoded Roman numerals to always preserve their uppercase formatting.
    /// </summary>
    private static readonly FrozenSet<string> RomanNumerals = new[]
    {
        "I", "II", "III", "IV", "V", "VI", "VII", "VIII", "IX", "X", "XI", "XII"
    }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    public static string ToTitleCase(
        this string source,
        FrozenSet<string>? minorWords = null,
        bool preserveContractions = true)
    {
        ArgumentNullException.ThrowIfNull(source);

        if (source.Length == 0)
            return string.Empty;

        var exceptions = minorWords ?? DefaultMinorWords;

        return string.Create(source.Length, (source, exceptions, preserveContractions), static (destination, state) =>
        {
            var (input, minorSet, preserve) = state;
            var minorLookup = minorSet.GetAlternateLookup<ReadOnlySpan<char>>();
            var numeralsLookup = RomanNumerals.GetAlternateLookup<ReadOnlySpan<char>>();

            var i = 0;
            var isFirstWord = true;

            while (i < input.Length)
            {
                // Copy non-letter characters (delimiters, whitespace, symbols)
                if (!char.IsLetter(input[i]))
                {
                    destination[i] = input[i];
                    i++;
                    continue;
                }

                var wordStart = i;
                while (i < input.Length)
                {
                    if (char.IsLetter(input[i]))
                        i++;
                    else if (input[i] == '\'' && i + 1 < input.Length && char.IsLetter(input[i + 1]))
                        i += 2;
                    else
                        break;
                }

                var wordEnd = i;
                var wordSpan = input.AsSpan(wordStart, wordEnd - wordStart);

                if (numeralsLookup.TryGetValue(wordSpan, out var exactNumeral))
                {
                    for (var j = 0; j < exactNumeral.Length; j++)
                        destination[wordStart + j] = exactNumeral[j];

                    isFirstWord = false;
                    continue;
                }

                var isLastWord = !HasLettersAhead(input.AsSpan(wordEnd));
                var shouldBeLower = !isFirstWord && !isLastWord && minorLookup.Contains(wordSpan);

                var capitalizeNext = !shouldBeLower;
                for (var j = wordStart; j < wordEnd; j++)
                {
                    var c = input[j];
                    if (char.IsLetter(c))
                    {
                        destination[j] = capitalizeNext
                            ? char.ToUpperInvariant(c)
                            : char.ToLowerInvariant(c);

                        capitalizeNext = false;
                    }
                    else
                    {
                        destination[j] = c;
                        capitalizeNext = (c != '\'' || !preserve) && !shouldBeLower;
                    }
                }

                isFirstWord = false;
            }
        });
    }

    /// <summary>
    /// Checks whether the remaining slice contains any more letters.
    /// </summary>
    private static bool HasLettersAhead(ReadOnlySpan<char> span)
    {
        foreach (var t in span)
            if (char.IsLetter(t))
                return true;

        return false;
    }
}