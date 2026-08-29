using System.Runtime.InteropServices;
using Core.Commands.Models;
using Core.DataComponents.Models;
using Core.SNBT.Interfaces;
using Core.SNBT.Nodes;
using Core.TextComponents.Components;
using Core.TextComponents.Events;
using Core.TextComponents.Models;
using JetBrains.Annotations;

namespace Core.TextComponents;

/// <summary>
/// Provides deserialization methods to convert SNBT (Stringified NBT) nodes
/// into strongly-typed Minecraft raw text components (<see cref="TextComponent"/>).
/// </summary>
public static class TextComponentParser
{
    /// <summary>
    /// Parses an <see cref="ISnbtNode"/> into a concrete <see cref="TextComponent"/>.
    /// Supports shorthand strings, list structures, and full compound objects.
    /// </summary>
    /// <param name="node">The SNBT node representing a text component.</param>
    /// <returns>A deserialized <see cref="TextComponent"/> instance, or an empty <see cref="PlainTextComponent"/> if the node is unsupported.</returns>
    [PublicAPI]
    public static TextComponent Parse(ISnbtNode node)
    {
        return node switch
        {
            // Shorthand 1: A raw string "Hello" is equivalent to {text: "Hello"}
            SnbtString str => new PlainTextComponent(str.Value),

            // Shorthand 2: A list ["A", "B"] is equivalent to {text: "A", extra: ["B"]}
            SnbtList list => ParseListShorthand(list),

            // Standard representation: Full compound object
            SnbtCompound compound => ParseCompound(compound),

            // Fallback for invalid or null data to prevent runtime failures
            _ => new PlainTextComponent("")
        };
    }

    /// <summary>
    /// Parses an SNBT list shorthand into a root component with trailing siblings inside <see cref="TextComponent.Extra"/>.
    /// </summary>
    private static TextComponent ParseListShorthand(SnbtList list)
    {
        var itemsSpan = CollectionsMarshal.AsSpan(list.Items);

        if (itemsSpan.Length == 0)
            return new PlainTextComponent("");

        // The first item defines the root component
        var root = Parse(itemsSpan[0]);

        if (itemsSpan.Length <= 1)
            return root;

        var existingExtra = root.Extra;
        var existingCount = existingExtra?.Count ?? 0;
        var newCount = itemsSpan.Length - 1;

        var extraList = new List<TextComponent>(existingCount + newCount);

        if (existingExtra != null)
        {
            // Use Span to avoid enumerator allocation
            foreach (var existing in CollectionsMarshal.AsSpan(existingExtra))
                extraList.Add(existing);
        }

        for (var i = 1; i < itemsSpan.Length; i++)
            extraList.Add(Parse(itemsSpan[i]));

        return root with { Extra = extraList };
    }

