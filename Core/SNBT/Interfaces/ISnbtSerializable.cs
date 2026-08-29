namespace Core.SNBT.Interfaces;

/// <summary>
/// Defines an object that can be serialized into an SNBT node representation.
/// </summary>
public interface ISnbtSerializable
{
    /// <summary>
    /// Serializes the instance into an SNBT node.
    /// </summary>
    ISnbtNode ToSnbt();
}