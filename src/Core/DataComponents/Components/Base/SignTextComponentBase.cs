using Core.SNBT;
using Core.SNBT.Interfaces;
using Core.SNBT.Nodes;
using Core.TextComponents.Components;

namespace Core.DataComponents.Components.Base;

/// <summary>
/// Abstract base record encapsulating sign face text configuration, dyeing color, and formatting properties.
/// </summary>
/// <param name="Messages">The four text component lines displayed on the sign face.</param>
/// <param name="FilteredMessages">Optional profanity-filtered text component lines used in Realms.</param>
/// <param name="Color">The dye color applied to the text (e.g., <c>"black"</c>, <c>"white"</c>, <c>"red"</c>). Defaults to <c>"black"</c>.</param>
/// <param name="HasGlowingText">Whether the sign text was dyed with a glow ink sac to produce a luminous effect. Defaults to <see langword="false"/>.</param>
public abstract record SignTextComponentBase(
    List<TextComponent> Messages,
    List<TextComponent>? FilteredMessages = null,
    string Color = SignTextComponentBase.DefaultColor,
    bool HasGlowingText = false
) : ISnbtSerializable
{
    /// <summary>
    /// The default dye color for sign text.
    /// </summary>
    public const string DefaultColor = "black";

    /// <summary>
    /// Parses common sign text properties from an SNBT compound node.
    /// </summary>
    /// <param name="compound">The SNBT compound node to parse.</param>
    /// <returns>A tuple containing the parsed messages, filtered messages, color, and glowing flag.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="compound"/> does not contain a valid <c>messages</c> list node.</exception>
    protected static (List<TextComponent> Messages, List<TextComponent>? FilteredMessages, string Color, bool HasGlowingText) ParseProperties(SnbtCompound compound)
    {
        if (compound.GetNode("messages") is not SnbtList messagesList)
            throw new ArgumentException("Sign text component requires a 'messages' list.", nameof(compound));

        var messages = new List<TextComponent>(messagesList.Items.Count);

        messages.AddRange(messagesList.Items.Select(TextComponent.Parse));

        List<TextComponent>? filteredMessages = null;
        if (compound.GetNode("filtered_messages") is SnbtList filteredList)
        {
            filteredMessages = new List<TextComponent>(filteredList.Items.Count);
            filteredMessages.AddRange(filteredList.Items.Select(TextComponent.Parse));
        }

        var color = compound.GetString("color", DefaultColor);
        var hasGlowingText = compound.GetBool("has_glowing_text");

        return (messages, filteredMessages, color, hasGlowingText);
    }

    /// <summary>
    /// Serializes the sign face text configuration into an SNBT compound node.
    /// </summary>
    /// <returns>An <see cref="ISnbtNode"/> containing the sign text compound.</returns>
    public virtual ISnbtNode ToSnbt()
    {
        var builder = Snbt.Compound();

        var messagesList = new SnbtList();
        foreach (var message in Messages)
            messagesList.Items.Add(message.ToSnbt());
        builder.Put("messages", messagesList);

        if (FilteredMessages is { Count: > 0 })
        {
            var filteredList = new SnbtList();
            foreach (var message in FilteredMessages)
                filteredList.Items.Add(message.ToSnbt());
            builder.Put("filtered_messages", filteredList);
        }

        builder.PutOptional("color", Color, DefaultColor);
        builder.PutOptional("has_glowing_text", HasGlowingText, false);

        return builder.Build();
    }
}