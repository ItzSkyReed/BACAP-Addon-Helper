namespace BacapGenerator.Models.Datapacks.Settings;

/// <summary>
/// Defines the file system access permissions for a loaded datapack or function.
/// </summary>
public enum DatapackAccess
{
    /// <summary>
    /// The datapack can only be parsed. Generating or modifying files on disk is prohibited.
    /// </summary>
    ReadOnly,

    /// <summary>
    /// The datapack can be parsed, modified, and generated on the disk.
    /// </summary>
    ReadWrite
}