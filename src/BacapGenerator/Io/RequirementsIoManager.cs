using System.Text;
using BacapGenerator.DocGen.Requirements.Exceptions;
using JetBrains.Annotations;
using YamlDotNet.Serialization;

// Обрати внимание: теперь тут string? в самом конце, потому что YAML вернет null для пустых полей
using RequirementsMap = System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, string?>>;

namespace BacapGenerator.Io;

/// <summary>
/// Service responsible for file I/O operations related to the web export requirements YAML file.
/// </summary>
public static class RequirementsIoManager
{
    private static readonly UTF8Encoding Utf8NoBom = new(encoderShouldEmitUTF8Identifier: false);

    [PublicAPI]
    public static (int AddedAdvancements, int InjectedSections) SyncRequirementsFile(
        string filePath,
        IReadOnlyDictionary<string, HashSet<string>> advancementSections)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        ArgumentNullException.ThrowIfNull(advancementSections);

        if (advancementSections.Count == 0)
            return (0, 0);

        var existingKeys = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);
        var lines = new List<string>();

        if (File.Exists(filePath))
        {
            lines = File.ReadAllLines(filePath, Utf8NoBom).ToList();
            var yamlContent = string.Join(Environment.NewLine, lines);

            var deserializer = new DeserializerBuilder()
                .IgnoreUnmatchedProperties()
                .Build();

            try
            {
                var existingData = deserializer.Deserialize<RequirementsMap>(yamlContent);

                foreach (var (mcPath, sections) in existingData)
                    existingKeys[mcPath] = new HashSet<string>(sections.Keys, StringComparer.Ordinal);
            }
            catch (YamlDotNet.Core.YamlException ex)
            {
                throw new RequirementsYamlParseException(
                    filePath,
                    ex.Message,
                    ex.Start.Line,
                    ex.Start.Column,
                    ex);
            }
        }

        var modified = false;
        var addedAdvancementsCount = 0;
        var injectedSectionsCount = 0;
        var completelyMissingAdvancements = new List<string>();

        foreach (var (mcPath, neededSections) in advancementSections)
        {
            if (!existingKeys.TryGetValue(mcPath, out var existingSections))
            {
                completelyMissingAdvancements.Add(mcPath);
                continue;
            }

            var missingSections = neededSections.Where(s => !existingSections.Contains(s)).ToList();

            if (missingSections.Count <= 0)
                continue;

            var keyLineIndex = lines.FindIndex(l => l.Trim() == $"{mcPath}:");

            if (keyLineIndex < 0)
                continue;

            var insertIndex = keyLineIndex + 1;
            foreach (var section in missingSections)
            {
                // ПРОСТО ПИШЕМ КЛЮЧ БЕЗ ЗНАЧЕНИЯ И БЕЗ |-
                lines.Insert(insertIndex++, $"  {section}:");
                injectedSectionsCount++;
            }
            modified = true;
        }

        if (completelyMissingAdvancements.Count > 0)
        {
            if (lines.Count > 0 && !string.IsNullOrWhiteSpace(lines[^1]))
                lines.Add(string.Empty);

            lines.Add($"# Auto-generated stubs (Added: {DateTime.Now:yyyy-MM-dd HH:mm})");
            lines.Add(string.Empty);

            foreach (var mcPath in completelyMissingAdvancements)
            {
                lines.Add($"{mcPath}:");

                lines.AddRange(advancementSections[mcPath].Select(section => $"  {section}:"));

                lines.Add(string.Empty);
                addedAdvancementsCount++;
            }
            modified = true;
        }

        if (!modified)
            return (addedAdvancementsCount, injectedSectionsCount);

        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllLines(filePath, lines, Utf8NoBom);

        return (addedAdvancementsCount, injectedSectionsCount);
    }

    [PublicAPI]
    public static RequirementsMap ReadRequirements(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            return [];

        var yamlContent = File.ReadAllText(filePath, Utf8NoBom);
        var deserializer = new DeserializerBuilder()
            .IgnoreUnmatchedProperties()
            .Build();

        try
        {
            return deserializer.Deserialize<RequirementsMap>(yamlContent);
        }
        catch (YamlDotNet.Core.YamlException ex)
        {
            throw new RequirementsYamlParseException(
                filePath,
                ex.Message,
                ex.Start.Line,
                ex.Start.Column,
                ex);
        }
    }
}