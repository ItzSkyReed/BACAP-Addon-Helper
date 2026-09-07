using System.ComponentModel;
using BacapGenerator.Converters;

namespace BacapGenerator.Models.Datapacks.Settings;

/// <summary>
/// Defines the file system access permissions for a loaded datapack or function.
/// </summary>
[TypeConverter(typeof(DatapackTypeConverter))]
public enum DatapackType
{
    /// <summary>
    /// The datapack can only be parsed. Generating or modifying files on disk is prohibited.
    /// </summary>
    Reference,

    /// <summary>
    /// The addon that can be parsed, modified, and generated on the disk.
    /// </summary>
    Addon,

    /// <summary>
    /// The compatibility addon to main addon datapack, usually "Hardcore, Terralith" versions.
    /// </summary>
    CompatibilityAddon
}