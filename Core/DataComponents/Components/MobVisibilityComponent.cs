using Core.DataComponents.Interfaces;
using Core.SNBT;
using Core.SNBT.Interfaces;
using Core.SNBT.Nodes;
using JetBrains.Annotations;

namespace Core.DataComponents.Components;

/// <summary>
/// Configures the wearer's detectability multiplier for specified mob entity types (<c>minecraft:mob_visibility</c>).
/// Applies when the item is equipped in an appropriate armor or equipment slot.
/// </summary>
/// <param name="TargetingEntityTypes">The collection of entity type IDs or tag references (prefixed with <c>#</c>) affected by this visibility modifier.</param>
/// <param name="Visibility">The detection range multiplier for targeted mobs (valid range is 0.0 to 10.0).</param>
[UsedImplicitly]
public record MobVisibilityComponent(
    List<string> TargetingEntityTypes,
    float Visibility
) : ICompoundComponent<MobVisibilityComponent>
{
    /// <summary>
    /// The minimum allowed visibility multiplier (reduces detection range to 2 blocks).
    /// </summary>
    public const float MinVisibility = 0.0f;

    /// <summary>
    /// The maximum allowed visibility multiplier (increases detection range tenfold).
    /// </summary>
    public const float MaxVisibility = 10.0f;

    /// <inheritdoc/>
    public static string ComponentId => "minecraft:mob_visibility";

    /// <summary>
    /// Initializes a new instance of the <see cref="MobVisibilityComponent"/> record targeting a single entity type or tag.
    /// </summary>
    /// <param name="targetingEntityType">The entity resource ID (e.g., <c>"minecraft:skeleton"</c>) or tag reference (e.g., <c>"#minecraft:skeletons"</c>).</param>
    /// <param name="visibility">The detection range multiplier.</param>
    public MobVisibilityComponent(string targetingEntityType, float visibility)
        : this([targetingEntityType], visibility)
    {
    }

    /// <summary>
    /// Parses a <see cref="MobVisibilityComponent"/> from an SNBT compound node representation.
    /// </summary>
    /// <param name="compound">The SNBT compound node containing mob visibility configuration.</param>
    /// <returns>A populated <see cref="MobVisibilityComponent"/> instance.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="compound"/> is missing the required <c>targeting_entity_types</c> or <c>visibility</c> tags.
    /// </exception>
    /// <example>
    /// <code>
    /// // Single entity type:
    /// var singleNode = SnbtParser.Parse("{targeting_entity_types: \"minecraft:skeleton\", visibility: 0.0f}");
    /// var comp1 = MobVisibilityComponent.Parse(singleNode);
    ///
    /// // List of entity types:
    /// var listNode = SnbtParser.Parse("{targeting_entity_types: [\"minecraft:zombie\", \"minecraft:skeleton\"], visibility: 0.5f}");
    /// var comp2 = MobVisibilityComponent.Parse(listNode);
    /// </code>
    /// </example>
    public static MobVisibilityComponent Parse(SnbtCompound compound)
    {
        var typesNode = compound.GetNode("targeting_entity_types");
        List<string> entityTypes = typesNode switch
        {
            SnbtString singleType => [singleType.Value],
            SnbtList typeList => typeList.Items
                .OfType<SnbtString>()
                .Select(s => s.Value)
                .ToList(),
            _ => throw new ArgumentException("Mob visibility component requires a 'targeting_entity_types' string or list tag.", nameof(compound))
        };

        var visibility = compound.GetOptionalFloat("visibility")
            ?? throw new ArgumentException("Mob visibility component requires a 'visibility' numeric tag.", nameof(compound));

        return new MobVisibilityComponent(entityTypes, visibility);
    }

    /// <summary>
    /// Serializes the component into an SNBT compound node.
    /// Emits <c>targeting_entity_types</c> as a single string if only one type is specified; otherwise, as a list.
    /// </summary>
    /// <returns>An <see cref="ISnbtNode"/> representing the mob visibility compound.</returns>
    public ISnbtNode ToSnbt()
    {
        var builder = Snbt.Compound()
            .Put("visibility", Visibility);

        if (TargetingEntityTypes.Count == 1)
        {
            builder.Put("targeting_entity_types", TargetingEntityTypes[0]);
        }
        else
        {
            var list = new SnbtList();
            foreach (var type in TargetingEntityTypes)
                list.Items.Add(new SnbtString(type));

            builder.Put("targeting_entity_types", list);
        }

        return builder.Build();
    }
}