namespace UI.Configuration;

/// <summary>
/// Represents user-specific environment settings and target synchronization directories.
/// </summary>
public sealed record UserConfig
{
    /// <summary>
    /// Gets the destination directory path for worlds without compatibility addons.
    /// </summary>
    public string? WorldDatapacksPath { get; init; }

    /// <summary>
    /// Gets the destination directory path for worlds that include compatibility addons.
    /// </summary>
    public string? WorldCompatDatapacksPath { get; init; }

    /// <summary>
    /// Gets the destination directory path where resource packs will be synchronized.
    /// </summary>
    public string? ResourcePacksPath { get; init; }

    /// <summary>
    /// Gets a value indicating whether the base world datapack synchronization directory is configured and exists on disk.
    /// </summary>
    public bool IsWorldDatapacksPathValid =>
        !string.IsNullOrWhiteSpace(WorldDatapacksPath) &&
        Directory.Exists(WorldDatapacksPath);

    /// <summary>
    /// Gets a value indicating whether the compatibility world datapack synchronization directory is configured and exists on disk.
    /// </summary>
    public bool IsWorldCompatDatapacksPathValid =>
        !string.IsNullOrWhiteSpace(WorldCompatDatapacksPath) &&
        Directory.Exists(WorldCompatDatapacksPath);

    /// <summary>
    /// Gets a value indicating whether the resource pack target synchronization directory is configured and exists on disk.
    /// </summary>
    public bool IsResourcePacksPathValid =>
        !string.IsNullOrWhiteSpace(ResourcePacksPath) &&
        Directory.Exists(ResourcePacksPath);

    /// <summary>
    /// Gets a value indicating whether at least one target synchronization directory is configured and valid.
    /// </summary>
    public bool HasAnyValidPath =>
        IsWorldDatapacksPathValid ||
        IsWorldCompatDatapacksPathValid ||
        IsResourcePacksPathValid;
}