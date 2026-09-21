using JetBrains.Annotations;

namespace BacapGenerator.Configuration.Exceptions;

/// <summary>
/// Specifies the failure classification when expanding configuration templates.
/// </summary>
public enum ConfigurationTemplateErrorKind
{
    /// <summary>The referenced template was not found under the 'templates' configuration section.</summary>
    TemplateNotFound,

    /// <summary>A circular inheritance chain was detected among configuration templates.</summary>
    CircularDependency
}

/// <summary>
/// Exception thrown when configuration template inheritance cannot be resolved.
/// </summary>
[PublicAPI]
public sealed class ConfigurationTemplateException : Exception
{
    /// <summary>
    /// Gets the name of the referenced template (e.g., "default").
    /// </summary>
    public string TemplateName { get; }

    /// <summary>
    /// Gets the configuration section path referencing the template (e.g., "datapacks:bacaped:validation").
    /// </summary>
    public string TargetSectionPath { get; }

    /// <summary>
    /// Gets the expected path where the template was searched for (e.g., "templates:validation:default").
    /// </summary>
    public string ExpectedTemplatePath { get; }

    /// <summary>
    /// Gets the failure category.
    /// </summary>
    public ConfigurationTemplateErrorKind Kind { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationTemplateException"/> class.
    /// </summary>
    public ConfigurationTemplateException(
        string message,
        ConfigurationTemplateErrorKind kind,
        string templateName,
        string targetSectionPath,
        string expectedTemplatePath)
        : base(message)
    {
        Kind = kind;
        TemplateName = templateName;
        TargetSectionPath = targetSectionPath;
        ExpectedTemplatePath = expectedTemplatePath;
    }
}