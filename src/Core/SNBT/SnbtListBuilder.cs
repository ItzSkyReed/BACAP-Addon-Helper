using Core.SNBT.Interfaces;
using Core.SNBT.Nodes;
using JetBrains.Annotations;

namespace Core.SNBT;

/// <summary>
/// Provides a fluent builder for constructing immutable <see cref="SnbtList"/> instances.
/// </summary>
public sealed class SnbtListBuilder
{
    private readonly List<ISnbtNode> _list = [];

    /// <summary>
    /// Appends an existing <see cref="ISnbtNode"/> to the list.
    /// </summary>
    /// <param name="node">The SNBT node to add.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="node"/> is null.</exception>
    [PublicAPI]
    public SnbtListBuilder Add(ISnbtNode node)
    {
        ArgumentNullException.ThrowIfNull(node);
        _list.Add(node);
        return this;
    }

    /// <summary>
    /// Appends a boolean tag (<see cref="SnbtBool"/>) to the list.
    /// </summary>
    /// <param name="value">The boolean value.</param>
    /// <returns>The current builder instance.</returns>
    [PublicAPI]
    public SnbtListBuilder Add(bool value) => Add(new SnbtBool(value));

    /// <summary>
    /// Appends an 8-bit signed byte tag (<see cref="SnbtByte"/>) to the list.
    /// </summary>
    /// <param name="value">The byte value.</param>
    /// <returns>The current builder instance.</returns>
    [PublicAPI]
    public SnbtListBuilder Add(sbyte value) => Add(new SnbtByte(value));

    /// <summary>
    /// Appends a 16-bit signed short tag (<see cref="SnbtShort"/>) to the list.
    /// </summary>
    /// <param name="value">The short integer value.</param>
    /// <returns>The current builder instance.</returns>
    [PublicAPI]
    public SnbtListBuilder Add(short value) => Add(new SnbtShort(value));

    /// <summary>
    /// Appends a 32-bit signed integer tag (<see cref="SnbtInt"/>) to the list.
    /// </summary>
    /// <param name="value">The integer value.</param>
    /// <returns>The current builder instance.</returns>
    [PublicAPI]
    public SnbtListBuilder Add(int value) => Add(new SnbtInt(value));

    /// <summary>
    /// Appends a 64-bit signed long tag (<see cref="SnbtLong"/>) to the list.
    /// </summary>
    /// <param name="value">The 64-bit integer value.</param>
    /// <returns>The current builder instance.</returns>
    [PublicAPI]
    public SnbtListBuilder Add(long value) => Add(new SnbtLong(value));

    /// <summary>
    /// Appends a 32-bit single-precision floating-point tag (<see cref="SnbtFloat"/>) to the list.
    /// </summary>
    /// <param name="value">The float value.</param>
    /// <returns>The current builder instance.</returns>
    [PublicAPI]
    public SnbtListBuilder Add(float value) => Add(new SnbtFloat(value));

    /// <summary>
    /// Appends a 64-bit double-precision floating-point tag (<see cref="SnbtDouble"/>) to the list.
    /// </summary>
    /// <param name="value">The double value.</param>
    /// <returns>The current builder instance.</returns>
    [PublicAPI]
    public SnbtListBuilder Add(double value) => Add(new SnbtDouble(value));

    /// <summary>
    /// Appends a string tag (<see cref="SnbtString"/>) to the list.
    /// </summary>
    /// <param name="value">The string value.</param>
    /// <returns>The current builder instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    [PublicAPI]
    public SnbtListBuilder Add(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return Add(new SnbtString(value));
    }

    /// <summary>
    /// Appends a collection of arbitrary SNBT nodes.
    /// </summary>
    /// <param name="nodes">The nodes to append.</param>
    /// <returns>The current builder instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="nodes"/> is null.</exception>
    [PublicAPI]
    public SnbtListBuilder AddRange(IEnumerable<ISnbtNode> nodes)
    {
        ArgumentNullException.ThrowIfNull(nodes);
        var snbtNodes = nodes as ISnbtNode[] ?? nodes.ToArray();
        EnsureCapacityFor(snbtNodes);

        foreach (var node in snbtNodes)
            Add(node);

        return this;
    }

