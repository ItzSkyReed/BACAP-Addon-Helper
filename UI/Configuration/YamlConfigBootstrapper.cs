using System.Reflection;

namespace UI.Configuration;

public static class YamlConfigBootstrapper
{
    /// <summary>
    /// Ensures that the configuration file exists on the disk.
    /// If it is missing, it extracts the default configuration from the embedded assembly resources,
    /// saves it to the disk, and throws an exception to prevent the application from running with unconfigured settings.
    /// </summary>
    /// <param name="targetFilePath">The path where the configuration file should be located on the disk.</param>
    /// <param name="resourceName">The full name of the embedded resource (usually "ProjectNamespace.FileName").</param>
    /// <exception cref="InvalidOperationException">Thrown when the embedded resource cannot be found in the assembly.</exception>
    /// <exception cref="FileNotFoundException">Thrown intentionally after creating the default file to force the user to configure the application.</exception>
    /// <example>
    /// <code>
    /// YamlConfigBootstrapper.EnsureConfigExists("config.yaml", "BacapGenerator.default_config.yaml");
    /// </code>
    /// </example>
    public static void EnsureConfigExists(string targetFilePath, string resourceName)
    {
        // If the file already exists, we just proceed.
        if (File.Exists(targetFilePath))
            return;

        // Get the current assembly to extract the embedded resource
        var assembly = Assembly.GetExecutingAssembly();

        using var resourceStream = assembly.GetManifestResourceStream(resourceName);

        if (resourceStream == null)
        {
            throw new InvalidOperationException(
                $"Cannot find embedded resource '{resourceName}'. " +
                $"Make sure it is marked as EmbeddedResource in the .csproj file.");
        }

        // Create the file on disk and copy the stream content
        using FileStream fileStream = new(targetFilePath, FileMode.Create, FileAccess.Write);
        resourceStream.CopyTo(fileStream);

        // Fail-fast: Stop execution and notify the user to fill the configuration
        throw new FileNotFoundException(
            $"The configuration file '{targetFilePath}' was not found. " +
            $"A default template has been generated in the application directory. " +
            $"Please configure it and restart the application.",
            targetFilePath);
    }
}