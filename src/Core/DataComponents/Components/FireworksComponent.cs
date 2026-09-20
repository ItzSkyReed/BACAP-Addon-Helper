 using Core.DataComponents.Models;
using Core.SNBT;
using Core.SNBT.Nodes;

using Core.SNBT.Interfaces;
using JetBrains.Annotations;

using Core.DataComponents.Interfaces;

namespace Core.DataComponents.Components;

/// <summary>
/// Represents the flight duration and explosion payload of a firework rocket (<c>minecraft:fireworks</c>).
/// </summary>
/// <param name="FlightDuration">The flight duration of the rocket in arbitrary units (typically 1 to 3). Defaults to 1.</param>
/// <param name="Explosions">Optional list of firework explosion effects caused upon detonation (up to 256 entries).</param>
[UsedImplicitly]
public record FireworksComponent(
    sbyte FlightDuration = 1,
    List<FireworkExplosion>? Explosions = null
) : ICompoundComponent<FireworksComponent>
{
    /// <inheritdoc/>
    public static string ComponentId => "minecraft:fireworks";

    /// <summary>
    /// Parses a <see cref="FireworksComponent"/> from an SNBT node representation.
    /// </summary>
    /// <param name="compound">The SNBT node to parse, which must be an <see cref="SnbtCompound"/>.</param>
    /// <returns>A populated <see cref="FireworksComponent"/> instance.</returns>
    /// <example>
    /// <code>
    /// var node = SnbtParser.Parse("{flight_duration: 2b, explosions: [{shape: \"star\", colors: [I; 16711680]}]}");
    /// var component = FireworksComponent.Parse(node);
    /// </code>
    /// </example>
    public static FireworksComponent Parse(SnbtCompound compound)
    {

        List<FireworkExplosion>? explosions = null;
        if (compound.GetNode("explosions") is SnbtList list)
        {
            explosions = new List<FireworkExplosion>(list.Items.Count);
            foreach (var item in list.Items)
            {
                if (item is SnbtCompound explosionComp)
                {
                    explosions.Add(FireworkExplosion.Parse(explosionComp));
                }
            }
        }

        return new FireworksComponent(
            FlightDuration: (sbyte)compound.GetInt("flight_duration", 1),
            Explosions: explosions
        );
    }

    /// <summary>
    /// Serializes the component into an SNBT compound node.
    /// </summary>
    /// <returns>An <see cref="ISnbtNode"/> representing the fireworks compound.</returns>
    public ISnbtNode ToSnbt()
    {
        var builder = Snbt.Compound()
            .PutOptional("flight_duration", FlightDuration, 1);

        if (Explosions is { Count: > 0 })
        {
            builder.PutList("explosions", list =>
            {
                foreach (var explosion in Explosions)
                {
                    list.Add(explosion.ToSnbt());
                }
            });
        }

        return builder.Build();
    }
}