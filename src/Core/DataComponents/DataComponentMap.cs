using System.Collections;
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
public class DataComponentMap : ISnbtSerializable, IEnumerable<IDataComponent>
{
    /// <summary>
    /// Gets the dictionary of currently active data components mapped by their normalized string identifier.
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
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="component"/> is null.</exception>
    [PublicAPI]
    public void Set(IDataComponent component)
    {
        ArgumentNullException.ThrowIfNull(component);

        var id = NormalizeId(component.Id);
        Removed.Remove(id);
        Added[id] = component;
    }

    /// <summary>
    /// Updates an existing component using a transform function, or initializes it if it does not exist.
    /// </summary>
    /// <typeparam name="T">The strongly-typed component class.</typeparam>
    /// <param name="updater">Function that accepts the existing component (or <see langword="null"/>) and returns the updated component.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="updater"/> is null.</exception>
    /// <example>
    /// <code>
    /// item.Components.Update&lt;CustomModelDataComponent&gt;(existing => (existing ?? new CustomModelDataComponent()) with
    /// {
    ///     Strings = [..existing?.Strings ?? [], "bacap:slug"]
    /// });
    /// </code>
    /// </example>
    [PublicAPI]
    public void Update<T>(Func<T?, T> updater) where T : class, ITypedComponent<T>
    {
        ArgumentNullException.ThrowIfNull(updater);

        var existing = Get<T>();
        var updated = updater(existing);
        Set(updated);
    }

    /// <summary>
    /// Removes a component from the map by its string identifier.
    /// </summary>
    /// <param name="componentId">The string identifier of the component to remove (e.g., "minecraft:damage" or "damage").</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="componentId"/> is null.</exception>
    [PublicAPI]
    public void Remove(string componentId)
    {
        ArgumentNullException.ThrowIfNull(componentId);

        var normalized = NormalizeId(componentId);
        Added.Remove(normalized);
        Removed.Add(normalized);
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
    /// <returns><see langword="true"/> if the component is active; otherwise, <see langword="false"/>.</returns>
    [PublicAPI]
    public bool Has(string componentId)
    {
        return !string.IsNullOrEmpty(componentId) && Added.ContainsKey(NormalizeId(componentId));
    }

    /// <summary>
    /// Determines whether a component with the specified string identifier exists in the map.
    /// Provides an alias for <see cref="Has(string)"/>.
    /// </summary>
    /// <param name="componentId">The string identifier of the component.</param>
    /// <returns><see langword="true"/> if the component is active; otherwise, <see langword="false"/>.</returns>
    [PublicAPI]
    public bool Contains(string componentId) => Has(componentId);

    /// <summary>
    /// Determines whether a strongly-typed component exists in the map.
    /// </summary>
    /// <typeparam name="T">The type of the component to check.</typeparam>
    /// <returns><see langword="true"/> if the component is active; otherwise, <see langword="false"/>.</returns>
    [PublicAPI]
    public bool Has<T>() where T : ITypedComponent<T> =>
        Added.ContainsKey(NormalizeId(T.ComponentId));

    /// <summary>
    /// Determines whether a strongly-typed component exists in the map.
    /// Provides an alias for <see cref="Has{T}()"/>.
    /// </summary>
    /// <typeparam name="T">The type of the component to check.</typeparam>
    /// <returns><see langword="true"/> if the component is active; otherwise, <see langword="false"/>.</returns>
    [PublicAPI]
    public bool Contains<T>() where T : ITypedComponent<T> => Has<T>();

    /// <summary>
    /// Retrieves a component by its string identifier.
    /// </summary>
    /// <param name="componentId">The string identifier of the component.</param>
    /// <returns>The <see cref="IDataComponent"/> if found; otherwise, <see langword="null"/>.</returns>
    [PublicAPI]
    public IDataComponent? Get(string componentId)
    {
        return string.IsNullOrEmpty(componentId)
            ? null
            : Added.GetValueOrDefault(NormalizeId(componentId));
    }

    /// <summary>
    /// Retrieves a strongly-typed component by its class.
    /// </summary>
    /// <typeparam name="T">The type of the component to retrieve.</typeparam>
    /// <returns>The strongly-typed component if found; otherwise, <see langword="null"/>.</returns>
    [PublicAPI]
    public T? Get<T>() where T : class, ITypedComponent<T> =>
        Added.TryGetValue(NormalizeId(T.ComponentId), out var comp) ? (T)comp : null;

    /// <summary>
    /// Attempts to retrieve a component by its string identifier safely.
    /// </summary>
    /// <param name="componentId">The string identifier of the component.</param>
    /// <param name="component">When this method returns, contains the component if found; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the component was found; otherwise, <see langword="false"/>.</returns>
    [PublicAPI]
    public bool TryGet(string componentId, [NotNullWhen(true)] out IDataComponent? component)
    {
        if (!string.IsNullOrEmpty(componentId))
            return Added.TryGetValue(NormalizeId(componentId), out component);

        component = null;
        return false;

    }

    /// <summary>
    /// Attempts to retrieve a strongly-typed component by its class safely.
    /// </summary>
    /// <typeparam name="T">The type of the component to retrieve.</typeparam>
    /// <param name="component">When this method returns, contains the casted component if found; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the component was found; otherwise, <see langword="false"/>.</returns>
    [PublicAPI]
    public bool TryGet<T>([NotNullWhen(true)] out T? component) where T : class, ITypedComponent<T>
    {
        if (Added.TryGetValue(NormalizeId(T.ComponentId), out var comp))
        {
            component = (T)comp;
            return true;
        }

        component = null;
        return false;
    }

    /// <summary>
    /// Returns an enumerator that iterates through the active data components in the map.
    /// </summary>
    /// <returns>An enumerator for active <see cref="IDataComponent"/> instances.</returns>
    /// <example>
    /// <code>
    /// foreach (var component in componentMap)
    /// {
    ///     Console.WriteLine(component.Id);
    /// }
    /// </code>
    /// </example>
    public IEnumerator<IDataComponent> GetEnumerator()
    {
        return Added.Values.GetEnumerator();
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    /// Populates this map from a given components SNBT compound using the <see cref="ComponentRegistry"/>.
    /// Handles both added components and '!' prefixed removal entries.
    /// </summary>
    /// <param name="compound">The SNBT compound node representing item components.</param>
    /// <returns>A new <see cref="DataComponentMap"/> populated with the parsed components.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="compound"/> is null.</exception>
    [PublicAPI]
    public static DataComponentMap Parse(SnbtCompound compound)
    {
        ArgumentNullException.ThrowIfNull(compound);

        var map = new DataComponentMap();
        foreach (var (compId, valNode) in compound.Tags)
        {
            if (compId.StartsWith('!'))
                map.Remove(compId[1..]);
            else
                map.Set(ComponentRegistry.Parse(compId, valNode));
        }

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

    public string ToSnbtString() => ToSnbt().ToSnbtString();

    private static string NormalizeId(string id) =>
        id.Contains(':') ? id : $"minecraft:{id}";
}