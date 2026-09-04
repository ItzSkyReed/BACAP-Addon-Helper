namespace BacapGenerator.Models.Datapacks.Settings;

/// <summary>
/// Provides extension methods for the <see cref="DatapackId"/> enumeration.
/// </summary>
public static class DatapackIdExtensions
{
    /// <summary>
    /// Converts the <see cref="DatapackId"/> to its corresponding configuration key string representation.
    /// </summary>
    /// <param name="id">The datapack identifier.</param>
    /// <returns>The standardized lowercase string representation (e.g., "bacaped_hardcore").</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when an unknown <see cref="DatapackId"/> is encountered.</exception>
    /// <example>
    /// <code>
    /// string key = DatapackId.BacapedHardcore.ToConfigKey(); // "bacaped_hardcore"
    /// </code>
    /// </example>
    public static string ToConfigKey(this DatapackId id) => id switch
    {
        DatapackId.Bacap => "bacap",
        DatapackId.Bacaped => "bacaped",
        DatapackId.BacapedHardcore => "bacaped_hardcore",
        _ => throw new ArgumentOutOfRangeException(nameof(id), id, "Unsupported DatapackId value.")
    };

    /// <summary>
    /// Converts the <see cref="DatapackId"/> to its corresponding configuration key string representation.
    /// </summary>
    /// <param name="id">The datapack identifier.</param>
    /// <returns>The standardized lowercase string representation (e.g., "bacaped_hardcore").</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when an unknown <see cref="DatapackId"/> is encountered.</exception>
    /// <example>
    /// <code>
    /// string key = DatapackId.BacapedHardcore.ToConfigKey(); // "bacaped_hardcore"
    /// </code>
    /// </example>
    public static string ToDisplayName(this DatapackId id) => id switch
    {
        DatapackId.Bacap => "Bacap",
        DatapackId.Bacaped => "Bacaped",
        DatapackId.BacapedHardcore => "Bacaped Hardcore",
        _ => throw new ArgumentOutOfRangeException(nameof(id), id, "Unsupported DatapackId value.")
    };
}