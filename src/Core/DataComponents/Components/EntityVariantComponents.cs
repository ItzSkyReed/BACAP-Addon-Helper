using Core.DataComponents.Components.Base;
using Core.DataComponents.Interfaces;
using Core.SNBT.Nodes;
using JetBrains.Annotations;

namespace Core.DataComponents.Components;

/// <summary>
/// Defines the variant type of an axolotl (<c>minecraft:axolotl/variant</c>).
/// </summary>
/// <param name="Value">The axolotl variant identifier.</param>
[UsedImplicitly]
public record AxolotlVariantComponent(string Value) : StringComponentBase(Value), IStringComponent<AxolotlVariantComponent>
{
    /// <inheritdoc/>
    public static string ComponentId => "minecraft:axolotl/variant";

    /// <summary>
    /// Parses an <see cref="AxolotlVariantComponent"/> from an SNBT string node.
    /// </summary>
    public static AxolotlVariantComponent Parse(SnbtString node) => new(node.Value);

    /// <summary>
    /// Implicitly converts a string variant name into an <see cref="AxolotlVariantComponent"/>.
    /// </summary>
    public static implicit operator AxolotlVariantComponent(string value) => new(value);

    [PublicAPI]
    public const string Lucy = "lucy", Wild = "wild", Gold = "gold", Cyan = "cyan", Blue = "blue";
}

/// <summary>
/// Defines the collar dye color of a tamed cat (<c>minecraft:cat/collar</c>).
/// </summary>
/// <param name="Value">The collar dye color identifier.</param>
[UsedImplicitly]
public record CatCollarComponent(string Value) : StringComponentBase(Value), IStringComponent<CatCollarComponent>
{
    /// <inheritdoc/>
    public static string ComponentId => "minecraft:cat/collar";

    /// <summary>
    /// Parses a <see cref="CatCollarComponent"/> from an SNBT string node.
    /// </summary>
    public static CatCollarComponent Parse(SnbtString node) => new(node.Value);

    /// <summary>
    /// Implicitly converts a dye color name into a <see cref="CatCollarComponent"/>.
    /// </summary>
    public static implicit operator CatCollarComponent(string value) => new(value);
}

/// <summary>
/// Defines the skin texture variant of a cat (<c>minecraft:cat/variant</c>).
/// </summary>
/// <param name="Value">The resource location of the cat variant (e.g. <c>minecraft:tabby</c>, <c>minecraft:black</c>).</param>
[UsedImplicitly]
public record CatVariantComponent(string Value) : StringComponentBase(Value), IStringComponent<CatVariantComponent>
{
    /// <inheritdoc/>
    public static string ComponentId => "minecraft:cat/variant";

    /// <summary>
    /// Parses a <see cref="CatVariantComponent"/> from an SNBT string node.
    /// </summary>
    public static CatVariantComponent Parse(SnbtString node) => new(node.Value);

    /// <summary>
    /// Implicitly converts a variant identifier into a <see cref="CatVariantComponent"/>.
    /// </summary>
    public static implicit operator CatVariantComponent(string value) => new(value);
}

/// <summary>
/// Defines the visual breed variant of a chicken (<c>minecraft:chicken/variant</c>).
/// </summary>
/// <param name="Value">The chicken variant identifier.</param>
[UsedImplicitly]
public record ChickenVariantComponent(string Value) : StringComponentBase(Value), IStringComponent<ChickenVariantComponent>
{
    /// <inheritdoc/>
    public static string ComponentId => "minecraft:chicken/variant";

    /// <summary>
    /// Parses a <see cref="ChickenVariantComponent"/> from an SNBT string node.
    /// </summary>
    public static ChickenVariantComponent Parse(SnbtString node) => new(node.Value);

    /// <summary>
    /// Implicitly converts a variant identifier into a <see cref="ChickenVariantComponent"/>.
    /// </summary>
    public static implicit operator ChickenVariantComponent(string value) => new(value);
}

