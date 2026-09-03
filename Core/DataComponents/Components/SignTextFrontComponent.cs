using Core.DataComponents.Components.Base;
using Core.DataComponents.Interfaces;
using Core.SNBT.Nodes;
using Core.TextComponents.Components;
using JetBrains.Annotations;

namespace Core.DataComponents.Components;

/// <summary>
/// Configures text, color, and glow properties displayed on the front side of a sign (<c>minecraft:sign_text_front</c>).
/// </summary>
/// <param name="Messages">The text component lines displayed on the front face.</param>
/// <param name="FilteredMessages">Optional profanity-filtered text component lines used in Realms.</param>
/// <param name="Color">The dye color applied to the text. Defaults to <c>"black"</c>.</param>
/// <param name="HasGlowingText">Whether the front sign text has been dyed with a glow ink sac. Defaults to <see langword="false"/>.</param>
[UsedImplicitly]
public record SignTextFrontComponent(
    List<TextComponent> Messages,
    List<TextComponent>? FilteredMessages = null,
    string Color = SignTextComponentBase.DefaultColor,
    bool HasGlowingText = false
) : SignTextComponentBase(Messages, FilteredMessages, Color, HasGlowingText),
    ICompoundComponent<SignTextFrontComponent>
{
    /// <inheritdoc/>
    public static string ComponentId => "minecraft:sign_text_front";

    /// <summary>
    /// Initializes a new instance of the <see cref="SignTextFrontComponent"/> record from message lines.
    /// </summary>
    /// <param name="messages">The text component lines to display.</param>
    public SignTextFrontComponent(params TextComponent[] messages)
        : this(messages.ToList())
    {
    }

    /// <summary>
    /// Parses a <see cref="SignTextFrontComponent"/> from an SNBT compound node representation.
    /// </summary>
    /// <param name="compound">The SNBT compound node containing the front sign text configuration.</param>
    /// <returns>A populated <see cref="SignTextFrontComponent"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="compound"/> is missing the required <c>messages</c> list node.</exception>
    /// <example>
    /// <code>
    /// var node = SnbtParser.Parse("{messages: [\"Welcome\", \"to\", \"the\", \"server!\"], color: \"red\", has_glowing_text: 1b}");
    /// var frontText = SignTextFrontComponent.Parse(node);
    /// </code>
    /// </example>
    public static SignTextFrontComponent Parse(SnbtCompound compound)
    {
        var (messages, filteredMessages, color, hasGlowingText) = ParseProperties(compound);
        return new SignTextFrontComponent(messages, filteredMessages, color, hasGlowingText);
    }
}