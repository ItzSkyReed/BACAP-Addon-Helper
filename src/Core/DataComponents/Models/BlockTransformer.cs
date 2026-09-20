using Core.DataComponents.Models.Interfaces;
using Core.SNBT;
using Core.SNBT.Interfaces;
using Core.SNBT.Nodes;
using JetBrains.Annotations;

namespace Core.DataComponents.Models;

/// <summary>
/// Configures a single block transformation rule and its side effects.
/// </summary>
/// <param name="BlockStateProvider">The provider determining the target replacement block state.</param>
/// <param name="Sound">Optional sound event played when the transformation succeeds.</param>
/// <param name="Particle">Particle effect rendered on transformation (<c>"none"</c>, <c>"scrape"</c>, <c>"wax_on"</c>, <c>"wax_off"</c>). Defaults to <c>"none"</c>.</param>
/// <param name="DisallowedFaces">Block faces that reject transformation interaction.</param>
/// <param name="Loot">Optional loot table resource location dropped upon transformation.</param>
/// <param name="DropStrategy">Spawn position of dropped loot (<c>"from_middle"</c> or <c>"clicked_face"</c>). Defaults to <c>"from_middle"</c>.</param>
/// <param name="UpdateFromNeighbors">Whether the placed block receives block updates from neighbors. Defaults to <see langword="true"/>.</param>
/// <param name="TransformType">The scope of transformation (<c>"single_block"</c> or <c>"copper_chest"</c>). Defaults to <c>"single_block"</c>.</param>
/// <param name="ConsumeOnUse">Whether stackable items are consumed in Survival/Adventure mode. Defaults to <see langword="true"/>.</param>
/// <param name="ItemDamagePerUse">Durability cost applied to non-stackable items in Survival/Adventure mode. Defaults to 1.</param>
[UsedImplicitly]
public record BlockTransformer(
    BlockStateProvider BlockStateProvider,
    SoundEvent? Sound = null,
    string Particle = BlockTransformer.ParticleNone,
    List<string>? DisallowedFaces = null,
    string? Loot = null,
    string DropStrategy = BlockTransformer.DropFromMiddle,
    bool UpdateFromNeighbors = true,
    string TransformType = BlockTransformer.TransformSingleBlock,
    bool ConsumeOnUse = true,
    int ItemDamagePerUse = 1
) : ICompoundModel<BlockTransformer>
{
    [PublicAPI]
    public const string ParticleNone = "none";
    [PublicAPI]
    public const string ParticleScrape = "scraperape";
    [PublicAPI]
    public const string ParticleWaxOn = "wax_on";
    [PublicAPI]
    public const string ParticleWaxOff = "wax_off";

    [PublicAPI]
    public const string DropFromMiddle = "from_middle";
    [PublicAPI]
    public const string DropClickedFace = "clicked_face";

    [PublicAPI]
    public const string TransformSingleBlock = "single_block";

    [PublicAPI]
    public const string TransformCopperChest = "copper_chest";

    /// <summary>
    /// Parses a <see cref="BlockTransformer"/> from an SNBT compound node.
    /// </summary>
    /// <param name="compound">The SNBT compound containing transformation parameters.</param>
    /// <returns>A populated <see cref="BlockTransformer"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="compound"/> is missing the required <c>block_state_provider</c>.</exception>
    /// <example>
    /// <code>
    /// var node = SnbtParser.Parse("{block_state_provider: {type: \"rule_based\", rules: []}, particle: \"scrape\"}");
    /// var transformer = BlockTransformer.Parse(node);
    /// </code>
    /// </example>
    public static BlockTransformer Parse(SnbtCompound compound)
    {
        if (compound.GetNode("block_state_provider") is not SnbtCompound providerNode)
            throw new ArgumentException("Block transformer requires a 'block_state_provider' compound.", nameof(compound));

        var sound = compound.GetNode("sound") switch
        {
            { } soundNode => SoundEvent.Parse(soundNode),
            null => null
        };

        List<string>? disallowedFaces = null;
        if (compound.GetNode("disallowed_faces") is SnbtList facesList)
        {
            disallowedFaces = facesList.Items
                .OfType<SnbtString>()
                .Select(s => s.Value)
                .ToList();
        }

        return new BlockTransformer(
            BlockStateProvider: BlockStateProvider.Parse(providerNode),
            Sound: sound,
            Particle: compound.GetString("particle", ParticleNone),
            DisallowedFaces: disallowedFaces,
            Loot: compound.GetOptionalString("loot"),
            DropStrategy: compound.GetString("drop_strategy", DropFromMiddle),
            UpdateFromNeighbors: compound.GetBool("update_from_neighbors", true),
            TransformType: compound.GetString("transform_type", TransformSingleBlock),
            ConsumeOnUse: compound.GetBool("consume_on_use", true),
            ItemDamagePerUse: compound.GetInt("item_damage_per_use", 1)
        );
    }

    /// <summary>
    /// Serializes the transformer configuration into an SNBT compound node.
    /// </summary>
    /// <returns>An <see cref="ISnbtNode"/> representing the transformer compound.</returns>
    public ISnbtNode ToSnbt()
    {
        var builder = Snbt.Compound()
            .Put("block_state_provider", BlockStateProvider.ToSnbt())
            .PutOptional("particle", Particle, ParticleNone)
            .PutOptional("drop_strategy", DropStrategy, DropFromMiddle)
            .PutOptional("update_from_neighbors", UpdateFromNeighbors, true)
            .PutOptional("transform_type", TransformType, TransformSingleBlock)
            .PutOptional("consume_on_use", ConsumeOnUse, true)
            .PutOptional("item_damage_per_use", ItemDamagePerUse, 1);

        if (Sound != null)
            builder.Put("sound", Sound.ToSnbt());

        if (Loot != null)
            builder.Put("loot", Loot);

        if (DisallowedFaces is not { Count: > 0 })
            return builder.Build();

        var facesList = new SnbtList();
        foreach (var face in DisallowedFaces)
            facesList.Items.Add(new SnbtString(face));

        builder.Put("disallowed_faces", facesList);

        return builder.Build();
    }
}