using Core.SNBT.Interfaces;
using Core.SNBT.Nodes;
using JetBrains.Annotations;

namespace Core.SNBT;

/// <summary>
/// A fluent builder for constructing strongly-typed, immutable <see cref="SnbtCompound"/> instances.
/// </summary>
[PublicAPI]
public sealed class SnbtCompoundBuilder
{
    private readonly Dictionary<string, ISnbtNode> _dict = new();

    /// <summary>
    /// Sets an <see cref="ISnbtNode"/> by key, overwriting any existing entry.
    /// </summary>
    /// <param name="key">The tag name.</param>
    /// <param name="node">The SNBT node value.</param>
    /// <returns>The builder instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="key"/> or <paramref name="node"/> is null.</exception>
    public SnbtCompoundBuilder Put(string key, ISnbtNode node)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(node);

        _dict[key] = node;
        return this;
    }

    #region Primitive Put Overloads

    /// <summary>
    /// Adds a signed 8-bit byte tag (<see cref="SnbtByte"/>) by key.
    /// </summary>
    public SnbtCompoundBuilder Put(string key, sbyte value) => Put(key, new SnbtByte(value));

    /// <summary>
    /// Adds a signed 16-bit short tag (<see cref="SnbtShort"/>) by key.
    /// </summary>
    public SnbtCompoundBuilder Put(string key, short value) => Put(key, new SnbtShort(value));

    /// <summary>
    /// Adds a signed 32-bit integer tag (<see cref="SnbtInt"/>) by key.
    /// </summary>
    public SnbtCompoundBuilder Put(string key, int value) => Put(key, new SnbtInt(value));

    /// <summary>
    /// Adds a signed 64-bit long tag (<see cref="SnbtLong"/>) by key.
    /// </summary>
    public SnbtCompoundBuilder Put(string key, long value) => Put(key, new SnbtLong(value));

    /// <summary>
    /// Adds a 32-bit single-precision float tag (<see cref="SnbtFloat"/>) by key.
    /// </summary>
    public SnbtCompoundBuilder Put(string key, float value) => Put(key, new SnbtFloat(value));

    /// <summary>
    /// Adds a 64-bit double-precision float tag (<see cref="SnbtDouble"/>) by key.
    /// </summary>
    public SnbtCompoundBuilder Put(string key, double value) => Put(key, new SnbtDouble(value));

    /// <summary>
    /// Adds a string tag (<see cref="SnbtString"/>) by key.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public SnbtCompoundBuilder Put(string key, string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return Put(key, new SnbtString(value));
    }

    /// <summary>
    /// Adds a boolean tag (<see cref="SnbtBool"/>) by key.
    /// </summary>
    public SnbtCompoundBuilder Put(string key, bool value) => Put(key, new SnbtBool(value));

    #endregion

    #region Complex & Nested Builders

    /// <summary>
    /// Adds a nested <see cref="SnbtCompound"/> configured via a nested builder action.
    /// </summary>
    /// <param name="key">The tag name.</param>
    /// <param name="buildAction">Action to configure the nested compound builder.</param>
    /// <returns>The builder instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="buildAction"/> is null.</exception>
    /// <example>
    /// <code>
    /// builder.PutCompound("display", b => b.Put("Name", "Hero Sword"));
    /// </code>
    /// </example>
    public SnbtCompoundBuilder PutCompound(string key, Action<SnbtCompoundBuilder> buildAction)
    {
        ArgumentNullException.ThrowIfNull(buildAction);

        var nestedBuilder = new SnbtCompoundBuilder();
        buildAction(nestedBuilder);
        return Put(key, nestedBuilder.Build());
    }

    /// <summary>
    /// Adds an <see cref="SnbtList"/> configured via a list builder action.
    /// </summary>
    /// <param name="key">The tag name.</param>
    /// <param name="buildAction">Action to configure the list builder.</param>
    /// <returns>The builder instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="buildAction"/> is null.</exception>
    public SnbtCompoundBuilder PutList(string key, Action<SnbtListBuilder> buildAction)
    {
        ArgumentNullException.ThrowIfNull(buildAction);

        var listBuilder = new SnbtListBuilder();
        buildAction(listBuilder);
        return Put(key, listBuilder.Build());
    }

    #endregion

    #region Array Overloads

    /// <summary>
    /// Adds a signed byte array tag ([B;...]) by key.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is null.</exception>
    public SnbtCompoundBuilder PutByteArray(string key, params sbyte[] values) =>
        PutArrayCore(key, values, static v => new SnbtByte(v), static nodes => new SnbtByteArray(nodes));

    /// <summary>
    /// Adds a byte array tag ([B;...]) by key, casting unsigned bytes to signed 8-bit bytes.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is null.</exception>
    public SnbtCompoundBuilder PutByteArray(string key, params byte[] values) =>
        PutArrayCore(key, values, static v => new SnbtByte((sbyte)v), static nodes => new SnbtByteArray(nodes));

    /// <summary>
    /// Adds a 32-bit integer array tag ([I;...]) by key.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is null.</exception>
    public SnbtCompoundBuilder PutIntArray(string key, params int[] values) =>
        PutArrayCore(key, values, static v => new SnbtInt(v), static nodes => new SnbtIntArray(nodes));

    /// <summary>
    /// Adds a 64-bit integer array tag ([L;...]) by key.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is null.</exception>
    public SnbtCompoundBuilder PutLongArray(string key, params long[] values) =>
        PutArrayCore(key, values, static v => new SnbtLong(v), static nodes => new SnbtLongArray(nodes));

    private SnbtCompoundBuilder PutArrayCore<TSource, TArrayNode>(
        string key,
        TSource[] values,
        Func<TSource, ISnbtNode> nodeFactory,
        Func<List<ISnbtNode>, TArrayNode> arrayNodeFactory)
        where TArrayNode : ISnbtNode
    {
        ArgumentNullException.ThrowIfNull(values);

        var list = new List<ISnbtNode>(values.Length);
        foreach (var value in values)
            list.Add(nodeFactory(value));

        return Put(key, arrayNodeFactory(list));
    }

    #endregion

    #region Optional Overloads (Default Value Check)

    /// <summary>
    /// Adds a byte value only if it differs from the specified default value.
    /// </summary>
    public SnbtCompoundBuilder PutOptional(string key, sbyte value, sbyte defaultValue)
    {
        if (value != defaultValue) Put(key, value);
        return this;
    }

    /// <summary>
    /// Adds a short value only if it differs from the specified default value.
    /// </summary>
    public SnbtCompoundBuilder PutOptional(string key, short value, short defaultValue)
    {
        if (value != defaultValue) Put(key, value);
        return this;
    }

    /// <summary>
    /// Adds an integer value only if it differs from the specified default value.
    /// </summary>
    public SnbtCompoundBuilder PutOptional(string key, int value, int defaultValue)
    {
        if (value != defaultValue) Put(key, value);
        return this;
    }

    /// <summary>
    /// Adds a long value only if it differs from the specified default value.
    /// </summary>
    public SnbtCompoundBuilder PutOptional(string key, long value, long defaultValue)
    {
        if (value != defaultValue) Put(key, value);
        return this;
    }

    /// <summary>
    /// Adds a float value only if it differs from the specified default value.
    /// </summary>
    public SnbtCompoundBuilder PutOptional(string key, float value, float defaultValue)
    {
        if (!value.Equals(defaultValue)) Put(key, value);
        return this;
    }

    /// <summary>
    /// Adds a double value only if it differs from the specified default value.
    /// </summary>
    public SnbtCompoundBuilder PutOptional(string key, double value, double defaultValue)
    {
        if (!value.Equals(defaultValue)) Put(key, value);
        return this;
    }

    /// <summary>
    /// Adds a boolean value only if it differs from the specified default value.
    /// </summary>
    public SnbtCompoundBuilder PutOptional(string key, bool value, bool defaultValue)
    {
        if (value != defaultValue) Put(key, value);
        return this;
    }

    /// <summary>
    /// Adds a string value only if it differs from the specified default value.
    /// </summary>
    public SnbtCompoundBuilder PutOptional(string key, string value, string defaultValue)
    {
        if (value != defaultValue) Put(key, value);
        return this;
    }

    #endregion

    #region Optional Overloads (Nullable Check)

    /// <summary>
    /// Adds a string value only if it is not null.
    /// </summary>
    public SnbtCompoundBuilder PutOptional(string key, string? value)
    {
        if (value is not null) Put(key, value);
        return this;
    }

    /// <summary>
    /// Adds a node only if it is not null.
    /// </summary>
    public SnbtCompoundBuilder PutOptional(string key, ISnbtNode? value)
    {
        if (value is not null) Put(key, value);
        return this;
    }

    public SnbtCompoundBuilder PutOptional(string key, sbyte? value)
    {
        if (value.HasValue) Put(key, value.Value);
        return this;
    }

    public SnbtCompoundBuilder PutOptional(string key, short? value)
    {
        if (value.HasValue) Put(key, value.Value);
        return this;
    }

    public SnbtCompoundBuilder PutOptional(string key, int? value)
    {
        if (value.HasValue) Put(key, value.Value);
        return this;
    }

    public SnbtCompoundBuilder PutOptional(string key, long? value)
    {
        if (value.HasValue) Put(key, value.Value);
        return this;
    }

    public SnbtCompoundBuilder PutOptional(string key, float? value)
    {
        if (value.HasValue) Put(key, value.Value);
        return this;
    }

    public SnbtCompoundBuilder PutOptional(string key, double? value)
    {
        if (value.HasValue) Put(key, value.Value);
        return this;
    }

    public SnbtCompoundBuilder PutOptional(string key, bool? value)
    {
        if (value.HasValue) Put(key, value.Value);
        return this;
    }

    #endregion

    /// <summary>
    /// Builds and returns an immutable <see cref="SnbtCompound"/> instance containing a snapshot of all configured pairs.
    /// </summary>
    /// <returns>A new <see cref="SnbtCompound"/> containing all configured key-value pairs.</returns>
    public SnbtCompound Build() => new(new Dictionary<string, ISnbtNode>(_dict));
}