/// <summary>
/// Defines the visual breed variant of a cow (<c>minecraft:cow/variant</c>).
/// </summary>
/// <param name="Value">The cow variant identifier.</param>
[UsedImplicitly]
public record CowVariantComponent(string Value) : StringComponentBase(Value), IStringComponent<CowVariantComponent>
{
    /// <inheritdoc/>
    public static string ComponentId => "minecraft:cow/variant";

    /// <summary>
    /// Parses a <see cref="CowVariantComponent"/> from an SNBT string node.
    /// </summary>
    public static CowVariantComponent Parse(SnbtString node) => new(node.Value);

    /// <summary>
    /// Implicitly converts a variant identifier into a <see cref="CowVariantComponent"/>.
    /// </summary>
    public static implicit operator CowVariantComponent(string value) => new(value);
}

/// <summary>
/// Defines the cushion color of decorative entities such as happy ghasts or seats (<c>minecraft:cushion/color</c>).
/// </summary>
/// <param name="Value">The dye color identifier of the cushion.</param>
[UsedImplicitly]
public record CushionColorComponent(string Value) : StringComponentBase(Value), IStringComponent<CushionColorComponent>
{
    /// <inheritdoc/>
    public static string ComponentId => "minecraft:cushion/color";

    /// <summary>
    /// Parses a <see cref="CushionColorComponent"/> from an SNBT string node.
    /// </summary>
    public static CushionColorComponent Parse(SnbtString node) => new(node.Value);

    /// <summary>
    /// Implicitly converts a color name into a <see cref="CushionColorComponent"/>.
    /// </summary>
    public static implicit operator CushionColorComponent(string value) => new(value);
}

/// <summary>
/// Defines the species variant of a fox (<c>minecraft:fox/variant</c>).
/// </summary>
/// <param name="Value">The fox variant identifier (<c>red</c> or <c>snow</c>).</param>
[UsedImplicitly]
public record FoxVariantComponent(string Value) : StringComponentBase(Value), IStringComponent<FoxVariantComponent>
{
    /// <inheritdoc/>
    public static string ComponentId => "minecraft:fox/variant";

    /// <summary>
    /// Parses a <see cref="FoxVariantComponent"/> from an SNBT string node.
    /// </summary>
    public static FoxVariantComponent Parse(SnbtString node) => new(node.Value);

    /// <summary>
    /// Implicitly converts a string variant into a <see cref="FoxVariantComponent"/>.
    /// </summary>
    public static implicit operator FoxVariantComponent(string value) => new(value);

    [PublicAPI]
    public const string Red = "red", Snow = "snow";
}

/// <summary>
/// Defines the climate-based variant of a frog (<c>minecraft:frog/variant</c>).
/// </summary>
/// <param name="Value">The resource location of the frog variant.</param>
[UsedImplicitly]
public record FrogVariantComponent(string Value) : StringComponentBase(Value), IStringComponent<FrogVariantComponent>
{
    /// <inheritdoc/>
    public static string ComponentId => "minecraft:frog/variant";

    /// <summary>
    /// Parses a <see cref="FrogVariantComponent"/> from an SNBT string node.
    /// </summary>
    public static FrogVariantComponent Parse(SnbtString node) => new(node.Value);

    /// <summary>
    /// Implicitly converts a resource location into a <see cref="FrogVariantComponent"/>.
    /// </summary>
    public static implicit operator FrogVariantComponent(string value) => new(value);

    [PublicAPI]
    public const string Temperate = "minecraft:temperate", Warm = "minecraft:warm", Cold = "minecraft:cold";
}

/// <summary>
/// Defines the coat color pattern variant of a horse (<c>minecraft:horse/variant</c>).
/// </summary>
/// <param name="Value">The horse coat color identifier.</param>
[UsedImplicitly]
public record HorseVariantComponent(string Value) : StringComponentBase(Value), IStringComponent<HorseVariantComponent>
{
    /// <inheritdoc/>
    public static string ComponentId => "minecraft:horse/variant";

