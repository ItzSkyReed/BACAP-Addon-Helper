using Core.DataComponents.Components.Base;
using Core.DataComponents.Interfaces;
using Core.SNBT.Nodes;
using Core.TextComponents.Components;
using JetBrains.Annotations;

namespace Core.DataComponents.Components;

/// <summary>
/// Configures text, color, and glow properties displayed on the back side of a sign (<c>minecraft:sign_text_back</c>).
/// </summary>
/// <param name="Messages">The text component lines displayed on the back face.</param>
/// <param name="FilteredMessages">Optional profanity-filtered text component lines used in Realms.</param>
/// <param name="Color">The dye color applied to the text. Defaults to <c>"black"</c>.</param>
/// <param name="HasGlowingText">Whether the back sign text has been dyed with a glow ink sac. Defaults to <see langword="false"/>.</param>
[UsedImplicitly]
public record SignTextBackComponent(
    List<TextComponent> Messages,
    List<TextComponent>? FilteredMessages = null,
    string Color = SignTextComponentBase.DefaultColor,
    bool HasGlowingText = false
) : SignTextComponentBase(Messages, FilteredMessages, Color, HasGlowingText),
    ICompoundComponent<SignTextBackComponent>
{
    /// <inheritdoc/>
    public static string ComponentId => "minecraft:sign_text_back";

    /// <summary>
    /// Initializes a new instance of the <see cref="SignTextBackComponent"/> record from message lines.
    /// </summary>
    /// <param name="messages">The text component lines to display.</param>
    public SignTextBackComponent(params TextComponent[] messages)
        : this(messages.ToList())
    {
    }

    /// <summary>
    /// Parses a <see cref="SignTextBackComponent"/> from an SNBT compound node representation.
    /// </summary>
    /// <param name="compound">The SNBT compound node containing the back sign text configuration.</param>
    /// <returns>A populated <see cref="SignTextBackComponent"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="compound"/> is missing the required <c>messages</c> list node.</exception>
    /// <example>
    /// <code>
    /// var node = SnbtParser.Parse("{messages: [\"Private\", \"Staff\", \"Only\", \"\"], color: \"yellow\"}");
    /// var backText = SignTextBackComponent.Parse(node);
    /// </code>
    /// </example>
    public static SignTextBackComponent Parse(SnbtCompound compound)
    {
        var (messages, filteredMessages, color, hasGlowingText) = ParseProperties(compound);
        return new SignTextBackComponent(messages, filteredMessages, color, hasGlowingText);
    }
}