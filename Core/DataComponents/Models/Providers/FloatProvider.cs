using Core.DataComponents.Models.Interfaces;
using Core.SNBT.Interfaces;
using Core.SNBT.Nodes;
using JetBrains.Annotations;

namespace Core.DataComponents.Models.Providers;

/// <summary>
/// Represents a numeric float provider value that can be defined either as a direct constant or referenced by a namespaced provider ID.
/// </summary>
/// <param name="Value">The constant float value, if defined directly.</param>
/// <param name="ProviderId">The namespaced identifier of the registered number provider, if referenced.</param>
[UsedImplicitly]
public record FloatProvider(
    float? Value = null,
    string? ProviderId = null
) : IFlexibleModel<FloatProvider>
{
    /// <summary>
    /// Parses a <see cref="FloatProvider"/> from an SNBT node representation.
    /// </summary>
    /// <param name="node">The SNBT node (either a numeric node or an <see cref="SnbtString"/>).</param>
    /// <returns>A populated <see cref="FloatProvider"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="node"/> is neither a numeric value nor a string identifier.</exception>
    /// <example>
    /// <code>
    /// // From a constant number:
    /// var constant = FloatProvider.Parse(new SnbtFloat(20.0f));
    ///
    /// // From a provider identifier:
    /// var reference = FloatProvider.Parse(new SnbtString("minecraft:brewing/uses_default"));
    /// </code>
    /// </example>
    public static FloatProvider Parse(ISnbtNode node)
    {
        return node switch
        {
            SnbtFloat f => new FloatProvider(Value: f.Value),
            SnbtDouble d => new FloatProvider(Value: (float)d.Value),
            SnbtInt i => new FloatProvider(Value: i.Value),
            SnbtShort s => new FloatProvider(Value: s.Value),
            SnbtByte b => new FloatProvider(Value: b.Value),
            SnbtLong l => new FloatProvider(Value: l.Value),
            SnbtString str => new FloatProvider(ProviderId: str.Value),
            _ => throw new ArgumentException($"Expected a numeric node or string ID for FloatProvider, but got {node.GetType().Name}.", nameof(node))
        };
    }

    /// <summary>
    /// Serializes the provider into its compact SNBT representation (<see cref="SnbtString"/> or <see cref="SnbtFloat"/>).
    /// </summary>
    /// <returns>An <see cref="ISnbtNode"/> containing the float or string representation.</returns>
    public ISnbtNode ToSnbt()
    {
        if (ProviderId != null)
            return new SnbtString(ProviderId);

        return new SnbtFloat(Value ?? 0.0f);
    }

    /// <summary>
    /// Determines whether this provider is a constant value approximately equal to the specified target.
    /// </summary>
    /// <param name="target">The expected float value to test against.</param>
    /// <returns><see langword="true"/> if this instance is a constant matching <paramref name="target"/>; otherwise, <see langword="false"/>.</returns>
    public bool IsFixed(float target) => Value.HasValue && Math.Abs(Value.Value - target) < 0.0001f;

    /// <summary>
    /// Implicitly converts a float literal into a constant <see cref="FloatProvider"/>.
    /// </summary>
    /// <param name="value">The constant float value.</param>
    public static implicit operator FloatProvider(float value) => new(Value: value);

    /// <summary>
    /// Implicitly converts a provider resource identifier into a referenced <see cref="FloatProvider"/>.
    /// </summary>
    /// <param name="providerId">The resource identifier of the number provider.</param>
    public static implicit operator FloatProvider(string providerId) => new(ProviderId: providerId);
}