    /// <summary>
    /// Parses a <see cref="HorseVariantComponent"/> from an SNBT string node.
    /// </summary>
    public static HorseVariantComponent Parse(SnbtString node) => new(node.Value);

    /// <summary>
    /// Implicitly converts a coat name into a <see cref="HorseVariantComponent"/>.
    /// </summary>
    public static implicit operator HorseVariantComponent(string value) => new(value);

    [PublicAPI]
    public const string White = "white", Creamy = "creamy", Chestnut = "chestnut", Brown = "brown",
                        Black = "black", Gray = "gray", DarkBrown = "dark_brown";
}

/// <summary>
/// Defines the fur color variant of a llama or trader llama (<c>minecraft:llama/variant</c>).
/// </summary>
/// <param name="Value">The llama fur color identifier.</param>
[UsedImplicitly]
public record LlamaVariantComponent(string Value) : StringComponentBase(Value), IStringComponent<LlamaVariantComponent>
{
    /// <inheritdoc/>
    public static string ComponentId => "minecraft:llama/variant";

    /// <summary>
    /// Parses a <see cref="LlamaVariantComponent"/> from an SNBT string node.
    /// </summary>
    public static LlamaVariantComponent Parse(SnbtString node) => new(node.Value);

    /// <summary>
    /// Implicitly converts a fur color name into a <see cref="LlamaVariantComponent"/>.
    /// </summary>
    public static implicit operator LlamaVariantComponent(string value) => new(value);

    [PublicAPI]
    public const string Creamy = "creamy", White = "white", Brown = "brown", Gray = "gray";
}

/// <summary>
/// Defines the mushroom type variant of a mooshroom (<c>minecraft:mooshroom/variant</c>).
/// </summary>
/// <param name="Value">The mooshroom variant (<c>red</c> or <c>brown</c>).</param>
[UsedImplicitly]
public record MooshroomVariantComponent(string Value) : StringComponentBase(Value), IStringComponent<MooshroomVariantComponent>
{
    /// <inheritdoc/>
    public static string ComponentId => "minecraft:mooshroom/variant";

    /// <summary>
    /// Parses a <see cref="MooshroomVariantComponent"/> from an SNBT string node.
    /// </summary>
    public static MooshroomVariantComponent Parse(SnbtString node) => new(node.Value);

    /// <summary>
    /// Implicitly converts a variant name into a <see cref="MooshroomVariantComponent"/>.
    /// </summary>
    public static implicit operator MooshroomVariantComponent(string value) => new(value);

    [PublicAPI]
    public const string Red = "red", Brown = "brown";
}

/// <summary>
/// Defines the motif artwork variant of a painting entity (<c>minecraft:painting/variant</c>).
/// </summary>
/// <param name="Value">The resource location of the painting motif (e.g. <c>minecraft:kebab</c>, <c>minecraft:aztec</c>).</param>
[UsedImplicitly]
public record PaintingVariantComponent(string Value) : StringComponentBase(Value), IStringComponent<PaintingVariantComponent>
{
    /// <inheritdoc/>
    public static string ComponentId => "minecraft:painting/variant";

    /// <summary>
    /// Parses a <see cref="PaintingVariantComponent"/> from an SNBT string node.
    /// </summary>
    public static PaintingVariantComponent Parse(SnbtString node) => new(node.Value);

    /// <summary>
    /// Implicitly converts a motif resource location into a <see cref="PaintingVariantComponent"/>.
    /// </summary>
    public static implicit operator PaintingVariantComponent(string value) => new(value);
}

/// <summary>
/// Defines the feather color plumage variant of a parrot (<c>minecraft:parrot/variant</c>).
/// </summary>
/// <param name="Value">The parrot plumage variant identifier.</param>
[UsedImplicitly]
public record ParrotVariantComponent(string Value) : StringComponentBase(Value), IStringComponent<ParrotVariantComponent>
{
    /// <inheritdoc/>
    public static string ComponentId => "minecraft:parrot/variant";

