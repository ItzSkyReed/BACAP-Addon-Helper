using System.Diagnostics.CodeAnalysis;

namespace BacapGenerator.Utils;

public static class BacapUtils
{
    /// <summary>
    /// Extracts the tab segment (first path component) from a resource location string.
    /// </summary>
    /// <param name="resourceLocation">The raw resource location (e.g., "blazeandcave:weaponry/kill_mob").</param>
    /// <returns>The extracted tab string, or <see langword="null"/> if the location is null or invalid.</returns>
    /// <example>
    /// <code>
    /// string? tab = BacapUtils.ExtractTab("blazeandcave:weaponry/kill_mob"); // "weaponry"
    /// </code>
    /// </example>
    public static string? ExtractTab(string? resourceLocation)
    {
        if (string.IsNullOrWhiteSpace(resourceLocation))
            return null;

        var tabSpan = ExtractTab(resourceLocation.AsSpan());
        return tabSpan.IsEmpty ? null : tabSpan.ToString();
    }

    /// <summary>
    /// Extracts the tab segment (first path component) from a resource location span with zero allocations.
    /// </summary>
    /// <param name="resourceLocation">The raw resource location span.</param>
    /// <returns>A span containing the tab name, or an empty span if invalid.</returns>
    /// <example>
    /// <code>
    /// ReadOnlySpan&lt;char&gt; tab = BacapUtils.ExtractTab("blazeandcave:weaponry/kill_mob".AsSpan());
    /// </code>
    /// </example>
    public static ReadOnlySpan<char> ExtractTab(ReadOnlySpan<char> resourceLocation)
    {
        if (resourceLocation.IsWhiteSpace())
            return ReadOnlySpan<char>.Empty;

        var path = MinecraftUtils.GetPathSpan(resourceLocation);
        var slashIndex = path.IndexOf('/');

        return slashIndex >= 0 ? path[..slashIndex] : path;
    }

    /// <summary>
    /// Tries to extract the tab segment from a resource location string.
    /// </summary>
    /// <param name="resourceLocation">The raw resource location (e.g., "blazeandcave:weaponry/kill_mob").</param>
    /// <param name="tab">When this method returns, contains the extracted tab name if successful; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the tab was successfully extracted; otherwise, <see langword="false"/>.</returns>
    /// <example>
    /// <code>
    /// if (BacapUtils.TryExtractTab("blazeandcave:weaponry/kill_mob", out var tab))
    /// {
    ///     Console.WriteLine(tab); // "weaponry"
    /// }
    /// </code>
    /// </example>
    public static bool TryExtractTab(string? resourceLocation, [NotNullWhen(true)] out string? tab)
    {
        tab = ExtractTab(resourceLocation);
        return tab is not null;
    }

    /// <summary>
    /// Tries to extract the tab segment from a resource location span with zero allocations.
    /// </summary>
    /// <param name="resourceLocation">The raw resource location span.</param>
    /// <param name="tab">When this method returns, contains the extracted tab span if successful; otherwise, an empty span.</param>
    /// <returns><see langword="true"/> if the tab was successfully extracted and is not empty; otherwise, <see langword="false"/>.</returns>
    /// <example>
    /// <code>
    /// if (BacapUtils.TryExtractTab("blazeandcave:weaponry/kill_mob".AsSpan(), out ReadOnlySpan&lt;char&gt; tab))
    /// {
    ///     // Process tab
    /// }
    /// </code>
    /// </example>
    public static bool TryExtractTab(ReadOnlySpan<char> resourceLocation, out ReadOnlySpan<char> tab)
    {
        tab = ExtractTab(resourceLocation);
        return !tab.IsEmpty;
    }
}