    /// <summary>
    /// Parses an <see cref="SnbtCompound"/> into its specific component variant based on explicit type or discriminator tags.
    /// </summary>
    /// <summary>
    /// Parses an <see cref="SnbtCompound"/> into its specific component variant based on explicit type or discriminator tags.
    /// </summary>
    private static TextComponent ParseCompound(SnbtCompound compound)
    {
        // Parse component style formatting
        var style = ParseStyle(compound);

        // Parse the 'extra' collection without LINQ .Select().ToList()
        List<TextComponent>? extra = null;
        if (compound.GetNode("extra") is SnbtList extraList)
        {
            var extraSpan = CollectionsMarshal.AsSpan(extraList.Items);
            if (extraSpan.Length > 0)
            {
                extra = new List<TextComponent>(extraSpan.Length);
                foreach (var item in extraSpan)
                {
                    extra.Add(Parse(item));
                }
            }
        }

        // Cache dictionary reference to avoid repeated property access
        var tags = compound.Tags;

        // Resolve component type via explicit 'type' field or fallback to property presence
        var explicitType = compound.GetString("type");

        switch (explicitType)
        {
            case "text":
            case "" when tags.ContainsKey("text"):
                return new PlainTextComponent(compound.GetString("text"), style, extra);

            case "translatable":
            case "" when tags.ContainsKey("translate"):
            {
                List<TextComponent>? with = null;
                if (compound.GetNode("with") is SnbtList withList)
                {
                    var withSpan = CollectionsMarshal.AsSpan(withList.Items);
                    if (withSpan.Length > 0)
                    {
                        with = new List<TextComponent>(withSpan.Length);
                        foreach (var item in withSpan)
                        {
                            with.Add(Parse(item));
                        }
                    }
                }

                return new TranslatableComponent(
                    Translate: compound.GetString("translate"),
                    Fallback: compound.GetOptionalString("fallback"), // Removed double lookup
                    With: with,
                    Style: style,
                    Extra: extra
                );
            }

            case "score":
            case "" when tags.ContainsKey("score"):
            {
                if (compound.GetNode("score") is SnbtCompound scoreComp)
                {
                    return new ScoreComponent(
                        Name: scoreComp.GetString("name"),
                        Objective: scoreComp.GetString("objective"),
                        Style: style,
                        Extra: extra
                    );
                }

                break;
            }

            case "selector":
            case "" when tags.ContainsKey("selector"):
            {
                var sepNode = compound.GetNode("separator");
                return new SelectorComponent(
                    Selector: Selector.Custom(compound.GetString("selector")),
                    Separator: sepNode != null ? Parse(sepNode) : null, // Removed double lookup
                    Style: style,
                    Extra: extra
                );
            }

            case "keybind":
            case "" when tags.ContainsKey("keybind"):
                return new KeybindComponent(compound.GetString("keybind"), style, extra);

            case "nbt":
            case "" when tags.ContainsKey("nbt"):
            {
                var sourceType = NbtDataSource.Entity;
                var sourceTarget = "";

                if (tags.ContainsKey("block"))
                {
                    sourceType = NbtDataSource.Block;
                    sourceTarget = compound.GetString("block");
                }
                else if (tags.ContainsKey("storage"))
                {
                    sourceType = NbtDataSource.Storage;
                    sourceTarget = compound.GetString("storage");
                }
                else if (tags.ContainsKey("entity"))
                {
                    sourceType = NbtDataSource.Entity;
                    sourceTarget = compound.GetString("entity");
                }

                var sepNode = compound.GetNode("separator");
                var separator = sepNode != null ? Parse(sepNode) : null; // Removed double lookup

                // Читаем interpret с поддержкой и байтов, и новых SnbtBool
                var interpret = compound.GetNode("interpret") switch
                {
                    SnbtBool bl => bl.Value,
                    SnbtByte b => b.Value == 1,
                    _ => false
                };

                return new NbtComponent(
                    NbtPath: compound.GetString("nbt"),
                    SourceType: sourceType,
                    SourceTarget: sourceTarget,
                    Interpret: interpret,
                    Separator: separator,
                    Style: style,
                    Extra: extra
                );
            }

            case "object":
            case "" when tags.ContainsKey("object"):
            {
                var objectType = compound.GetString("object", "atlas");

                switch (objectType)
                {
                    case "atlas":
                        return new AtlasComponent(
                            Sprite: compound.GetString("sprite"),
                            Atlas: compound.GetOptionalString("atlas"),
                            Style: style,
                            Extra: extra
                        );
                    case "player":
                    {
                        var playerNode = compound.GetNode("player");

                        switch (playerNode)
                        {
                            // Handle shorthand string format (e.g. {object: "player", player: "Notch"})
                            case SnbtString playerStr:
                                return new PlayerObjectComponent(Name: playerStr.Value, Style: style, Extra: extra);
                            case SnbtCompound playerComp:
                            {
                                // Parse ID without LINQ
                                int[]? idArray = null;
                                if (playerComp.GetNode("id") is SnbtIntArray snbtIntArray)
                                {
                                    var intSpan = CollectionsMarshal.AsSpan(snbtIntArray.Items);
                                    idArray = new int[intSpan.Length];
                                    for (var i = 0; i < intSpan.Length; i++)
                                    {
                                        idArray[i] = intSpan[i] is SnbtInt val ? val.Value : 0;
                                    }
                                }

                                // Parse Properties without LINQ
                                List<ProfileProperty>? properties = null;
                                if (playerComp.GetNode("properties") is SnbtList propList)
                                {
                                    var propSpan = CollectionsMarshal.AsSpan(propList.Items);
                                    if (propSpan.Length > 0)
                                    {
                                        properties = new List<ProfileProperty>(propSpan.Length);
                                        foreach (var propNode in propSpan)
                                        {
                                            if (propNode is SnbtCompound propComp)
                                                properties.Add(ProfileProperty.Parse(propComp));
                                        }
                                    }
                                }

                                return new PlayerObjectComponent(
                                    Name: playerComp.GetOptionalString("name"),
                                    Id: idArray,
                                    Properties: properties,
                                    Texture: playerComp.GetOptionalString("texture"),
                                    Cape: playerComp.GetOptionalString("cape"),
                                    Elytra: playerComp.GetOptionalString("elytra"),
                                    Model: playerComp.GetOptionalString("model"),
                                    Hat: playerComp.GetOptionalBool("hat"),
                                    Style: style,
                                    Extra: extra
                                );
                            }
                        }

                        break;
                    }
                }

                break;
            }
        }

        return new PlainTextComponent("", style, extra);
    }