    /// <summary>
    /// Appends a collection of strings as <see cref="SnbtString"/> tags.
    /// </summary>
    /// <param name="values">The string values to append.</param>
    /// <returns>The current builder instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is null.</exception>
    [PublicAPI]
    public SnbtListBuilder AddRange(IEnumerable<string> values)
    {
        ArgumentNullException.ThrowIfNull(values);
        var enumerable = values as string[] ?? values.ToArray();
        EnsureCapacityFor(enumerable);

        foreach (var v in enumerable)
            Add(v);

        return this;
    }

    /// <summary>
    /// Appends a collection of integers as <see cref="SnbtInt"/> tags.
    /// </summary>
    /// <param name="values">The integer values to append.</param>
    /// <returns>The current builder instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is null.</exception>
    [PublicAPI]
    public SnbtListBuilder AddRange(IEnumerable<int> values)
    {
        ArgumentNullException.ThrowIfNull(values);
        var enumerable = values as int[] ?? values.ToArray();
        EnsureCapacityFor(enumerable);

        foreach (var v in enumerable)
            Add(v);

        return this;
    }

    /// <summary>
    /// Appends a collection of doubles as <see cref="SnbtDouble"/> tags.
    /// </summary>
    /// <param name="values">The double values to append.</param>
    /// <returns>The current builder instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is null.</exception>
    [PublicAPI]
    public SnbtListBuilder AddRange(IEnumerable<double> values)
    {
        ArgumentNullException.ThrowIfNull(values);
        var enumerable = values as double[] ?? values.ToArray();
        EnsureCapacityFor(enumerable);

        foreach (var v in enumerable)
            Add(v);

        return this;
    }

    /// <summary>
    /// Appends a nested <see cref="SnbtCompound"/> constructed via a configured builder.
    /// </summary>
    /// <param name="buildAction">The configuration delegate for the nested compound builder.</param>
    /// <returns>The current builder instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="buildAction"/> is null.</exception>
    [PublicAPI]
    public SnbtListBuilder AddCompound(Action<SnbtCompoundBuilder> buildAction)
    {
        ArgumentNullException.ThrowIfNull(buildAction);

        var nestedBuilder = new SnbtCompoundBuilder();
        buildAction(nestedBuilder);
        return Add(nestedBuilder.Build());
    }

    /// <summary>
    /// Appends a nested <see cref="SnbtList"/> constructed via a configured builder.
    /// </summary>
    /// <param name="buildAction">The configuration delegate for the nested list builder.</param>
    /// <returns>The current builder instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="buildAction"/> is null.</exception>
    [PublicAPI]
    public SnbtListBuilder AddList(Action<SnbtListBuilder> buildAction)
    {
        ArgumentNullException.ThrowIfNull(buildAction);

        var nestedBuilder = new SnbtListBuilder();
        buildAction(nestedBuilder);
        return Add(nestedBuilder.Build());
    }

    /// <summary>
    /// Builds an immutable <see cref="SnbtList"/> containing a snapshot of all added nodes.
    /// </summary>
    /// <returns>A new <see cref="SnbtList"/> instance.</returns>
    [PublicAPI]
    public SnbtList Build() => new([.. _list]);

    private void EnsureCapacityFor<T>(IEnumerable<T> source)
    {
        switch (source)
        {
            case ICollection<T> collection:
                _list.EnsureCapacity(_list.Count + collection.Count);
                break;
            case IReadOnlyCollection<T> readOnlyCollection:
                _list.EnsureCapacity(_list.Count + readOnlyCollection.Count);
                break;
        }
    }
}