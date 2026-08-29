namespace Core.DataComponents.Interfaces;

/// <summary>
/// Base interface for strongly-typed data components with a static component identifier.
/// </summary>
/// <typeparam name="TSelf">The concrete component type.</typeparam>
public interface ITypedComponent<TSelf> : IDataComponent
    where TSelf : ITypedComponent<TSelf>
{
    /// <summary>
    /// Gets the static resource location identifier of the component.
    /// </summary>
    static abstract string ComponentId { get; }

    string IDataComponent.Id => TSelf.ComponentId;
}