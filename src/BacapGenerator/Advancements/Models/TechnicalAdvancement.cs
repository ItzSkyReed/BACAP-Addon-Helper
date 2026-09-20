using BacapGenerator.Datapacks.Models;
using Core.Advancements.Models;

namespace BacapGenerator.Advancements.Models;

/// <summary>
/// Represents a technical advancement used exclusively as an event trigger without display metadata.
/// </summary>
public class TechnicalAdvancement(
    FileInfo file,
    Advancement advancement,
    IReadOnlyDatapack datapack)
    : ValidAdvancement(file, advancement, datapack);