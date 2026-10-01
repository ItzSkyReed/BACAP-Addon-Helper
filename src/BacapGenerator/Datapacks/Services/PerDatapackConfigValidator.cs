using BacapGenerator.Configuration.Exceptions;
using BacapGenerator.Datapacks.Models;
using JetBrains.Annotations;

namespace BacapGenerator.Datapacks.Services;

/// <summary>
/// Service responsible for validating global values constraints across all loaded datapacks.
/// </summary>
public static class PerDatapackConfigValidator
{
    /// <summary>
    /// Validates cross-datapack unique document generation section names.
    /// </summary>
    /// <param name="datapacks">All loaded datapacks in the registry.</param>
    /// <exception cref="DuplicateDocumentSectionException">Thrown when multiple datapacks use the same section name.</exception>
    [PublicAPI]
    public static void ValidateDocumentGeneratorSections(IEnumerable<Datapack> datapacks)
    {
        ArgumentNullException.ThrowIfNull(datapacks);

        var conflictingSections = datapacks
            .Where(dp => dp.Settings.DocumentGeneratorSettings?.Enabled == true)
            .GroupBy(dp => dp.Settings.DocumentGeneratorSettings!.SectionName, StringComparer.OrdinalIgnoreCase)
            .Where(g => g.Count() > 1)
            .ToList();

        if (conflictingSections.Count <= 0)
            return;

        var conflict = conflictingSections.First();
        var sectionName = conflict.Key;
        var conflictingDatapackIds = conflict.Select(dp => dp.Id).ToArray();

        throw new DuplicateDocumentSectionException(sectionName, conflictingDatapackIds);
    }
}