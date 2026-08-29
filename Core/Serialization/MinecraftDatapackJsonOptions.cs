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
        IndentSize = 4,
        MaxDepth = 256,
    };
}