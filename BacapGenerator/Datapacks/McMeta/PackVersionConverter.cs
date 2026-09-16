using System.Text.Json;
using System.Text.Json.Serialization;

namespace BacapGenerator.Datapacks.McMeta;

/// <summary>
/// Handles reading and writing <see cref="PackVersion"/> from either a single integer or an array of integers.
/// </summary>
public sealed class PackVersionConverter : JsonConverter<PackVersion>
{
    public override PackVersion Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.Number:
                return new PackVersion(reader.GetInt32());
            case JsonTokenType.StartArray:
            {
                reader.Read(); // move to first element
                var major = reader.GetInt32();

                reader.Read(); // move to second element
                var minor = reader.TokenType == JsonTokenType.Number ? reader.GetInt32() : 0;

                // Advance to the end of the array
                while (reader.TokenType != JsonTokenType.EndArray)
                    reader.Read();

                return new PackVersion(major, minor);
            }
            case JsonTokenType.None:
            case JsonTokenType.StartObject:
            case JsonTokenType.EndObject:
            case JsonTokenType.EndArray:
            case JsonTokenType.PropertyName:
            case JsonTokenType.Comment:
            case JsonTokenType.String:
            case JsonTokenType.True:
            case JsonTokenType.False:
            case JsonTokenType.Null:
            default:
                throw new JsonException("Expected an integer or an array of integers for the pack format version.");
        }
    }

    public override void Write(Utf8JsonWriter writer, PackVersion value, JsonSerializerOptions options)
    {
        if (value.Minor == 0)
            writer.WriteNumberValue(value.Major);
        else
        {
            writer.WriteStartArray();
            writer.WriteNumberValue(value.Major);
            writer.WriteNumberValue(value.Minor);
            writer.WriteEndArray();
        }
    }
}