using Core.DataComponents.Interfaces;
using Core.DataComponents.Models.Providers;
using Core.SNBT;
using Core.SNBT.Interfaces;
using Core.SNBT.Nodes;
using JetBrains.Annotations;

namespace Core.DataComponents.Components;

/// <summary>
/// Enables an item to be used as compost in a composter (<c>minecraft:compostable</c>).
/// </summary>
/// <param name="Layers">The number of compost layers added when interacting with a non-full composter. Either a fixed numeric count or a provider ID.</param>
[UsedImplicitly]
public record CompostableComponent(
    FloatProvider Layers
) : ICompoundComponent<CompostableComponent>
{
    /// <inheritdoc/>
    public static string ComponentId => "minecraft:compostable";

    /// <summary>
    /// Initializes a new instance of the <see cref="CompostableComponent"/> record with a constant number of layers.
    /// </summary>
    /// <param name="layers">The fixed number of layers to add.</param>
    public CompostableComponent(float layers)
        : this(new FloatProvider(Value: layers))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CompostableComponent"/> record referencing a dynamic number provider.
    /// </summary>
    /// <param name="providerId">The namespaced resource identifier of the number provider.</param>
    public CompostableComponent(string providerId)
        : this(new FloatProvider(ProviderId: providerId))
    {
    }

    /// <summary>
    /// Parses a <see cref="CompostableComponent"/> from an SNBT compound node representation.
    /// </summary>
    /// <param name="compound">The SNBT compound containing the compostable configuration.</param>
    /// <returns>A populated <see cref="CompostableComponent"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="compound"/> is missing the required <c>layers</c> node.</exception>
    /// <example>
    /// <code>
    /// // Fixed layer count:
    /// var comp1 = CompostableComponent.Parse(SnbtParser.Parse("{layers: 1.0f}"));
    ///
    /// // Dynamic layer provider:
    /// var comp2 = CompostableComponent.Parse(SnbtParser.Parse("{layers: \"minecraft:composter/chance_30\"}"));
    /// </code>
    /// </example>
    public static CompostableComponent Parse(SnbtCompound compound)
    {
        if (compound.GetNode("layers") is not { } layersNode)
            throw new ArgumentException("Compostable component requires a 'layers' tag.", nameof(compound));

        return new CompostableComponent(FloatProvider.Parse(layersNode));
    }

    /// <summary>
    /// Serializes the component into an SNBT compound node containing the <c>layers</c> field.
    /// </summary>
    /// <returns>An <see cref="ISnbtNode"/> representing the compostable compound.</returns>
    public ISnbtNode ToSnbt() => Snbt.Compound()
        .Put("layers", Layers.ToSnbt())
        .Build();
}