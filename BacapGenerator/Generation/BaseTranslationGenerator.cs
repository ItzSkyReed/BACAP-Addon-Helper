using System.Text;
using System.Text.Json;
using BacapGenerator.Datapacks.Models;
using BacapGenerator.LanguagePack.Services;
using Core.Serialization;

namespace BacapGenerator.Generation;

/// <summary>
/// Service responsible for generating and synchronizing the special <c>base_translation.json</c> template file
/// containing configuration header comments and unpopulated key-value entries.
/// </summary>
public sealed class BaseTranslationGenerator
{
    private static readonly UTF8Encoding Utf8NoBom = new(encoderShouldEmitUTF8Identifier: false);

    /// <summary>
    /// Generates or overwrites <c>base_translation.json</c> inside the language pack's directory,
    /// merging keys from the main datapack and any related compatibility addons.
    /// </summary>
    /// <param name="datapack">The primary addon datapack containing language pack settings.</param>
    /// <param name="compatibilityAddons">Optional collection of compatibility addons belonging to this parent datapack.</param>
    /// <returns>The <see cref="FileInfo"/> representing the written <c>base_translation.json</c> file.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="datapack"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">Thrown when language pack configuration or path is missing.</exception>
    /// <example>
    /// <code>
    /// var generator = new BaseTranslationGenerator();
    /// FileInfo file = generator.Generate(mainAddon, compatibilityAddons);
    /// </code>
    /// </example>
    public FileInfo Generate(Datapack datapack, IEnumerable<Datapack>? compatibilityAddons = null)
    {
        ArgumentNullException.ThrowIfNull(datapack);

        var settings = datapack.Settings.LanguagePackSettigs
            ?? throw new InvalidOperationException($"Datapack '{datapack.ReleaseName}' does not configure language pack settings.");

        if (string.IsNullOrWhiteSpace(settings.Path))
            throw new InvalidOperationException($"Language pack path is not specified for '{datapack.ReleaseName}'.");

        var langDirectory = Path.Combine(settings.Path, "assets", "minecraft", "lang");
        Directory.CreateDirectory(langDirectory);

        var targetFilePath = Path.Combine(langDirectory, "base_translation.json");

        // Build scan sequence: primary pack first, then any compatibility addons
        var packsToScan = new List<Datapack> { datapack };
        if (compatibilityAddons is not null)
            packsToScan.AddRange(compatibilityAddons);

        var keys = TranslationKeyDiscoveryService.DiscoverKeys(packsToScan);

        using (var stream = new FileStream(targetFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
        using (var writer = new StreamWriter(stream, Utf8NoBom))
        {
            writer.WriteLine("{");

            // Write Header comments if specified
            if (!string.IsNullOrWhiteSpace(settings.BaseTranslationHeader))
                WriteHeaderComments(writer, settings.BaseTranslationHeader);

            // Write key-value pairs
            WriteEntries(writer, keys);

            writer.WriteLine("}");
        }

        return new FileInfo(targetFilePath);
    }

    private static void WriteHeaderComments(TextWriter writer, string rawHeader)
    {
        using var reader = new StringReader(rawHeader);

        while (reader.ReadLine() is { } line)
        {
            var trimmed = line.TrimStart();

            if (string.IsNullOrWhiteSpace(trimmed))
            {
                writer.WriteLine();
                continue;
            }

            writer.WriteLine(trimmed.StartsWith("//", StringComparison.Ordinal)
                ? line
                : $"// {line}");
        }

        writer.WriteLine();
    }

    private static void WriteEntries(TextWriter writer, IReadOnlyCollection<string> keys)
    {
        var total = keys.Count;
        var index = 0;

        foreach (var key in keys)
        {
            index++;
            var isLast = index == total;

            var serializedKey = JsonSerializer.Serialize(key, MinecraftDatapackJsonOptions.BaseTranslation);
            var comma = isLast ? string.Empty : ",";

            writer.WriteLine($"    {serializedKey}: \"\"{comma}");
        }
    }
}