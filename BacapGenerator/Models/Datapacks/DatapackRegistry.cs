using System.Diagnostics.CodeAnalysis;

namespace BacapGenerator.Models.Datapacks;

/// <summary>
/// A central registry for accessing all loaded datapacks by their unique string identifiers.
/// </summary>
public class DatapackRegistry
{
    private readonly Dictionary<string, Datapack> _datapacks = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets a read-only view of all registered datapacks.
    /// </summary>
    public IReadOnlyDictionary<string, Datapack> All => _datapacks;

    /// <summary>
    /// Registers a newly loaded datapack into the registry.
    /// </summary>
    /// <param name="id">The unique identifier of the datapack (e.g., "bacap").</param>
    /// <param name="datapack">The fully initialized datapack instance.</param>
    /// <exception cref="ArgumentException">Thrown if a datapack with the same ID is already registered.</exception>
    public void Register(string id, Datapack datapack)
    {
        ArgumentException.ThrowIfNullOrEmpty(id);
        ArgumentNullException.ThrowIfNull(datapack);

        if (!_datapacks.TryAdd(id, datapack))
            throw new ArgumentException($"A datapack with the ID '{id}' is already registered.");
    }

    /// <summary>
    /// Retrieves a datapack by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier to search for.</param>
    /// <returns>The registered datapack.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if no datapack with the specified ID exists.</exception>
    public Datapack Get(string id) => _datapacks[id];

    /// <summary>
    /// Attempts to retrieve a datapack by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier to search for.</param>
    /// <param name="datapack">When this method returns, contains the datapack if found; otherwise, null.</param>
    /// <returns>True if the datapack was found; otherwise, false.</returns>
    public bool TryGet(string id, [NotNullWhen(true)] out Datapack? datapack)
    {
        return _datapacks.TryGetValue(id, out datapack);
    }


}