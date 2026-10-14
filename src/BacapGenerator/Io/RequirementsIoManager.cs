using System.Text;
using BacapGenerator.DocGen.Requirements.Exceptions;
using JetBrains.Annotations;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.EventEmitters;

using RequirementsMap = System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, string?>>;

namespace BacapGenerator.Io;

/// <summary>
/// Service responsible for file I/O operations related to the web export requirements YAML file.
/// </summary>
public static class RequirementsIoManager
{
    private static readonly UTF8Encoding Utf8NoBom = new(encoderShouldEmitUTF8Identifier: false);

    private static readonly IDeserializer YamlDeserializer = new DeserializerBuilder()
        .IgnoreUnmatchedProperties()
        .Build();

    private static readonly ISerializer YamlSerializer = new SerializerBuilder()
        .ConfigureDefaultValuesHandling(DefaultValuesHandling.Preserve)
        .WithEventEmitter(next => new MultilineLiteralScalarEmitter(next))
        .Build();

    /// <summary>
    /// Synchronizes the target requirements YAML file with the required advancement sections.
    /// Parses the YAML DOM, injects missing advancements and sections in memory, and serializes the clean structure back to disk.
    /// </summary>
    /// <param name="filePath">The path to the target YAML file.</param>
    /// <param name="advancementSections">A map of advancement Minecraft paths to their required documentation section names.</param>
    /// <returns>A tuple containing the number of newly added advancements and newly injected sections.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="filePath"/> is null or whitespace.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="advancementSections"/> is null.</exception>
    /// <exception cref="RequirementsYamlParseException">Thrown when the existing YAML file contains invalid syntax.</exception>
    /// <example>
    /// <code>
    /// var (added, injected) = RequirementsIoManager.SyncRequirementsFile("requirements/bacaped.yaml", sectionsMap);
    /// </code>
    /// </example>
    [PublicAPI]
    public static (int AddedAdvancements, int InjectedSections) SyncRequirementsFile(
        string filePath,
        IReadOnlyDictionary<string, HashSet<string>> advancementSections)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        ArgumentNullException.ThrowIfNull(advancementSections);

        if (advancementSections.Count == 0)
            return (0, 0);

        var rawData = ReadRequirements(filePath);
        var data = new RequirementsMap(rawData, StringComparer.Ordinal);

        var addedAdvancementsCount = 0;
        var injectedSectionsCount = 0;

        foreach (var (mcPath, neededSections) in advancementSections)
        {
            if (!data.TryGetValue(mcPath, out var existingSections))
            {
                var newSections = new Dictionary<string, string?>(StringComparer.Ordinal);
                foreach (var section in neededSections)
                {
                    newSections[section] = null;
                }

                data[mcPath] = newSections;
                addedAdvancementsCount++;
                continue;
            }

            foreach (var section in neededSections)
            {
                if (!existingSections.TryAdd(section, null)) continue;
                injectedSectionsCount++;
            }
        }

        if (addedAdvancementsCount == 0 && injectedSectionsCount == 0)
            return (0, 0);

        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var serializedYaml = FormatWithRootSpacing(YamlSerializer.Serialize(data));
        File.WriteAllText(filePath, serializedYaml, Utf8NoBom);

        return (addedAdvancementsCount, injectedSectionsCount);
    }

    /// <summary>
    /// Reads and deserializes the requirements YAML file into a nested dictionary.
    /// Returns an empty dictionary if the file does not exist or is empty.
    /// </summary>
    /// <param name="filePath">The path to the YAML file to read.</param>
    /// <returns>A dictionary mapping advancement paths to their section descriptions.</returns>
    /// <exception cref="RequirementsYamlParseException">Thrown when the YAML file contains syntax errors.</exception>
    /// <example>
    /// <code>
    /// RequirementsMap requirements = RequirementsIoManager.ReadRequirements("requirements/bacaped.yaml");
    /// </code>
    /// </example>
    [PublicAPI]
    public static RequirementsMap ReadRequirements(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            return [];

        var yamlContent = File.ReadAllText(filePath, Utf8NoBom);
        if (string.IsNullOrWhiteSpace(yamlContent))
            return [];

        try
        {
            // YamlDotNet returns null when the file contains only whitespace or comments
            return YamlDeserializer.Deserialize<RequirementsMap?>(yamlContent) ?? [];
        }
        catch (YamlException ex)
        {
            throw new RequirementsYamlParseException(
                filePath,
                ex.Message,
                ex.Start.Line,
                ex.Start.Column,
                ex);
        }
    }

    /// <summary>
    /// Inserts an empty line between top-level advancement blocks to keep the YAML file clean and readable.
    /// </summary>
    private static string FormatWithRootSpacing(string yaml)
    {
        var sb = new StringBuilder(yaml.Length + 128);
        using var reader = new StringReader(yaml);

        var isFirstRootEntry = true;

        while (reader.ReadLine() is { } line)
        {
            var isRootKeyLine = line.Length > 0 && !char.IsWhiteSpace(line[0]);
            if (isRootKeyLine)
            {
                if (!isFirstRootEntry)
                    sb.AppendLine();

                isFirstRootEntry = false;
            }

            sb.AppendLine(line);
        }

        return sb.ToString();
    }

    /// <summary>
    /// Custom YAML event emitter that formats multiline strings using literal block style (<c>|-</c>)
    /// instead of escaped double-quoted strings.
    /// </summary>
    private sealed class MultilineLiteralScalarEmitter(IEventEmitter nextEmitter) : ChainedEventEmitter(nextEmitter)
    {
        public override void Emit(ScalarEventInfo eventInfo, IEmitter emitter)
        {
            if (eventInfo.Source.Type == typeof(string) &&
                eventInfo.Source.Value is string value &&
                value.Contains('\n'))
            {
                eventInfo.Style = ScalarStyle.Literal;
            }

            base.Emit(eventInfo, emitter);
        }
    }
}