    /// <summary>
    /// Parses a <see cref="ParrotVariantComponent"/> from an SNBT string node.
    /// </summary>
    public static ParrotVariantComponent Parse(SnbtString node) => new(node.Value);

    /// <summary>
    /// Implicitly converts a plumage name into a <see cref="ParrotVariantComponent"/>.
    /// </summary>
    public static implicit operator ParrotVariantComponent(string value) => new(value);

    [PublicAPI]
    public const string RedBlue = "red_blue", Blue = "blue", Green = "green", YellowBlue = "yellow_blue", Gray = "gray";
}

/// <summary>
/// Defines the visual breed variant of a pig (<c>minecraft:pig/variant</c>).
/// </summary>
/// <param name="Value">The pig variant identifier.</param>
[UsedImplicitly]
public record PigVariantComponent(string Value) : StringComponentBase(Value), IStringComponent<PigVariantComponent>
{
    /// <inheritdoc/>
    public static string ComponentId => "minecraft:pig/variant";

    /// <summary>
    /// Parses a <see cref="PigVariantComponent"/> from an SNBT string node.
    /// </summary>
    public static PigVariantComponent Parse(SnbtString node) => new(node.Value);

    /// <summary>
    /// Implicitly converts a variant name into a <see cref="PigVariantComponent"/>.
    /// </summary>
    public static implicit operator PigVariantComponent(string value) => new(value);
}

/// <summary>
/// Defines the breed coat variant of a rabbit (<c>minecraft:rabbit/variant</c>).
/// </summary>
/// <param name="Value">The rabbit breed identifier.</param>
[UsedImplicitly]
public record RabbitVariantComponent(string Value) : StringComponentBase(Value), IStringComponent<RabbitVariantComponent>
{
    /// <inheritdoc/>
    public static string ComponentId => "minecraft:rabbit/variant";

    /// <summary>
    /// Parses a <see cref="RabbitVariantComponent"/> from an SNBT string node.
    /// </summary>
    public static RabbitVariantComponent Parse(SnbtString node) => new(node.Value);

    /// <summary>
    /// Implicitly converts a breed name into a <see cref="RabbitVariantComponent"/>.
    /// </summary>
    public static implicit operator RabbitVariantComponent(string value) => new(value);

    [PublicAPI]
    public const string Brown = "brown", White = "white", Black = "black", WhiteSplotched = "white_splotched",
                        Gold = "gold", Salt = "salt", Evil = "evil";
}

/// <summary>
/// Defines the physical size category of a salmon entity (<c>minecraft:salmon/size</c>).
/// </summary>
/// <param name="Value">The size category (<c>small</c>, <c>medium</c>, or <c>large</c>).</param>
[UsedImplicitly]
public record SalmonSizeComponent(string Value) : StringComponentBase(Value), IStringComponent<SalmonSizeComponent>
{
    /// <inheritdoc/>
    public static string ComponentId => "minecraft:salmon/size";

    /// <summary>
    /// Parses a <see cref="SalmonSizeComponent"/> from an SNBT string node.
    /// </summary>
    public static SalmonSizeComponent Parse(SnbtString node) => new(node.Value);

    /// <summary>
    /// Implicitly converts a size string into a <see cref="SalmonSizeComponent"/>.
    /// </summary>
    public static implicit operator SalmonSizeComponent(string value) => new(value);

    [PublicAPI]
    public const string Small = "small", Medium = "medium", Large = "large";
}

/// <summary>
/// Defines the wool fleece dye color of a sheep (<c>minecraft:sheep/color</c>).
/// </summary>
/// <param name="Value">The dye color identifier of the sheep fleece.</param>
[UsedImplicitly]
public record SheepColorComponent(string Value) : StringComponentBase(Value), IStringComponent<SheepColorComponent>
{
    /// <inheritdoc/>
    public static string ComponentId => "minecraft:sheep/color";

    /// <summary>
    /// Parses a <see cref="SheepColorComponent"/> from an SNBT string node.
    /// </summary>
    public static SheepColorComponent Parse(SnbtString node) => new(node.Value);

