using BacapGenerator.Models.Interfaces;
using Core.Advancements.Models;

namespace BacapGenerator.Models.Advancements;

/// <summary>
/// Represents a technical advancement used exclusively as an event trigger without display metadata.
/// </summary>
public class TechnicalAdvancement(
    FileInfo file,
    Advancement advancement,
    IReadOnlyDatapack datapack)
    : ValidAdvancement(file, advancement, datapack);