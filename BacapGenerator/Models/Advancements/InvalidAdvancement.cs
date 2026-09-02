using BacapGenerator.Models.Interfaces;
using Core.Advancements.Models;

namespace BacapGenerator.Models.Advancements;

/// <summary>
/// Represents a broken or unparseable advancement file.
/// </summary>
public class InvalidAdvancement : ManagedAdvancement
{
    public AdvancementValidationError ErrorReason { get; init; }

    /// <param name="file">The physical file information.</param>
    /// <param name="advancement"> Advancement parsed data or null if JSON is broken or file not exists</param>
    /// <param name="datapack">Datapack of the advancement</param>
    /// <param name="errorReason">The reason why it is considered invalid.</param>
    public InvalidAdvancement(FileInfo file, Advancement? advancement, IReadOnlyDatapack datapack, AdvancementValidationError errorReason) : base(
        file, advancement,
        datapack)
    {
        ErrorReason = errorReason;
    }

    public override string ToString() => $"{GetType().Name}({File}), Error: {ErrorReason}";
}