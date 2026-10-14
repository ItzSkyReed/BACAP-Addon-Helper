using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;
using BacapGenerator.LanguagePacks.Exceptions;
using BacapGenerator.LanguagePacks.Models;
using BacapGenerator.LanguagePacks.Utils;
using JetBrains.Annotations;

namespace BacapGenerator.Io;

/// <summary>
/// Service responsible for handling all file system and disk persistence operations
/// related to resource pack language files (<c>assets/minecraft/lang/</c>).
/// </summary>
public static class LanguagePackIoManager
{
    private const string BaseTranslationFileName = "base_translation.json";
    private const string EmbeddedBaseLanguageResource = "BacapGenerator.Resources.base_language_file.json";
    private const string DontForgetComment = " // <---  dont forget \",\"";
    private static readonly UTF8Encoding Utf8NoBom = new(encoderShouldEmitUTF8Identifier: false);

    private static readonly JsonDocumentOptions JsonOptions = new()
    {
        CommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    private static readonly JsonReaderOptions LineReaderOptions = new()
    {
        CommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    /// <summary>
    /// Reads and extracts all translation keys declared in the embedded BACAP base language template file.
    /// </summary>
    /// <returns>A read-only collection of translation keys defined in the embedded resource.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the embedded resource is missing from the assembly.</exception>
    /// <exception cref="CorruptedLanguageFileException">Thrown when the resource contains syntax errors or invalid JSON structure.</exception>
    [PublicAPI]
    public static IReadOnlyCollection<string> LoadEmbeddedBaseLanguageKeys()
    {
        var assembly = typeof(LanguagePackIoManager).Assembly;
        using var stream = assembly.GetManifestResourceStream(EmbeddedBaseLanguageResource);

        if (stream is null)
        {
            throw new InvalidOperationException(
                $"Embedded resource '{EmbeddedBaseLanguageResource}' was not found. " +
                "Ensure that 'Resources/base_language_file.json' is configured as an <EmbeddedResource> in the project file.");
        }

        using var reader = new StreamReader(stream, Encoding.UTF8);
        var translations = ParseTranslations(reader, EmbeddedBaseLanguageResource);
        return translations.Keys;
    }

    /// <summary>
    /// Scans and loads all language JSON files under <c>assets/minecraft/lang/</c>.
    /// Quarantines corrupted files into <see cref="LanguagePack.CorruptedFiles"/> to prevent data corruption.
    /// </summary>
    /// <param name="packRootPath">The filesystem path to the language resource pack root folder.</param>
    /// <returns>A populated <see cref="LanguagePack"/> instance containing both valid and corrupted file references.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="packRootPath"/> is null or empty.</exception>
    /// <exception cref="DirectoryNotFoundException">Thrown when the target language directory does not exist.</exception>
    [PublicAPI]
    public static LanguagePack LoadPack(string packRootPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(packRootPath);

        var rootDir = new DirectoryInfo(packRootPath);
        var langDirPath = GetLanguageDirectoryPath(rootDir.FullName);

        if (!Directory.Exists(langDirPath))
            throw new DirectoryNotFoundException($"Language directory not found at '{langDirPath}'.");

        var languageFiles = new List<LanguageFile>();
        var corruptedFiles = new List<CorruptedLanguageFileInfo>();

        foreach (var filePath in Directory.EnumerateFiles(langDirPath, "*.json"))
        {
            var fileName = Path.GetFileName(filePath);

            // Skip template base translation file
            if (string.Equals(fileName, BaseTranslationFileName, StringComparison.OrdinalIgnoreCase))
                continue;

            try
            {
                var langFile = LoadLanguageFile(filePath);
                if (langFile is not null)
                    languageFiles.Add(langFile);
            }
            catch (CorruptedLanguageFileException ex)
            {
                corruptedFiles.Add(new CorruptedLanguageFileInfo(
                    new FileInfo(filePath),
                    ex.Message,
                    ex.LineNumber));
            }
        }

        return new LanguagePack(rootDir, languageFiles, corruptedFiles);
    }

    /// <summary>
    /// Loads and strictly parses an individual language file from disk.
    /// </summary>
    /// <param name="filePath">The absolute or relative path to the language JSON file.</param>
    /// <returns>
    /// A parsed <see cref="LanguageFile"/> model, or <see langword="null"/> if the file name format does not match <c>major_minor.json</c>.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="filePath"/> is null or whitespace.</exception>
    /// <exception cref="FileNotFoundException">Thrown when the specified file does not exist on disk.</exception>
    /// <exception cref="CorruptedLanguageFileException">Thrown when the JSON payload contains syntax errors or is malformed.</exception>
    [PublicAPI]
    public static LanguageFile? LoadLanguageFile(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Language file was not found at '{filePath}'.", filePath);

        var fileNameWithoutExt = Path.GetFileNameWithoutExtension(filePath);
        var parts = fileNameWithoutExt.Split('_', 2);
        if (parts.Length != 2)
            return null;

        var major = parts[0].ToLowerInvariant();
        var minor = parts[1].ToLowerInvariant();
        var translations = ParseTranslations(filePath);

        return new LanguageFile
        {
            MajorLanguageGroup = major,
            MinorLanguageGroup = minor,
            File = new FileInfo(filePath),
            Translations = translations
        };
    }

    /// <summary>
    /// Safely updates a language file on disk while preserving user comments and blank lines.
    /// Removes unused keys, normalizes trailing commas on the last property, and appends missing keys as comments.
    /// </summary>
    /// <param name="file">The language file to patch.</param>
    /// <param name="missingKeys">The collection of missing keys to append.</param>
    /// <param name="unusedKeysToRemove">Optional collection of unused keys to delete from the file.</param>
    /// <returns><see langword="true"/> if changes were written to disk; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="CorruptedLanguageFileException">Thrown if the file does not have a valid JSON closing brace.</exception>
    [PublicAPI]
    public static bool UpdateLanguageFile(
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
        var closingBraceIndex = EnsureStandaloneClosingBrace(rawLines);

        if (closingBraceIndex < 0)
        {
            throw new CorruptedLanguageFileException(
                file.File.FullName,
                $"Cannot patch '{file.File.Name}' because the closing root brace '}}' could not be found.");
        }

        var modified = false;

        // 1. Remove obsolete unused keys from the file
        if (hasUnused)
        {
            var removeSet = unusedKeysToRemove!.ToHashSet(StringComparer.Ordinal);

            for (var i = closingBraceIndex - 1; i >= 0; i--)
            {
                if (!TryExtractPropertyKey(rawLines[i], out var key) || !removeSet.Contains(key))
                    continue;

                rawLines.RemoveAt(i);
                closingBraceIndex--;
                modified = true;
            }
        }

        // Remove previous "// Missed translations:" block to prevent duplicate accumulation
        if (RemoveExistingMissedSection(rawLines, ref closingBraceIndex))
            modified = true;

        // Locate the last actual JSON property line and normalize its trailing comma / reminder comment
        var lastPropertyIndex = FindLastPropertyIndex(rawLines, closingBraceIndex);
        var indent = "    ";

        if (lastPropertyIndex >= 0)
        {
            var line = rawLines[lastPropertyIndex];
            var leadingSpaces = line.Length - line.TrimStart().Length;
            if (leadingSpaces > 0)
                indent = line[..leadingSpaces];

            var updatedLastLine = FormatLastPropertyLine(line, hasMissingKeys: hasMissing);
            if (!string.Equals(line, updatedLastLine, StringComparison.Ordinal))
            {
                rawLines[lastPropertyIndex] = updatedLastLine;
                modified = true;
            }
        }

        // Append new "// Missed translations:" block if needed
        if (hasMissing)
        {
            var entriesToInsert = new List<string>(missingKeys.Count + 1)
            {
                $"{indent}// Missed translations:"
            };

            entriesToInsert.AddRange(TranslationEntryFormatter.FormatEntries(missingKeys, indent, asComment: true));

            rawLines.InsertRange(closingBraceIndex, entriesToInsert);
            modified = true;
        }

        if (!modified)
            return false;

        File.WriteAllLines(file.File.FullName, rawLines, Utf8NoBom);
        file.File.Refresh();

        return true;
    }

    /// <summary>
    /// Writes or overwrites the <c>base_translation.json</c> template file inside the resource pack.
    /// </summary>
    /// <param name="packRootPath">The filesystem path to the language resource pack root folder.</param>
    /// <param name="jsonContent">The JSON text content to write.</param>
    /// <returns>A <see cref="FileInfo"/> descriptor of the written file.</returns>
    /// <exception cref="ArgumentException">Thrown when arguments are null or whitespace.</exception>
    [PublicAPI]
    public static FileInfo SaveBaseTranslation(string packRootPath, string jsonContent)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(packRootPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(jsonContent);

        var langDir = GetLanguageDirectoryPath(packRootPath);
        Directory.CreateDirectory(langDir);

        var targetPath = Path.Combine(langDir, BaseTranslationFileName);
        File.WriteAllText(targetPath, jsonContent, Utf8NoBom);

        var fileInfo = new FileInfo(targetPath);
        fileInfo.Refresh();
        return fileInfo;
    }

    /// <summary>
    /// Deletes a language file from disk and refreshes its descriptor state.
    /// </summary>
    /// <param name="file">The language file to delete.</param>
    [PublicAPI]
    public static void DeleteLanguageFile(LanguageFile? file)
    {
        if (file?.File is null || !File.Exists(file.File.FullName))
            return;

        file.File.Delete();
        file.File.Refresh();
    }

    /// <summary>
    /// Resolves the canonical path to the language directory (<c>assets/minecraft/lang</c>) within a pack.
    /// </summary>
    /// <param name="packRootPath">The filesystem path to the resource pack root directory.</param>
    /// <returns>The combined path to the language folder.</returns>
    [PublicAPI]
    public static string GetLanguageDirectoryPath(string packRootPath) =>
        Path.Combine(packRootPath, "assets", "minecraft", "lang");

    private static Dictionary<string, string> ParseTranslations(string filePath)
    {
        using var reader = new StreamReader(filePath, Encoding.UTF8);
        return ParseTranslations(reader, Path.GetFileName(filePath));
    }

    /// <summary>
    /// Parses translations from a stream reader, sanitizing '#' comments to '//' for System.Text.Json compatibility.
    /// </summary>
    private static Dictionary<string, string> ParseTranslations(TextReader reader, string sourceIdentifier)
    {
        var sb = new StringBuilder();

        while (reader.ReadLine() is { } line)
        {
            var trimmed = line.TrimStart();
            if (trimmed.StartsWith('#'))
                sb.Append("//").AppendLine(trimmed[1..]);
            else
                sb.AppendLine(line);
        }

        var translations = new Dictionary<string, string>(StringComparer.Ordinal);

        try
        {
            using var doc = JsonDocument.Parse(sb.ToString(), JsonOptions);
            if (doc.RootElement.ValueKind != JsonValueKind.Object)
            {
                throw new CorruptedLanguageFileException(
                    sourceIdentifier,
                    $"Root JSON structure in '{sourceIdentifier}' must be an object, but found {doc.RootElement.ValueKind}.");
            }

            foreach (var property in doc.RootElement.EnumerateObject())
            {
                translations[property.Name] = property.Value.GetString() ?? string.Empty;
            }

            return translations;
        }
        catch (JsonException ex)
        {
            throw new CorruptedLanguageFileException(
                sourceIdentifier,
                $"JSON syntax error in '{sourceIdentifier}' at line {ex.LineNumber}, pos {ex.BytePositionInLine}: {ex.Message}",
                ex.LineNumber,
                ex.BytePositionInLine,
                ex);
        }
    }

    /// <summary>
    /// Extracts the JSON property key from a single line using <see cref="Utf8JsonReader"/>.
    /// Safely handles all JSON escape sequences (<c>\"</c>, <c>\\</c>, <c>\uXXXX</c>) and trailing comments.
    /// </summary>
    private static bool TryExtractPropertyKey(string line, [NotNullWhen(true)] out string? key)
    {
        key = null;
        var trimmed = line.AsSpan().TrimStart();

        if (!trimmed.StartsWith("\""))
            return false;

        // Wrap the single line in braces so Utf8JsonReader treats it as a valid JSON object property
        var wrappedJson = Encoding.UTF8.GetBytes($"{{{trimmed}}}");
        var reader = new Utf8JsonReader(wrappedJson, LineReaderOptions);

        try
        {
            if (reader.Read() && reader.TokenType == JsonTokenType.StartObject &&
                reader.Read() && reader.TokenType == JsonTokenType.PropertyName)
            {
                key = reader.GetString();
                return key is not null;
            }
        }
        catch (JsonException)
        {
            // Line is not a valid JSON property
        }

        return false;
    }

    /// <summary>
    /// Finds the closing root brace <c>}</c> and, if it is sharing a line with a property
    /// (e.g. <c>"key": "val" }</c>), splits <c>}</c> onto its own line so insertions never corrupt syntax.
    /// </summary>
    private static int EnsureStandaloneClosingBrace(List<string> lines)
    {
        for (var i = lines.Count - 1; i >= 0; i--)
        {
            var trimmed = lines[i].AsSpan().Trim();
            if (trimmed.IsEmpty || trimmed.StartsWith("//") || trimmed.StartsWith("#"))
                continue;

            var bracePos = lines[i].LastIndexOf('}');
            if (bracePos < 0)
                continue;

            var beforeBrace = lines[i][..bracePos];
            if (string.IsNullOrWhiteSpace(beforeBrace))
                return i;

            lines[i] = beforeBrace.TrimEnd();
            lines.Insert(i + 1, "}");
            return i + 1;

        }

        return -1;
    }

    /// <summary>
    /// Formats the last property line by stripping old reminder comments, removing trailing commas when no missing keys follow,
    /// or attaching the <see cref="DontForgetComment"/> reminder when missing keys are appended.
    /// </summary>
    private static string FormatLastPropertyLine(string line, bool hasMissingKeys)
    {
        var trimmedEnd = line.TrimEnd();

        // Strip previous reminder comment if present
        if (trimmedEnd.EndsWith(DontForgetComment, StringComparison.OrdinalIgnoreCase))
        {
            trimmedEnd = trimmedEnd[..^DontForgetComment.Length].TrimEnd();
        }

        if (hasMissingKeys)
        {
            return trimmedEnd.EndsWith(',')
                ? trimmedEnd
                : $"{trimmedEnd}{DontForgetComment}";
        }

        // No missing keys: ensure the last property in JSON does not have a trailing comma
        return trimmedEnd.EndsWith(',')
            ? trimmedEnd[..^1]
            : trimmedEnd;
    }

    private static bool RemoveExistingMissedSection(List<string> lines, ref int closingBraceIndex)
    {
        var missedHeaderIndex = -1;

        for (var i = 0; i < closingBraceIndex; i++)
        {
            if (!lines[i].AsSpan().Trim().StartsWith("// Missed translations:", StringComparison.OrdinalIgnoreCase))
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
            if (TryExtractPropertyKey(lines[i], out _))
                return i;
        }

        return -1;
    }
}