    /// <summary>
    /// Extracts font styling, formatting flags, and interaction events from an SNBT compound.
    /// </summary>
    private static TextStyle? ParseStyle(SnbtCompound compound)
    {
        var tags = compound.Tags;

        // Fast early exit to prevent 10 dictionary lookups if empty
        if (tags.Count == 0) return null;

        var hasStyle = tags.ContainsKey("color") || tags.ContainsKey("font") ||
                       tags.ContainsKey("bold") || tags.ContainsKey("italic") ||
                       tags.ContainsKey("underlined") || tags.ContainsKey("strikethrough") ||
                       tags.ContainsKey("obfuscated") || tags.ContainsKey("insertion") ||
                       tags.ContainsKey("click_event") || tags.ContainsKey("hover_event");

        if (!hasStyle) return null;

        return new TextStyle(
            Color: compound.GetOptionalString("color"),
            Font: compound.GetOptionalString("font"),
            Bold: compound.GetOptionalBool("bold"),
            Italic: compound.GetOptionalBool("italic"),
            Underlined: compound.GetOptionalBool("underlined"),
            Strikethrough: compound.GetOptionalBool("strikethrough"),
            Obfuscated: compound.GetOptionalBool("obfuscated"),
            Insertion: compound.GetOptionalString("insertion"),
            ClickEvent: ParseClickEvent(compound.GetNode("click_event") as SnbtCompound),
            HoverEvent: ParseHoverEvent(compound.GetNode("hover_event") as SnbtCompound)
        );
    }

    /// <summary>
    /// Parses an action-based click event definition from an SNBT compound.
    /// </summary>
    private static ClickEvent? ParseClickEvent(SnbtCompound? comp)
    {
        if (comp == null) return null;
        var action = comp.GetString("action");

        return action switch
        {
            "open_url" => new OpenUrlClickEvent(comp.GetString("url")),
            "open_file" => new OpenFileClickEvent(comp.GetString("path")),
            "run_command" => new RunCommandClickEvent(comp.GetString("command")),
            "suggest_command" => new SuggestCommandClickEvent(comp.GetString("command")),
            "change_page" => new ChangePageClickEvent(comp.GetInt("page")),
            "copy_to_clipboard" => new CopyToClipboardClickEvent(comp.GetString("value")),
            "show_dialog" => new ShowDialogClickEvent(comp.GetNode("dialog")!),
            "custom" => new CustomClickEvent(comp.GetString("id"), comp.GetOptionalString("payload")), // Avoid double lookup
            _ => null
        };
    }

    /// <summary>
    /// Parses an action-based hover event definition from an SNBT compound.
    /// </summary>
    private static HoverEvent? ParseHoverEvent(SnbtCompound? comp)
    {
        if (comp == null) return null;
        var action = comp.GetString("action");

        return action switch
        {
            "show_text" => new ShowTextHoverEvent(Parse(comp.GetNode("value")!)),

            "show_item" => new ShowItemHoverEvent(
                Id: comp.GetString("id"),
                Count: comp.Tags.ContainsKey("count") ? comp.GetInt("count") : null,
                Components: comp.GetNode("components") as SnbtCompound
            ),

            "show_entity" => new ShowEntityHoverEvent(
                Id: comp.GetString("id"),
                Uuid: comp.GetNode("uuid")!,
                Name: comp.GetNode("name") is { } nameNode ? Parse(nameNode) : null
            ),
            _ => null
        };
    }
}