    /// <summary>
    /// Implicitly converts a dye color name into a <see cref="SheepColorComponent"/>.
    /// </summary>
    public static implicit operator SheepColorComponent(string value) => new(value);
}

/// <summary>
/// Defines the shell dye color of a shulker (<c>minecraft:shulker/color</c>).
/// </summary>
/// <param name="Value">The dye color identifier of the shulker shell.</param>
[UsedImplicitly]
public record ShulkerColorComponent(string Value) : StringComponentBase(Value), IStringComponent<ShulkerColorComponent>
{
    /// <inheritdoc/>
    public static string ComponentId => "minecraft:shulker/color";

    /// <summary>
    /// Parses a <see cref="ShulkerColorComponent"/> from an SNBT string node.
    /// </summary>
    public static ShulkerColorComponent Parse(SnbtString node) => new(node.Value);

    /// <summary>
    /// Implicitly converts a dye color name into a <see cref="ShulkerColorComponent"/>.
    /// </summary>
    public static implicit operator ShulkerColorComponent(string value) => new(value);
}


/// <summary>
/// Defines the base body dye color of a tropical fish (<c>minecraft:tropical_fish/base_color</c>).
/// </summary>
/// <param name="Value">The dye color identifier for the body.</param>
[UsedImplicitly]
public record TropicalFishBaseColorComponent(string Value) : StringComponentBase(Value), IStringComponent<TropicalFishBaseColorComponent>
{
    /// <inheritdoc/>
    public static string ComponentId => "minecraft:tropical_fish/base_color";

    /// <summary>
    /// Parses a <see cref="TropicalFishBaseColorComponent"/> from an SNBT string node.
    /// </summary>
    public static TropicalFishBaseColorComponent Parse(SnbtString node) => new(node.Value);

    /// <summary>
    /// Implicitly converts a dye color name into a <see cref="TropicalFishBaseColorComponent"/>.
    /// </summary>
    public static implicit operator TropicalFishBaseColorComponent(string value) => new(value);
}

/// <summary>
/// Defines the marking pattern shape of a tropical fish (<c>minecraft:tropical_fish/pattern</c>).
/// </summary>
/// <param name="Value">The fish pattern shape identifier.</param>
[UsedImplicitly]
public record TropicalFishPatternComponent(string Value) : StringComponentBase(Value), IStringComponent<TropicalFishPatternComponent>
{
    /// <inheritdoc/>
    public static string ComponentId => "minecraft:tropical_fish/pattern";

    /// <summary>
    /// Parses a <see cref="TropicalFishPatternComponent"/> from an SNBT string node.
    /// </summary>
    public static TropicalFishPatternComponent Parse(SnbtString node) => new(node.Value);

    /// <summary>
    /// Implicitly converts a pattern name into a <see cref="TropicalFishPatternComponent"/>.
    /// </summary>
    public static implicit operator TropicalFishPatternComponent(string value) => new(value);

    [PublicAPI]
    public const string Kob = "kob", Sunstreak = "sunstreak", Snooper = "snooper", Dasher = "dasher",
                        Brinely = "brinely", Spotty = "spotty", Flopper = "flopper", Stripey = "stripey",
                        Glitter = "glitter", Blockfish = "blockfish", Betty = "betty", Clayfish = "clayfish";
}

/// <summary>
/// Defines the secondary pattern overlay dye color of a tropical fish (<c>minecraft:tropical_fish/pattern_color</c>).
/// </summary>
/// <param name="Value">The dye color identifier for the pattern markings.</param>
[UsedImplicitly]
public record TropicalFishPatternColorComponent(string Value) : StringComponentBase(Value), IStringComponent<TropicalFishPatternColorComponent>
{
    /// <inheritdoc/>
    public static string ComponentId => "minecraft:tropical_fish/pattern_color";

    /// <summary>
    /// Parses a <see cref="TropicalFishPatternColorComponent"/> from an SNBT string node.
    /// </summary>
    public static TropicalFishPatternColorComponent Parse(SnbtString node) => new(node.Value);

