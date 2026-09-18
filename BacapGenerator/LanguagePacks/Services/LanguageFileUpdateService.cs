using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;
using BacapGenerator.LanguagePacks.Models;
using BacapGenerator.LanguagePacks.Utils;

namespace BacapGenerator.LanguagePacks.Services;

/// <summary>
/// Service responsible for patching language JSON files by appending missing translations
/// and optionally removing obsolete keys without disturbing existing headers or formatting.
/// </summary>
public static class LanguageFileUpdateService
{
    private const string DontForgetComment = " // <---  dont forget \",\"";
    private static readonly UTF8Encoding Utf8NoBom = new(encoderShouldEmitUTF8Identifier: false);

    /// <summary>
    /// Updates a language file on disk by removing obsolete unused keys and appending missing translation keys
    /// as commented-out template entries, adding a reminder comment instead of a trailing comma.
    /// </summary>
    /// <param name="file">The language file to patch.</param>
    /// <param name="missingKeys">The collection of missing keys to append.</param>
    /// <param name="unusedKeysToRemove">Optional collection of unused keys to delete from the file.</param>
    /// <returns><see langword="true"/> if the file was modified on disk; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="file"/> or <paramref name="missingKeys"/> is <see langword="null"/>.</exception>
    /// <example>
    /// <code>
    /// bool modified = LanguageFileUpdateService.UpdateFile(langFile, missingKeys, unusedKeys);
    /// </code>
    /// </example>
    public static bool UpdateFile(
        LanguageFile file,
        IReadOnlyCollection<string> missingKeys,
        IReadOnlyCollection<string>? unusedKeysToRemove = null)
    {
        ArgumentNullException.ThrowIfNull(file);
        ArgumentNullException.ThrowIfNull(missingKeys);

        var hasMissing = missingKeys.Count > 0;
        var hasUnused = unusedKeysToRemove is { Count: > 0 };

        if (!hasMissing && !hasUnused)
            return false;

        var rawLines = File.ReadAllLines(file.File.FullName, Encoding.UTF8).ToList();
        var closingBraceIndex = FindClosingBraceIndex(rawLines);

        if (closingBraceIndex < 0)
            return false;

        var modified = false;

        // Remove obsolete unused keys from the file
        if (hasUnused)
        {
            var removeSet = unusedKeysToRemove!.ToHashSet(StringComparer.Ordinal);

            for (var i = closingBraceIndex - 1; i >= 0; i--)
            {
                if (!TryExtractJsonKey(rawLines[i], out var key) || !removeSet.Contains(key))
                    continue;

                rawLines.RemoveAt(i);
                closingBraceIndex--;
                modified = true;
            }
        }

        // Remove previous "// Missed translations:" block to prevent duplicate accumulation
        if (RemoveExistingMissedSection(rawLines, ref closingBraceIndex))
            modified = true;

        // Locate the last actual JSON property
        var lastPropertyIndex = FindLastPropertyIndex(rawLines, closingBraceIndex);
        var indent = "    ";

        if (lastPropertyIndex >= 0)
        {
            var line = rawLines[lastPropertyIndex];
            var trimmedLine = line.TrimEnd();

            // Strip previous reminder comment if present
            if (trimmedLine.EndsWith(DontForgetComment, StringComparison.OrdinalIgnoreCase))
            {
                trimmedLine = trimmedLine[..^DontForgetComment.Length].TrimEnd();
                rawLines[lastPropertyIndex] = trimmedLine;
                modified = true;
            }

            var leadingSpaces = line.Length - line.TrimStart().Length;
            if (leadingSpaces > 0)
                indent = line[..leadingSpaces];

            if (hasMissing)
            {
                // Attach reminder comment instead of appending a comma
                if (!trimmedLine.EndsWith(','))
                {
                    rawLines[lastPropertyIndex] = $"{trimmedLine}{DontForgetComment}";
                }

                var entriesToInsert = new List<string>
                {
                    $"{indent}// Missed translations:"
                };

                entriesToInsert.AddRange(TranslationEntryFormatter.FormatEntries(missingKeys, indent, asComment: true));

                rawLines.InsertRange(closingBraceIndex, entriesToInsert);
                modified = true;
            }
            else
            {
                // Clean up trailing comma on the last property if no missing keys are being appended
                if (trimmedLine.EndsWith(','))
                {
                    rawLines[lastPropertyIndex] = trimmedLine[..^1];
                    modified = true;
                }
            }
        }
        else if (hasMissing)
        {
            var entriesToInsert = new List<string>
            {
                $"{indent}// Missed translations:"
            };

            entriesToInsert.AddRange(TranslationEntryFormatter.FormatEntries(missingKeys, indent, asComment: true));

            rawLines.InsertRange(closingBraceIndex, entriesToInsert);
            modified = true;
        }

        if (modified)
            File.WriteAllLines(file.File.FullName, rawLines, Utf8NoBom);

        return modified;
    }

    /// <summary>
    /// Backward-compatible alias for appending missing translation keys without removing unused keys.
    /// </summary>
    public static bool AppendMissingKeys(LanguageFile file, IReadOnlyCollection<string> missingKeys) =>
        UpdateFile(file, missingKeys);

    private static bool TryExtractJsonKey(string line, [NotNullWhen(true)] out string? key)
    {
        key = null;
        var trimmed = line.TrimStart();

        if (!trimmed.StartsWith('"'))
            return false;

        var inEscape = false;
        var quoteEnd = -1;

        for (var i = 1; i < trimmed.Length; i++)
        {
            if (inEscape)
            {
                inEscape = false;
                continue;
            }

            if (trimmed[i] == '\\')
            {
                inEscape = true;
                continue;
            }

            if (trimmed[i] != '"')
                continue;

            quoteEnd = i;
            break;
        }

        if (quoteEnd == -1)
            return false;

        var colonFound = false;
        for (var i = quoteEnd + 1; i < trimmed.Length; i++)
        {
            if (char.IsWhiteSpace(trimmed[i]))
                continue;

            if (trimmed[i] == ':')
                colonFound = true;

            break;
        }

        if (!colonFound)
            return false;

        try
        {
            key = JsonSerializer.Deserialize<string>(trimmed[..(quoteEnd + 1)]);
            return key is not null;
        }
        catch
        {
            return false;
        }
    }

    private static int FindClosingBraceIndex(List<string> lines)
    {
        for (var i = lines.Count - 1; i >= 0; i--)
        {
            if (lines[i].TrimEnd().EndsWith('}'))
                return i;
        }

        return -1;
    }

    private static bool RemoveExistingMissedSection(List<string> lines, ref int closingBraceIndex)
    {
        var missedHeaderIndex = -1;

        for (var i = 0; i < closingBraceIndex; i++)
        {
            if (!lines[i].Trim().StartsWith("// Missed translations:", StringComparison.OrdinalIgnoreCase))
                continue;

            missedHeaderIndex = i;
            break;
        }

        if (missedHeaderIndex < 0)
            return false;

        var countToRemove = closingBraceIndex - missedHeaderIndex;
        lines.RemoveRange(missedHeaderIndex, countToRemove);
        closingBraceIndex -= countToRemove;
        return true;
    }

    private static int FindLastPropertyIndex(List<string> lines, int beforeIndex)
    {
        for (var i = beforeIndex - 1; i >= 0; i--)
        {
            var trimmed = lines[i].Trim();

            if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith('#') || trimmed.StartsWith("//"))
                continue;

            return i;
        }

        return -1;
    }
}