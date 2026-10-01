using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Core.Serialization;

/// <summary>
/// Provides global, optimized JSON serialization options for Minecraft-related data (SNBT, Advancements, etc.).
/// </summary>
public static class MinecraftDatapackJsonOptions
{
    /// <summary>
    /// The default JSON options enforcing snake_case naming and ignoring null values.
    /// </summary>
    public static readonly JsonSerializerOptions Default = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = true,
        IndentSize = 2,
        MaxDepth = 256,

        // Disables aggressive HTML-escaping, keeping characters like ' and & as they are
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    /// <summary>
    /// The default JSON options enforcing snake_case naming and ignoring null values.
    /// </summary>
    public static readonly JsonSerializerOptions BaseTranslation = new()
    {
        WriteIndented = true,
        IndentSize = 2,

        // Disables aggressive HTML-escaping, keeping characters like ' and & as they are
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public static readonly JsonSerializerOptions Documentation = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public static readonly JsonSerializerOptions RegistryLoader = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static readonly JsonSerializerOptions DatapackIoManager = new()
    {
        WriteIndented = true
    };
}