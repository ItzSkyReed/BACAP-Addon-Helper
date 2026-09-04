using System.Collections;
using System.Diagnostics.CodeAnalysis;
using BacapGenerator.Models.Datapacks.Settings;

namespace BacapGenerator.Models.Datapacks;

/// <summary>
/// Central registry providing compile-time verified access to loaded datapack instances.
/// </summary>
public class DatapackRegistry : IReadOnlyDictionary<DatapackId, Datapack>
{
    private readonly Dictionary<DatapackId, Datapack> _datapacks = new(3);

    /// <summary>
    /// Gets the base BACAP datapack instance.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the datapack has not been loaded.</exception>
    public Datapack Bacap => Get(DatapackId.Bacap);

    /// <summary>
    /// Gets the BACAP Enhanced addon datapack instance.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the datapack has not been loaded.</exception>
    public Datapack Bacaped => Get(DatapackId.Bacaped);

    /// <summary>
    /// Gets the Hardcore BACAP Enhanced addon datapack instance.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the datapack has not been loaded.</exception>
    public Datapack BacapedHardcore => Get(DatapackId.BacapedHardcore);

    /// <summary>
    /// Registers a newly loaded datapack into the registry.
    /// </summary>
    /// <param name="datapack">The fully initialized datapack instance.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="datapack"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when a datapack with the same identifier is already registered.</exception>
    public void Register(Datapack datapack)
    {
        ArgumentNullException.ThrowIfNull(datapack);

        if (!_datapacks.TryAdd(datapack.Id, datapack))
            throw new ArgumentException($"A datapack with the identifier '{datapack.Id}' is already registered.", nameof(datapack));
    }

    /// <summary>
    /// Retrieves a datapack by its strongly-typed identifier.
    /// </summary>
    /// <param name="id">The datapack identifier to retrieve.</param>
    /// <returns>The registered datapack instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the requested datapack is not loaded.</exception>
    public Datapack Get(DatapackId id)
    {
        return _datapacks.TryGetValue(id, out var datapack)
            ? datapack
            : throw new InvalidOperationException($"Datapack '{id}' has not been loaded into the registry.");
    }

    /// <summary>
    /// Attempts to retrieve a datapack by its strongly-typed identifier.
    /// </summary>
    /// <param name="id">The datapack identifier to search for.</param>
    /// <param name="datapack">When this method returns, contains the datapack if found; otherwise, null.</param>
    /// <returns><see langword="true"/> if found; otherwise, <see langword="false"/>.</returns>
    public bool TryGet(DatapackId id, [NotNullWhen(true)] out Datapack? datapack)
    {
        return _datapacks.TryGetValue(id, out datapack);
    }

    /// <inheritdoc/>
    public Datapack this[DatapackId key] => Get(key);

    /// <inheritdoc/>
    public IEnumerable<DatapackId> Keys => _datapacks.Keys;

    /// <inheritdoc/>
    public IEnumerable<Datapack> Values => _datapacks.Values;

    /// <inheritdoc/>
    public int Count => _datapacks.Count;

    /// <inheritdoc/>
    public bool ContainsKey(DatapackId key) => _datapacks.ContainsKey(key);

    /// <inheritdoc/>
    public bool TryGetValue(DatapackId key, [NotNullWhen(true)] out Datapack? value) => _datapacks.TryGetValue(key, out value);

    /// <inheritdoc/>
    public IEnumerator<KeyValuePair<DatapackId, Datapack>> GetEnumerator() => _datapacks.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}