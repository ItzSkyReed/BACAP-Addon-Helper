using System.Text.Json.Serialization;
using Core.Items;
using Core.Items.Serialization;
using Core.TextComponents.Components;
using Core.TextComponents.Serialization;

namespace Core.Advancements.Models;

/// <summary>
/// Data related to the advancement's display in the UI.
/// </summary>
public record AdvancementDisplay
{
    /// <summary>
    /// Object containing data for the advancement's icon.
    /// Reuses the game's native ItemStack model.
    /// </summary>
    [JsonPropertyName("icon")]
    [JsonConverter(typeof(ItemStackJsonConverter))]
    public ItemStack? Icon { get; init; } = null!;

    [JsonPropertyName("title")]
    [JsonConverter(typeof(TextComponentJsonConverter))]
    public TextComponent? Title { get; init; }

    [JsonPropertyName("description")]
    [JsonConverter(typeof(TextComponentJsonConverter))]
    public TextComponent? Description { get; init; }

    [JsonPropertyName("frame")] public AdvancementFrame? Frame { get; init; } = AdvancementFrame.Task;

    [JsonIgnore]
    public bool ShowToast { get; init; } = true;

    [JsonPropertyName("show_toast")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? ShowToastJson => ShowToast ? null : false;

    [JsonPropertyName("announce_to_chat")]
    public bool AnnounceToChat { get; init; } = true;

    [JsonPropertyName("hidden")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool Hidden { get; init; }
}