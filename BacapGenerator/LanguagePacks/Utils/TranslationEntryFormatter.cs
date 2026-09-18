using System.Text.Json;
using Core.Serialization;

namespace BacapGenerator.LanguagePacks.Utils;

/// <summary>
/// Utility helper for formatting translation key entries as JSON lines or template comments.
/// </summary>
public static class TranslationEntryFormatter
{
    /// <summary>
    /// Formats a sequence of keys into JSON property lines with empty values and trailing commas.
    /// </summary>
    /// <param name="keys">The collection of translation keys to format.</param>
    /// <param name="indent">The indentation prefix to apply to each line.</param>
    /// <param name="asComment">If <see langword="true"/>, prefixes the line with <c>//</c>.</param>
    /// <returns>An enumerable of formatted entry lines.</returns>
    /// <example>
    /// <code>
    /// IEnumerable&lt;string&gt; lines = TranslationEntryFormatter.FormatEntries(keys, indent: "    ", asComment: true);
    /// </code>
    /// </example>
    public static IEnumerable<string> FormatEntries(
        IReadOnlyCollection<string> keys,
        string indent = "    ",
        bool asComment = false)
    {
        ArgumentNullException.ThrowIfNull(keys);

        var total = keys.Count;
        var index = 0;
        var prefix = asComment ? $"{indent}//" : indent;

        foreach (var key in keys)
        {
            index++;
            var isLast = index == total;
            var serializedKey = JsonSerializer.Serialize(key, MinecraftDatapackJsonOptions.BaseTranslation);
            var comma = isLast ? string.Empty : ",";

            yield return $"{prefix}{serializedKey}: \"\"{comma}";
        }
    }
}