using BacapGenerator.Configuration.Exceptions;
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
    /// <exception cref="ConfigurationTemplateException">Thrown when a template is missing or contains circular dependencies.</exception>
    /// <example>
    /// <code>
    /// var config = new ConfigurationBuilder()
    ///     .AddYamlFile("config.yaml")
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
            foreach (var childSection in packSection.GetChildren())
            {
                // Case 1: Direct feature section (e.g. datapacks:bacaped:validation)
                if (childSection.GetSection("template").Exists())
                {
                    ApplyTemplateInheritance(tempConfig, childSection, childSection.Key, overlays);
                }
                // Case 2: Array of feature objects (e.g. datapacks:bacaped:checklists:0)
                else
                {
                    foreach (var itemSection in childSection.GetChildren())
                    {
                        if (itemSection.GetSection("template").Exists())
                        {
                            ApplyTemplateInheritance(tempConfig, itemSection, childSection.Key, overlays);
                        }
                    }
                }
            }
        }

        if (overlays.Count > 0)
            builder.AddInMemoryCollection(overlays);

        return builder;
    }

    private static void ApplyTemplateInheritance(
        IConfigurationRoot rootConfig,
        IConfigurationSection targetSection,
        string categoryName,
        Dictionary<string, string?> overlays)
    {
        var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var currentSection = targetSection;

        while (true)
        {
            var templateName = currentSection["template"];
            if (string.IsNullOrWhiteSpace(templateName))
                break;

            var (templateSection, templatePath) = ResolveTemplateSection(rootConfig, categoryName, templateName, currentSection.Path);

            if (!visited.Add(templatePath))
            {
                throw new ConfigurationTemplateException(
                    $"Circular template dependency detected involving '{templateName}' at path '{templatePath}'.",
                    ConfigurationTemplateErrorKind.CircularDependency,
                    templateName,
                    currentSection.Path,
                    templatePath);
            }

            var templatePrefix = $"{templatePath}:";

            foreach (var (key, value) in rootConfig.AsEnumerable())
            {
                if (!key.StartsWith(templatePrefix, StringComparison.OrdinalIgnoreCase) || value is null)
                    continue;

                var relativeKey = key[templatePrefix.Length..];
                var targetKey = $"{targetSection.Path}:{relativeKey}";

                // If target already defines elements of this array, skip template array defaults
                if (IsArrayElement(relativeKey, out var arrayParentPath))
                {
                    var targetArraySection = targetSection.GetSection(arrayParentPath);
                    if (targetArraySection.GetChildren().Any())
                        continue;
                }

                // Apply template value only if not explicitly overridden
                if (rootConfig[targetKey] is null && !overlays.ContainsKey(targetKey))
                {
                    overlays[targetKey] = value;
                }
            }

            // Support chained templates (templates inheriting from other templates)
            currentSection = templateSection;
        }
    }

    private static (IConfigurationSection Section, string Path) ResolveTemplateSection(
        IConfigurationRoot rootConfig,
        string category,
        string templateName,
        string targetSectionPath)
    {
        // Try exact match (e.g. templates:validation:default)
        var path = $"templates:{category}:{templateName}";
        var section = rootConfig.GetSection(path);

        if (section.Exists())
            return (section, path);

        // Try singular/plural normalization fallback (e.g. checklists -> checklist)
        if (category.EndsWith('s'))
        {
            var singularCategory = category[..^1];
            var singularPath = $"templates:{singularCategory}:{templateName}";
            var singularSection = rootConfig.GetSection(singularPath);

            if (singularSection.Exists())
                return (singularSection, singularPath);
        }

        throw new ConfigurationTemplateException(
            $"Template '{templateName}' for category '{category}' referenced in '{targetSectionPath}' was not found.",
            ConfigurationTemplateErrorKind.TemplateNotFound,
            templateName,
            targetSectionPath,
            path);
    }

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