using System.ComponentModel;
using BacapGenerator.Converters;

namespace BacapGenerator.Datapacks.Models.Settings.Scoreboards;
/// <summary>
/// Defines filtering criteria for hidden advancements.
/// </summary>
[TypeConverter(typeof(SnakeCaseEnumConverter))]
public enum ScoreboardHiddenFilter
{
    /// <summary>
    /// Includes both regular and hidden advancements.
    /// </summary>
    All,

    /// <summary>
    /// Excludes hidden advancements from generation.
    /// </summary>
    Exclude,

    /// <summary>
    /// Includes exclusively hidden advancements.
    /// </summary>
    Only
}