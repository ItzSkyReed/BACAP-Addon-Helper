using Core.DataComponents.Models.Interfaces;
using Core.SNBT;
using Core.SNBT.Nodes;

using Core.SNBT.Interfaces;

namespace Core.DataComponents.Models;

/// <summary>
/// Represents an unamplified status effect applied when consuming a suspicious stew.
/// </summary>
/// <param name="Id">The status effect resource identifier (e.g. <c>minecraft:night_vision</c>).</param>
/// <param name="Duration">The effect duration in ticks. Defaults to 160 ticks (8 seconds).</param>
public record SuspiciousStewEffect(
    string Id,
    int Duration = 160
): ICompoundModel<SuspiciousStewEffect>
{
    /// <summary>
    /// Parses a <see cref="SuspiciousStewEffect"/> from an SNBT compound node.
    /// </summary>
    /// <param name="compound">The SNBT compound containing effect parameters.</param>
    /// <returns>A populated <see cref="SuspiciousStewEffect"/> instance.</returns>
    public static SuspiciousStewEffect Parse(SnbtCompound compound)
    {
        return new SuspiciousStewEffect(
            Id: compound.GetString("id"),
            Duration: compound.GetInt("duration", 160)
        );
    }

    /// <summary>
    /// Serializes the effect into an SNBT compound node.
    /// </summary>
    /// <returns>An <see cref="ISnbtNode"/> representing the effect compound.</returns>
    public ISnbtNode ToSnbt() => Snbt.Compound()
        .Put("id", Id)
        .PutOptional("duration", Duration, 160)
        .Build();

    /// <summary>
    /// Implicitly converts an effect identifier string into a <see cref="SuspiciousStewEffect"/> with default 160 ticks duration.
    /// </summary>
    /// <param name="id">The effect resource location identifier.</param>
    public static implicit operator SuspiciousStewEffect(string id) => new(id);
}