using Core.SNBT.Interfaces;

namespace Core.DataComponents.Interfaces;

/// <summary>
/// Non-generic base interface for all Minecraft item data components.
/// </summary>
public interface IDataComponent : ISnbtSerializable
{
    /// <summary>
    /// Gets the resource location identifier of the component (e.g. <c>minecraft:custom_name</c>).
    /// </summary>
    string Id { get; }
}