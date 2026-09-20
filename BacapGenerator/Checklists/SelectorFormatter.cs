namespace BacapGenerator.Checklists;

/// <summary>
/// Utility for constructing sanitized Minecraft entity target selectors.
/// </summary>
public static class SelectorFormatter
{
    /// <summary>
    /// Builds a formatted entity selector string with distance and optional extra selector arguments.
    /// Handles commas and whitespace automatically.
    /// </summary>
    /// <param name="entityId">The entity identifier (e.g. "cow").</param>
    /// <param name="distance">The maximum distance radius.</param>
    /// <param name="extraSelectors">Optional additional selector predicates or parameters.</param>
    /// <returns>A formatted Minecraft target selector string.</returns>
    /// <example>
    /// <code>
    /// // Returns "@e[type=minecraft:cow,distance=..32,predicate=bacaped:is_baby]"
    /// SelectorFormatter.BuildEntitySelector("cow", 32, "predicate=bacaped:is_baby");
    /// </code>
    /// </example>
    public static string BuildEntitySelector(string entityId, int distance, params string?[] extraSelectors)
    {
        var parameters = new List<string>
        {
            $"type=minecraft:{entityId}",
            $"distance=..{distance}"
        };
        parameters.AddRange(
            from extra in extraSelectors
            where !string.IsNullOrWhiteSpace(extra)
            select extra.Trim().Trim(',')
            into sanitized
            where !string.IsNullOrEmpty(sanitized) select sanitized);

        return $"@e[{string.Join(',', parameters)}]";
    }
}