    /// <summary>
    /// Implicitly converts a dye color name into a <see cref="TropicalFishPatternColorComponent"/>.
    /// </summary>
    public static implicit operator TropicalFishPatternColorComponent(string value) => new(value);
}


/// <summary>
/// Defines the biome-specific profession outfit variant of a villager (<c>minecraft:villager/variant</c>).
/// </summary>
/// <param name="Value">The biome outfit variant identifier.</param>
[UsedImplicitly]
public record VillagerVariantComponent(string Value) : StringComponentBase(Value), IStringComponent<VillagerVariantComponent>
{
    /// <inheritdoc/>
    public static string ComponentId => "minecraft:villager/variant";

    /// <summary>
    /// Parses a <see cref="VillagerVariantComponent"/> from an SNBT string node.
    /// </summary>
    public static VillagerVariantComponent Parse(SnbtString node) => new(node.Value);

    /// <summary>
    /// Implicitly converts a variant name into a <see cref="VillagerVariantComponent"/>.
    /// </summary>
    public static implicit operator VillagerVariantComponent(string value) => new(value);

    [PublicAPI]
    public const string Desert = "desert", Jungle = "jungle", Plains = "plains", Savanna = "savanna",
                        Snow = "snow", Swamp = "swamp", Taiga = "taiga";
}

/// <summary>
/// Defines the collar dye color of a tamed wolf (<c>minecraft:wolf/collar</c>).
/// </summary>
/// <param name="Value">The collar dye color identifier.</param>
[UsedImplicitly]
public record WolfCollarComponent(string Value) : StringComponentBase(Value), IStringComponent<WolfCollarComponent>
{
    /// <inheritdoc/>
    public static string ComponentId => "minecraft:wolf/collar";

    /// <summary>
    /// Parses a <see cref="WolfCollarComponent"/> from an SNBT string node.
    /// </summary>
    public static WolfCollarComponent Parse(SnbtString node) => new(node.Value);

    /// <summary>
    /// Implicitly converts a dye color name into a <see cref="WolfCollarComponent"/>.
    /// </summary>
    public static implicit operator WolfCollarComponent(string value) => new(value);
}

/// <summary>
/// Defines the custom vocalization sound pack variant of a wolf (<c>minecraft:wolf/sound_variant</c>).
/// </summary>
/// <param name="Value">The sound variant resource identifier.</param>
[UsedImplicitly]
public record WolfSoundVariantComponent(string Value) : StringComponentBase(Value), IStringComponent<WolfSoundVariantComponent>
{
    /// <inheritdoc/>
    public static string ComponentId => "minecraft:wolf/sound_variant";

    /// <summary>
    /// Parses a <see cref="WolfSoundVariantComponent"/> from an SNBT string node.
    /// </summary>
    public static WolfSoundVariantComponent Parse(SnbtString node) => new(node.Value);

    /// <summary>
    /// Implicitly converts a sound variant identifier into a <see cref="WolfSoundVariantComponent"/>.
    /// </summary>
    public static implicit operator WolfSoundVariantComponent(string value) => new(value);
}

/// <summary>
/// Defines the biome-specific coat breed variant of a wolf (<c>minecraft:wolf/variant</c>).
/// </summary>
/// <param name="Value">The wolf coat variant resource identifier.</param>
[UsedImplicitly]
public record WolfVariantComponent(string Value) : StringComponentBase(Value), IStringComponent<WolfVariantComponent>
{
    /// <inheritdoc/>
    public static string ComponentId => "minecraft:wolf/variant";

    /// <summary>
    /// Parses a <see cref="WolfVariantComponent"/> from an SNBT string node.
    /// </summary>
    public static WolfVariantComponent Parse(SnbtString node) => new(node.Value);

    /// <summary>
    /// Implicitly converts a variant resource location into a <see cref="WolfVariantComponent"/>.
    /// </summary>
    public static implicit operator WolfVariantComponent(string value) => new(value);
}