using Core.Advancements.Models;

namespace BacapGenerator.Models.Advancements;

/// <summary>
/// Represents a broken or unparseable advancement file.
/// </summary>
public class InvalidAdvancement : ManagedAdvancement
{
    public string ErrorReason { get; }

    /// <param name="file">The physical file information.</param>
    /// <param name="advancement"> Advancement parsed data or null if JSON is broken or file not exists</param>
    /// <param name="errorReason">The reason why it is considered invalid.</param>
    public InvalidAdvancement(FileInfo file, Advancement? advancement, string errorReason) : base(file, advancement)
    {
        ErrorReason = errorReason;
    }

    public override string ToString() => $"{GetType().Name}({File}), Error: {ErrorReason}";

}