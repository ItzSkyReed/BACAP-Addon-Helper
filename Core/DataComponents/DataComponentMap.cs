using System.Diagnostics.CodeAnalysis;
using Core.DataComponents.Interfaces;
using Core.SNBT;
using Core.SNBT.Interfaces;
using Core.SNBT.Nodes;
using JetBrains.Annotations;

namespace Core.DataComponents;

/// <summary>
/// Represents a map of Minecraft item data components, handling both added components
/// and explicit removals (e.g., starting with '!').
/// </summary>
public class DataComponentMap : ISnbtSerializable
{
    /// <summary>
    /// Gets the dictionary of currently active data components mapped by their string identifier.
    /// </summary>
    [PublicAPI]
    public Dictionary<string, IDataComponent> Added { get; } = new();

    /// <summary>
    /// Gets the set of component identifiers that have been explicitly removed.
    /// </summary>
    [PublicAPI]
    public HashSet<string> Removed { get; } = [];

    /// <summary>
    /// Gets a value indicating whether this map contains no added or removed components.
    /// </summary>
    [PublicAPI]
    public bool IsEmpty => Added.Count == 0 && Removed.Count == 0;

    /// <summary>
    /// Adds or overwrites a data component in the map.
    /// </summary>
    /// <param name="component">The data component implementation to add.</param>
    [PublicAPI]
    public void Set(IDataComponent component)
    {
        var id = component.Id;
        Removed.Remove(id);
        Added[id] = component;
    }

    /// <summary>
    /// Removes a component from the map by its string identifier.
    /// </summary>
    /// <param name="componentId">The string identifier of the component to remove (e.g., "minecraft:damage").</param>
    [PublicAPI]
    public void Remove(string componentId)
    {
        Added.Remove(componentId);
        Removed.Add(componentId);
    }

    /// <summary>
    /// Removes a component from the map by its strongly-typed component class.
    /// </summary>
    /// <typeparam name="T">The type of the component to remove.</typeparam>
    [PublicAPI]
    public void Remove<T>() where T : ITypedComponent<T>
    {
        Remove(T.ComponentId);
    }

    /// <summary>
    /// Determines whether a component with the specified string identifier exists in the map.
    /// </summary>
    /// <param name="componentId">The string identifier of the component.</param>
    /// <returns><c>true</c> if the component is active; otherwise, <c>false</c>.</returns>
    [PublicAPI]
    public bool Has(string componentId)
    {
        return Added.ContainsKey(componentId);
    }

    /// <summary>
    /// Determines whether a strongly-typed component exists in the map.
    /// </summary>
    /// <typeparam name="T">The type of the component to check.</typeparam>
    /// <returns><c>true</c> if the component is active; otherwise, <c>false</c>.</returns>
    [PublicAPI]
    public bool Has<T>() where T : ITypedComponent<T>
    {
        return Added.ContainsKey(T.ComponentId);
    }

    /// <summary>
    /// Retrieves a component by its string identifier.
    /// </summary>
    /// <param name="componentId">The string identifier of the component.</param>
    /// <returns>The <see cref="IDataComponent"/> if found; otherwise, <c>null</c>.</returns>
    [PublicAPI]
    public IDataComponent? Get(string componentId)
    {
        return Added.GetValueOrDefault(componentId);
    }

    /// <summary>
    /// Retrieves a strongly-typed component by its class.
    /// </summary>
    /// <typeparam name="T">The type of the component to retrieve.</typeparam>
    /// <returns>The strongly-typed component if found; otherwise, <c>null</c>.</returns>
    [PublicAPI]
    public T? Get<T>() where T : class, ITypedComponent<T>
    {
        return Added.TryGetValue(T.ComponentId, out var comp) ? (T)comp : null;
    }

    /// <summary>
    /// Attempts to retrieve a component by its string identifier safely.
    /// </summary>
    /// <param name="componentId">The string identifier of the component.</param>
    /// <param name="component">When this method returns, contains the component if found; otherwise, <c>null</c>.</param>
    /// <returns><c>true</c> if the component was found; otherwise, <c>false</c>.</returns>
    [PublicAPI]
    public bool TryGet(string componentId, [NotNullWhen(true)] out IDataComponent? component)
    {
        return Added.TryGetValue(componentId, out component);
    }

    /// <summary>
    /// Attempts to retrieve a strongly-typed component by its class safely.
    /// </summary>
    /// <typeparam name="T">The type of the component to retrieve.</typeparam>
    /// <param name="component">When this method returns, contains the casted component if found; otherwise, <c>null</c>.</param>
    /// <returns><c>true</c> if the component was found; otherwise, <c>false</c>.</returns>
    [PublicAPI]
    public bool TryGet<T>([NotNullWhen(true)] out T? component) where T : class, ITypedComponent<T>
    {
        if (Added.TryGetValue(T.ComponentId, out var comp))
        {
            component = (T)comp;
            return true;
        }

        component = null;
        return false;
    }

    /// <summary>
    /// Populates this map from a given components SNBT compound using the <see cref="ComponentRegistry"/>.
    /// </summary>
    /// <param name="compound">The SNBT compound node representing item components.</param>
    /// <returns>A new <see cref="DataComponentMap"/> populated with the parsed components.</returns>
    [PublicAPI]
    public static DataComponentMap Parse(SnbtCompound compound)
    {
        var map = new DataComponentMap();
        foreach (var (compId, valNode) in compound.Tags)
            map.Set(ComponentRegistry.Parse(compId, valNode));
        return map;
    }

    /// <summary>
    /// Serializes active and explicitly removed components into an SNBT compound node.
    /// </summary>
    /// <returns>An <see cref="ISnbtNode"/> representing the serialized components.</returns>
    public ISnbtNode ToSnbt()
    {
        var builder = Snbt.Compound();

        // Serialize active components
        foreach (var (_, component) in Added)
            builder.Put(component.Id, component.ToSnbt());

        // Serialize explicitly removed components
        // Minecraft represents removed components with a '!' prefix and an empty compound '{}'
        foreach (var removedId in Removed)
            builder.Put($"!{removedId}", Snbt.Compound().Build());

        return builder.Build();
    }

    /// <summary>
    /// Serializes active and explicitly removed components into an SNBT string.
    /// </summary>
    /// <returns>An SNBT string representing the serialized components.</returns>
    public string ToSnbtString()
    {
        // Reuse the logic from ToSnbt() to prevent code duplication
        return ToSnbt().ToSnbtString();
    }
}