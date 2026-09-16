using System.Text.Json.Serialization;

namespace BacapGenerator.Datapacks.McMeta;

public class PackMcMeta
{
    /// <summary>
    /// Holds the core pack information.
    /// </summary>
    [JsonPropertyName("pack")]
    public required PackInformation Pack { get; init; }

    /// <summary>
    /// Section for selecting experimental features.
    /// </summary>
    [JsonPropertyName("features")]
    public PackFeatures? Features { get; init; }

    /// <summary>
    /// Section for filtering out files from packs applied below this one.
    /// </summary>
    [JsonPropertyName("filter")]
    public PackFilter? Filter { get; init; }

    /// <summary>
    /// Section for specifying the overlays, which are sub-packs applied over the "normal" contents.
    /// </summary>
    [JsonPropertyName("overlays")]
    public PackOverlays? Overlays { get; init; }

    /// <summary>
    /// Contains additional languages to add to the language menu. Only present in resource packs.
    /// The key is the language code corresponding to a .json file in assets/&lt;namespace&gt;/lang.
    /// </summary>
    [JsonPropertyName("language")]
    public Dictionary<string, PackLanguageInfo>? Language { get; init; }
}