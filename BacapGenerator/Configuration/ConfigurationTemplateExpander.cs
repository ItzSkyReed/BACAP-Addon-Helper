using Microsoft.Extensions.Configuration;

namespace BacapGenerator.Configuration;

/// <summary>
/// Provides extension methods for <see cref="IConfigurationBuilder"/> to perform deep inheritance
/// and expansion of configured templates across datapack configurations.
/// </summary>
public static class ConfigurationTemplateExpander
{
    /// <summary>
    /// Traverses all registered datapacks, resolves referenced templates from the <c>templates</c> root section,
    /// and applies deep inheritance for missing configuration keys without overwriting explicit overrides.
    /// </summary>
    /// <param name="builder">The configuration builder to enhance.</param>
    /// <returns>The same <see cref="IConfigurationBuilder"/> instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">Thrown when a referenced template does not exist.</exception>
    /// <example>
    /// <code>
    /// var config = new ConfigurationBuilder()
    ///     .AddYamlFile("config.yml")
    ///     .ExpandTemplates()
    ///     .Build();
    /// </code>
    /// </example>
    public static IConfigurationBuilder ExpandTemplates(this IConfigurationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        // Build temporary snapshot to evaluate raw configuration keys
        var tempConfig = builder.Build();
        var overlays = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

        var datapacksSection = tempConfig.GetSection("datapacks");
        if (!datapacksSection.Exists())
            return builder;

        foreach (var packSection in datapacksSection.GetChildren())
        {
            foreach (var featureSection in packSection.GetChildren())
            {
                var templateName = featureSection["template"];
                if (string.IsNullOrWhiteSpace(templateName))
                    continue;

                var featureType = featureSection.Key; // e.g. "validation" or "language_pack"
                var templatePath = $"templates:{featureType}:{templateName}";
                var templateSection = tempConfig.GetSection(templatePath);

                if (!templateSection.Exists())
                {
                    throw new InvalidOperationException(
                        $"Template '{templateName}' referenced in '{featureSection.Path}' was not found at '{templatePath}'.");
                }

                var templatePrefix = $"{templatePath}:";

                foreach (var (key, value) in tempConfig.AsEnumerable())
                {
                    if (!key.StartsWith(templatePrefix, StringComparison.OrdinalIgnoreCase) || value is null)
                        continue;

                    var relativeKey = key[templatePrefix.Length..];
                    var targetKey = $"{featureSection.Path}:{relativeKey}";

                    // If target already defines elements of this array, do not append remaining template items
                    if (IsArrayElement(relativeKey, out var arrayParentPath))
                    {
                        var targetArraySection = featureSection.GetSection(arrayParentPath);
                        if (targetArraySection.GetChildren().Any())
                            continue;
                    }

                    // Overlay key only if not explicitly defined by the datapack
                    if (tempConfig[targetKey] is null)
                        overlays[targetKey] = value;
                }
            }
        }

        if (overlays.Count > 0)
            builder.AddInMemoryCollection(overlays);


        return builder;
    }

    /// <summary>
    /// Checks whether the relative configuration path represents an indexed element of an array.
    /// </summary>
    private static bool IsArrayElement(string relativeKey, out string arrayParentPath)
    {
        var lastColonIndex = relativeKey.LastIndexOf(':');
        if (lastColonIndex >= 0)
        {
            var lastSegment = relativeKey[(lastColonIndex + 1)..];
            if (int.TryParse(lastSegment, out _))
            {
                arrayParentPath = relativeKey[..lastColonIndex];
                return true;
            }
        }
        else if (int.TryParse(relativeKey, out _))
        {
            arrayParentPath = string.Empty;
            return true;
        }

        arrayParentPath = string.Empty;
        return false;
    }
}