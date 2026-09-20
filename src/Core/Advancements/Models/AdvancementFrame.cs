using System.Text.Json.Serialization;

namespace Core.Advancements.Models;

/// <summary>
/// Specifies the visual frame type of the advancement icon.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AdvancementFrame
{
    /// <summary>
    /// A standard task advancement. Displayed with a normal frame.
    /// </summary>
    [JsonStringEnumMemberName("task")] Task,

    /// <summary>
    /// A goal advancement. Displayed with a rounded frame.
    /// </summary>
    [JsonStringEnumMemberName("goal")] Goal,

    /// <summary>
    /// A challenge advancement. Displayed with a spiky frame and plays a special sound when completed.
    /// </summary>
    [JsonStringEnumMemberName("challenge")]
    Challenge
}