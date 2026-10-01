namespace BacapGenerator.Configuration.Exceptions;

/// <summary>
/// Exception thrown when multiple datapacks attempt to use the same document section name.
/// </summary>
public class DuplicateDocumentSectionException(string sectionName, IReadOnlyCollection<string> datapackIds)
    : Exception($"Duplicate document section name '{sectionName}' found in datapacks: {string.Join(", ", datapackIds)}.")
{
    /// <summary>
    /// Gets the section name that caused the conflict.
    /// </summary>
    public string SectionName { get; } = sectionName;

    /// <summary>
    /// Gets the list of datapack IDs that share the conflicting section name.
    /// </summary>
    public IReadOnlyCollection<string> DatapackIds { get; } = datapackIds;
}