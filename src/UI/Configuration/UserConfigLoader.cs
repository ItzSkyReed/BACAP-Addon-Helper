using dotenv.net;

namespace UI.Configuration;

/// <summary>
/// Provides functionality to load and construct <see cref="UserConfig"/> from system environment variables and an optional .env file.
/// </summary>
public static class UserConfigLoader
{
    private const string WorldDatapacksEnvKey = "WORLD_DATAPACKS_PATH";
    private const string WorldCompatDatapacksEnvKey = "WORLD_COMPAT_DATAPACKS_PATH";
    private const string ResourcePacksEnvKey = "RESOURCE_PACKS_PATH";

    /// <summary>
    /// Loads configuration settings, prioritizing actual system environment variables over values defined in the .env file.
    /// </summary>
    /// <param name="envFilePath">The file path to the environment file. Defaults to <c>".env"</c>.</param>
    /// <returns>A populated <see cref="UserConfig"/> instance.</returns>
    /// <example>
    /// <code>
    /// UserConfig config = UserConfigLoader.Load();
    /// </code>
    /// </example>
    public static UserConfig Load(string envFilePath = ".env")
    {
        IDictionary<string, string>? envValues = null;

        if (File.Exists(envFilePath))
        {
            envValues = DotEnv.Read(new DotEnvOptions(
                envFilePaths: [envFilePath],
                ignoreExceptions: true));
        }

        return new UserConfig
        {
            WorldDatapacksPath = ResolvePath(WorldDatapacksEnvKey, envValues),
            WorldCompatDatapacksPath = ResolvePath(WorldCompatDatapacksEnvKey, envValues),
            ResourcePacksPath = ResolvePath(ResourcePacksEnvKey, envValues)
        };
    }

    /// <summary>
    /// Resolves an environment setting from process environment variables, falling back to parsed .env file values,
    /// and expands any embedded system variables (e.g., %APPDATA%).
    /// </summary>
    /// <param name="key">The environment variable key name.</param>
    /// <param name="fileEnv">The parsed key-value pairs from the .env file, if available.</param>
    /// <returns>A normalized full directory path, or <see langword="null"/> if not configured.</returns>
    private static string? ResolvePath(string key, IDictionary<string, string>? fileEnv)
    {
        // Process / OS environment variables take precedence
        var rawValue = Environment.GetEnvironmentVariable(key);

        // Fall back to .env file entries
        if (string.IsNullOrWhiteSpace(rawValue) && fileEnv != null && fileEnv.TryGetValue(key, out var fileValue))
        {
            rawValue = fileValue;
        }

        if (string.IsNullOrWhiteSpace(rawValue))
        {
            return null;
        }

        var expandedPath = Environment.ExpandEnvironmentVariables(rawValue.Trim());
        return Path.GetFullPath(expandedPath);
    }
}