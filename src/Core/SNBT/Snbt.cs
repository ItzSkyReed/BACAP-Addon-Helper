using Core.SNBT.Nodes;
using JetBrains.Annotations;

namespace Core.SNBT;

/// <summary>
/// Provides factory methods and entry points for building SNBT nodes and builders.
/// </summary>
[PublicAPI]
public static class Snbt
{
    /// <summary>
    /// Creates a new fluent builder for constructing an <see cref="SnbtCompound"/>.
    /// </summary>
    /// <returns>A new <see cref="SnbtCompoundBuilder"/> instance.</returns>
    /// <example>
    /// <code>
    /// var compound = Snbt.Compound()
    ///     .Put("id", "minecraft:stone")
    ///     .Build();
    /// </code>
    /// </example>
    public static SnbtCompoundBuilder Compound() => new();

    /// <summary>
    /// Constructs and builds an <see cref="SnbtCompound"/> using a configuration action.
    /// </summary>
    /// <param name="configure">The delegate to configure the compound builder.</param>
    /// <returns>A fully built, immutable <see cref="SnbtCompound"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="configure"/> is null.</exception>
    /// <example>
    /// <code>
    /// var compound = Snbt.Compound(b => b
    ///     .Put("id", "minecraft:iron_sword")
    ///     .Put("count", (sbyte)1));
    /// </code>
    /// </example>
    public static SnbtCompound Compound(Action<SnbtCompoundBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        var builder = new SnbtCompoundBuilder();
        configure(builder);
        return builder.Build();
    }

    /// <summary>
    /// Creates a new fluent builder for constructing an <see cref="SnbtList"/>.
    /// </summary>
    /// <returns>A new <see cref="SnbtListBuilder"/> instance.</returns>
    /// <example>
    /// <code>
    /// var list = Snbt.List()
    ///     .Add("item1")
    ///     .Add("item2")
    ///     .Build();
    /// </code>
    /// </example>
    public static SnbtListBuilder List() => new();

    /// <summary>
    /// Constructs and builds an <see cref="SnbtList"/> using a configuration action.
    /// </summary>
    /// <param name="configure">The delegate to configure the list builder.</param>
    /// <returns>A fully built, immutable <see cref="SnbtList"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="configure"/> is null.</exception>
    /// <example>
    /// <code>
    /// var list = Snbt.List(l => l
    ///     .Add(1)
    ///     .Add(2)
    ///     .Add(3));
    /// </code>
    /// </example>
    public static SnbtList List(Action<SnbtListBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        var builder = new SnbtListBuilder();
        configure(builder);
        return builder.Build();
    }
}