using BacapGenerator.Models.Interfaces;
using Core.Advancements.Models;

namespace BacapGenerator.Models.Advancements;

/// <summary>
/// Represents an unparseable or broken advancement file.
/// </summary>
public class InvalidAdvancement : ManagedAdvancement
{
    /// <summary>
    /// Gets the possibly parsed advancement model, or <see langword="null"/> if parsing failed completely.
    /// </summary>
    public Advancement? Advancement { get; }

    /// <summary>
    /// Gets the validation error that caused this file to be marked invalid.
    /// </summary>
    public AdvancementValidationError ErrorReason { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidAdvancement"/> class.
    /// </summary>
    /// <param name="file">The physical file information.</param>
    /// <param name="advancement">The partially parsed advancement data, or <see langword="null"/>.</param>
    /// <param name="datapack">The owning datapack.</param>
    /// <param name="errorReason">The reason why the advancement failed validation.</param>
    public InvalidAdvancement(
        FileInfo file,
        Advancement? advancement,
        IReadOnlyDatapack datapack,
        AdvancementValidationError errorReason)
        : base(file, datapack)
    {
        Advancement = advancement;
        ErrorReason = errorReason;
    }

    public override string ToString() => $"{GetType().Name}({File.Name}), Error: {ErrorReason}";
}