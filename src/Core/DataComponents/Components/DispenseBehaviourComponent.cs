using Core.DataComponents.Interfaces;
using Core.SNBT.Interfaces;
using Core.SNBT.Nodes;
using JetBrains.Annotations;

namespace Core.DataComponents.Components;

/// <summary>
/// Determines what happens when an item is dispensed from a dispenser, dropper, or ominous item spawner (<c>minecraft:dispense_behavior</c>).
/// Can be represented either as a namespaced registry ID or as an inline list of dispense behavior compounds.
/// </summary>
/// <param name="BehaviorId">Namespaced ID of a <c>dispense_behavior</c> registry entry.</param>
/// <param name="Behaviors">Inline list of raw dispense behavior compound definitions.</param>
[UsedImplicitly]
public record DispenseBehaviorComponent(
    string? BehaviorId = null,
    List<SnbtCompound>? Behaviors = null
) : IFlexibleComponent<DispenseBehaviorComponent>
{
    /// <inheritdoc/>
    public static string ComponentId => "minecraft:dispense_behavior";

    /// <summary>
    /// Initializes a new instance of the <see cref="DispenseBehaviorComponent"/> record with an inline list of behaviors.
    /// </summary>
    /// <param name="behaviors">The dispense behavior compounds to include.</param>
    public DispenseBehaviorComponent(params SnbtCompound[] behaviors)
        : this(BehaviorId: null, Behaviors: [.. behaviors])
    {
    }

    /// <summary>
    /// Parses a <see cref="DispenseBehaviorComponent"/> from an SNBT string or list node.
    /// </summary>
    /// <param name="node">The SNBT node to parse, which must be either an <see cref="SnbtString"/> or an <see cref="SnbtList"/> of <see cref="SnbtCompound"/>.</param>
    /// <returns>A populated <see cref="DispenseBehaviorComponent"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="node"/> is neither an <see cref="SnbtString"/> nor a valid <see cref="SnbtList"/> of compounds.</exception>
    /// <example>
    /// <code>
    /// var node = SnbtParser.Parse("[{type:\"minecraft:place_block\"}]");
    /// var component = DispenseBehaviorComponent.Parse(node);
    /// </code>
    /// </example>
    public static DispenseBehaviorComponent Parse(ISnbtNode node) => node switch
    {
        SnbtString str => new DispenseBehaviorComponent(BehaviorId: str.Value),
        SnbtList list => ParseInlineBehaviors(list),
        _ => throw new ArgumentException(
            $"Expected {nameof(SnbtString)} or {nameof(SnbtString)} for '{ComponentId}', got '{node.GetType().Name}'.",
            nameof(node))
    };

    /// <summary>
    /// Serializes the component into either an <see cref="SnbtString"/> or an <see cref="SnbtList"/>.
    /// </summary>
    /// <returns>An <see cref="ISnbtNode"/> representing the dispense behavior.</returns>
    /// <exception cref="InvalidOperationException">Thrown when neither <see cref="BehaviorId"/> nor <see cref="Behaviors"/> is set.</exception>
    public ISnbtNode ToSnbt()
    {
        if (!string.IsNullOrWhiteSpace(BehaviorId))
            return new SnbtString(BehaviorId);

        if (Behaviors is not null)
            return new SnbtList([..Behaviors]);

        throw new InvalidOperationException(
            $"'{ComponentId}' must specify either a {nameof(BehaviorId)} or {nameof(Behaviors)}.");
    }

    /// <summary>
    /// Extracts inline dispense behavior compounds from an SNBT list node.
    /// </summary>
    /// <param name="list">The SNBT list containing behavior compounds.</param>
    /// <returns>A <see cref="DispenseBehaviorComponent"/> populated with the parsed compounds.</returns>
    /// <exception cref="ArgumentException">Thrown when any element in <paramref name="list"/> is not an <see cref="SnbtCompound"/>.</exception>
    private static DispenseBehaviorComponent ParseInlineBehaviors(SnbtList list)
    {
        var behaviors = new List<SnbtCompound>(list.Items.Count);
        foreach (var item in list.Items)
        {
            if (item is not SnbtCompound compound)
            {
                throw new ArgumentException(
                    $"All elements in '{ComponentId}' list must be {nameof(SnbtCompound)}, got '{item.GetType().Name}'.",
                    nameof(list));
            }

            behaviors.Add(compound);
        }

        return new DispenseBehaviorComponent(Behaviors: behaviors);
    }
}