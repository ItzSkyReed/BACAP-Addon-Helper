using System.Text;
using System.Text.Json;
using BacapGenerator.LanguagePacks.Models;
using LanguageFile = BacapGenerator.LanguagePacks.Models.LanguageFile;

namespace BacapGenerator.LanguagePacks.Services;

/// <summary>
/// Service responsible for loading and parsing language files from a resource pack directory.
/// </summary>
public static class LanguagePackLoader
{
    private static readonly JsonDocumentOptions JsonOptions = new()
    {
        CommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    /// <summary>
    /// Scans and loads all language files located under <c>assets/minecraft/lang/</c> in the specified resource pack.
    /// </summary>
    /// <param name="packRootPath">The filesystem path to the language pack root folder.</param>
    /// <returns>A populated <see cref="LanguagePack"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="packRootPath"/> is null or empty.</exception>
    /// <exception cref="DirectoryNotFoundException">Thrown when the language directory does not exist.</exception>
    /// <example>
    /// <code>
    /// LanguagePack pack = LanguagePackLoader.Load("./resourcepacks/Enhanced_Discoveries_Language_Pack");
    /// </code>
    /// </example>
    public static LanguagePack Load(string packRootPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(packRootPath);

        var rootDir = new DirectoryInfo(packRootPath);
        var langDir = Path.Combine(rootDir.FullName, "assets", "minecraft", "lang");

        if (!Directory.Exists(langDir))
            throw new DirectoryNotFoundException($"Language directory not found at '{langDir}'.");

        var languageFiles = new List<LanguageFile>();

        foreach (var filePath in Directory.EnumerateFiles(langDir, "*.json"))
        {
            var fileName = Path.GetFileNameWithoutExtension(filePath);

            // Skip template base translation file
            if (string.Equals(fileName, "base_translation", StringComparison.OrdinalIgnoreCase))
                continue;

            var parts = fileName.Split('_', 2);
            if (parts.Length != 2)
                continue;

            var major = parts[0].ToLowerInvariant();
            var minor = parts[1].ToLowerInvariant();
            var translations = ParseTranslations(filePath);

            languageFiles.Add(new LanguageFile
            {
                MajorLanguageGroup = major,
                MinorLanguageGroup = minor,
                File = new FileInfo(filePath),
                Translations = translations
            });
        }

        return new LanguagePack(rootDir, languageFiles);
    }

    /// <summary>
    /// Parses key-value translation mappings from a JSON file, tolerating '#' and '//' comments.
    /// </summary>
    private static Dictionary<string, string> ParseTranslations(string filePath)
    {
        var lines = File.ReadAllLines(filePath, Encoding.UTF8);
        var sb = new StringBuilder();

        foreach (var line in lines)
        {
            var trimmed = line.TrimStart();
            // Convert '#' comments to standard '//' comments for System.Text.Json
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
                return translations;

            foreach (var property in doc.RootElement.EnumerateObject())
            {
                translations[property.Name] = property.Value.GetString() ?? string.Empty;
            }
        }
        catch (JsonException)
        {
            // Return whatever was successfully collected or empty on invalid syntax
        }

        return translations;
    }
}