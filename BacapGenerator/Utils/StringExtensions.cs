using System.Collections.Frozen;

namespace BacapGenerator.Utils;

/// <summary>
/// Provides extension methods for string manipulation and formatting.
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Default set of minor English words (articles, coordinating conjunctions, short prepositions)
    /// that should remain lowercased in title case unless appearing as the first or last word.
    /// </summary>
    public static readonly FrozenSet<string> DefaultMinorWords = new[]
    {
        // Articles
        "a", "an", "the",
        // Coordinating conjunctions
        "and", "but", "or", "nor", "for", "yet", "so",
        // Short prepositions (<= 4 letters)
        "as", "at", "by", "in", "of", "off", "on", "per", "to", "up", "via", "with", "from", "into"
    }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Converts a string to Title Case while keeping minor exception words in lowercase
    /// unless they are positioned at the start or end of the title.
    /// </summary>
    /// <param name="source">The input string to transform.</param>
    /// <param name="minorWords">
    /// An optional set of minor words to keep in lowercase. If <see langword="null"/>,
    /// <see cref="DefaultMinorWords"/> is used.
    /// </param>
    /// <param name="preserveContractions">
    /// If <see langword="true"/>, prevents capitalizing characters following an internal apostrophe
    /// (e.g., converts <c>"DON'T"</c> to <c>"Don't"</c> instead of <c>"Don'T"</c>).
    /// </param>
    /// <returns>The formatted Title Case string, or <see cref="string.Empty"/> if input is empty.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> is <see langword="null"/>.</exception>
    /// <example>
    /// <code>
    /// string title = "the lord of the rings".ToTitleCase();
    /// // Returns: "The Lord of the Rings"
    ///
    /// string question = "what is it for?".ToTitleCase();
    /// // Returns: "What Is It For?"
    /// </code>
    /// </example>
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
            var lookup = minorSet.GetAlternateLookup<ReadOnlySpan<char>>();

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

                // Identify word token boundaries [wordStart, wordEnd)
                var wordStart = i;
                while (i < input.Length)
                {
                    if (char.IsLetter(input[i]))
                    {
                        i++;
                    }
                    else if (input[i] == '\'' && i + 1 < input.Length && char.IsLetter(input[i + 1]))
                    {
                        // Advance past internal contraction apostrophe (e.g., "don't")
                        i += 2;
                    }
                    else
                    {
                        break;
                    }
                }

                var wordEnd = i;
                var wordSpan = input.AsSpan(wordStart, wordEnd - wordStart);

                // Determine if this word is the final word in the string
                var isLastWord = !HasLettersAhead(input.AsSpan(wordEnd));

                // Minor words stay lowercase only if they are not first or last
                var shouldBeLower = !isFirstWord && !isLastWord && lookup.Contains(wordSpan);

                // Write characters into destination buffer
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
        {
            if (char.IsLetter(t))
                return true;
        }

        return false;
    }
}