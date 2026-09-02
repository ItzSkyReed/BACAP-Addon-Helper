using System.Text.Json;
using System.Text.Json.Serialization;
using Core.Serialization;
using Core.SNBT;
using Core.TextComponents.Components;

namespace Core.TextComponents.Serialization;

/// <summary>
/// Allows System.Text.Json to read and write TextComponent,
/// reusing the powerful SNBT/TextComponent parser.
/// </summary>
public class TextComponentJsonConverter : JsonConverter<TextComponent>
{
    public override TextComponent Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var document = JsonDocument.ParseValue(ref reader);
        var rawJson = document.RootElement.GetRawText();

        try
        {
            var snbtNode = SnbtParser.Parse(rawJson);
            return TextComponent.Parse(snbtNode);
        }
        catch (Exception ex) when (ex is not JsonException)
        {
            // Throw a descriptive error including the raw JSON snippet that caused the failure
            throw new JsonException($"Failed to parse TextComponent from the provided JSON. Raw JSON: {rawJson}", ex);
        }
    }

    public override void Write(Utf8JsonWriter writer, TextComponent value, JsonSerializerOptions options)
    {
        value.ToSnbt().WriteTo(writer);
    }
}