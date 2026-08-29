using Core.SNBT.Interfaces;
using Core.SNBT.Nodes;
using JetBrains.Annotations;

namespace Core.SNBT;

/// <summary>
/// A fluent builder for constructing strongly-typed <see cref="SnbtCompound"/> nodes.
/// </summary>
[PublicAPI]
public class SnbtCompoundBuilder
{
    private readonly Dictionary<string, ISnbtNode> _dict = new();

    /// <summary>
    /// Sets a node directly by key.
    /// </summary>
    /// <param name="key">The tag name.</param>
    /// <param name="node">The SNBT node value.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public SnbtCompoundBuilder Put(string key, ISnbtNode node)
    {
        _dict[key] = node;
        return this;
    }

    #region Primitive Put Overloads

    public SnbtCompoundBuilder Put(string key, sbyte value) => Put(key, new SnbtByte(value));
    public SnbtCompoundBuilder Put(string key, short value) => Put(key, new SnbtShort(value));
    public SnbtCompoundBuilder Put(string key, int value) => Put(key, new SnbtInt(value));
    public SnbtCompoundBuilder Put(string key, long value) => Put(key, new SnbtLong(value));
    public SnbtCompoundBuilder Put(string key, float value) => Put(key, new SnbtFloat(value));
    public SnbtCompoundBuilder Put(string key, double value) => Put(key, new SnbtDouble(value));
    public SnbtCompoundBuilder Put(string key, string value) => Put(key, new SnbtString(value));
    public SnbtCompoundBuilder Put(string key, bool value) => Put(key, new SnbtBool(value));

    #endregion

    #region Complex & Nested Builders

    /// <summary>
    /// Adds a nested <see cref="SnbtCompound"/> configured via a nested builder action.
    /// </summary>
    /// <param name="key">The tag name.</param>
    /// <param name="buildAction">Action to configure the nested compound builder.</param>
    /// <returns>The builder instance for method chaining.</returns>
    /// <example>
    /// <code>
    /// builder.PutCompound("display", b => b.Put("Name", "Hero Sword"));
    /// </code>
    /// </example>
    public SnbtCompoundBuilder PutCompound(string key, Action<SnbtCompoundBuilder> buildAction)
    {
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
    public SnbtCompoundBuilder PutList(string key, Action<SnbtListBuilder> buildAction)
    {
        var listBuilder = new SnbtListBuilder();
        buildAction(listBuilder);
        return Put(key, listBuilder.Build());
    }

    #endregion

    #region Array Overloads

    public SnbtCompoundBuilder PutByteArray(string key, params sbyte[] values) =>
        Put(key, new SnbtByteArray(values.Select(ISnbtNode (v) => new SnbtByte(v)).ToList()));

    public SnbtCompoundBuilder PutByteArray(string key, params byte[] values) =>
        Put(key, new SnbtByteArray(values.Select(ISnbtNode (v) => new SnbtByte((sbyte)v)).ToList()));

    public SnbtCompoundBuilder PutIntArray(string key, params int[] values) =>
        Put(key, new SnbtIntArray(values.Select(ISnbtNode (v) => new SnbtInt(v)).ToList()));

    public SnbtCompoundBuilder PutLongArray(string key, params long[] values) =>
        Put(key, new SnbtLongArray(values.Select(ISnbtNode (v) => new SnbtLong(v)).ToList()));

    #endregion

    #region Optional Overloads (Default Value Check)

    // ReSharper disable CompareOfFloatsByEqualityOperator
    /// <summary>
    /// Adds a float value only if it differs from the specified default value.
    /// </summary>
    public SnbtCompoundBuilder PutOptional(string key, float value, float defaultValue)
    {
        if (value != defaultValue) Put(key, value);
        return this;
    }

    /// <summary>
    /// Adds a double value only if it differs from the specified default value.
    /// </summary>
    public SnbtCompoundBuilder PutOptional(string key, double value, double defaultValue)
    {
        if (value != defaultValue) Put(key, value);
        return this;
    }
    // ReSharper restore CompareOfFloatsByEqualityOperator

    /// <summary>
    /// Adds an integer value only if it differs from the specified default value.
    /// </summary>
    public SnbtCompoundBuilder PutOptional(string key, int value, int defaultValue)
    {
        if (value != defaultValue) Put(key, value);
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
        if (value != null) Put(key, value);
        return this;
    }

    /// <summary>
    /// Adds a node only if it is not null.
    /// </summary>
    public SnbtCompoundBuilder PutOptional(string key, ISnbtNode? value)
    {
        if (value != null) Put(key, value);
        return this;
    }

    public SnbtCompoundBuilder PutOptional(string key, bool? value)
    {
        if (value.HasValue) Put(key, value.Value);
        return this;
    }

    public SnbtCompoundBuilder PutOptional(string key, float? value)
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

    public SnbtCompoundBuilder PutOptional(string key, double? value)
    {
        if (value.HasValue) Put(key, value.Value);
        return this;
    }

    #endregion

    /// <summary>
    /// Builds and returns the immutable <see cref="SnbtCompound"/> instance.
    /// </summary>
    /// <returns>A new <see cref="SnbtCompound"/> containing all configured key-value pairs.</returns>
    public SnbtCompound Build() => new(new Dictionary<string, ISnbtNode>(_dict));
}