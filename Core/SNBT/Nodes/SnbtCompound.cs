using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;
using Core.SNBT.Interfaces;
using JetBrains.Annotations;

namespace Core.SNBT.Nodes;

/// <summary>
/// Represents a compound tag containing a dictionary of named <see cref="ISnbtNode"/> instances.
/// </summary>
/// <param name="Tags">The underlying dictionary containing key-node pairs.</param>
public record SnbtCompound(Dictionary<string, ISnbtNode> Tags) : ISnbtNode
{
    /// <summary>
    /// Initializes a new empty instance of the <see cref="SnbtCompound"/> record.
    /// </summary>
    [PublicAPI]
    public SnbtCompound() : this(new Dictionary<string, ISnbtNode>())
    {
    }

    #region Serialization

    /// <summary>
    /// Serializes this compound node into its Stringified NBT (SNBT) representation.
    /// </summary>
    /// <param name="pretty">If <see langword="true"/>, formats the output with indents and line breaks.</param>
    /// <param name="indent">The current indentation prefix for recursive formatting.</param>
    /// <returns>A formatted SNBT string.</returns>
    public string ToSnbtString(bool pretty = false, string indent = "")
    {
        if (Tags.Count == 0) return "{}";

        if (!pretty)
        {
            var pairs = Tags.Select(kv => $"{FormatKey(kv.Key)}:{kv.Value.ToSnbtString()}");
            return "{" + string.Join(",", pairs) + "}";
        }

        var sb = new StringBuilder();
        sb.AppendLine("{");
        var nextIndent = indent + "  ";
        var count = 0;

        foreach (var kvp in Tags)
        {
            sb.Append($"{nextIndent}{FormatKey(kvp.Key)}: {kvp.Value.ToSnbtString(true, nextIndent)}");
            if (++count < Tags.Count) sb.AppendLine(",");
            else sb.AppendLine();
        }

        sb.Append(indent + "}");
        return sb.ToString();
    }

    private static string FormatKey(string key)
    {
        if (string.IsNullOrEmpty(key))
            return "\"\"";

        var needsQuotes = key.Any(c => !char.IsLetterOrDigit(c) && c is not ('_' or '-' or '.'));
        return needsQuotes ? $"\"{key}\"" : key;
    }

    #endregion

    #region Direct Node Access

    /// <summary>
    /// Retrieves a child node by its tag name.
    /// </summary>
    /// <param name="key">The tag name.</param>
    /// <returns>The matching <see cref="ISnbtNode"/>, or <see langword="null"/> if not found.</returns>
    public ISnbtNode? GetNode(string key) => Tags.GetValueOrDefault(key);

    /// <summary>
    /// Indexer to retrieve a raw child node by key.
    /// </summary>
    public ISnbtNode? this[string key] => GetNode(key);

    #endregion

    #region Generic Getters

    /// <summary>
    /// Attempts to retrieve and convert a value of the specified type <typeparamref name="T"/> associated with the given key.
    /// </summary>
    /// <typeparam name="T">The target primitive type, string, or <see cref="ISnbtNode"/> implementation.</typeparam>
    /// <param name="key">The tag name.</param>
    /// <param name="value">When this method returns, contains the converted value if found and valid; otherwise, <see langword="default"/>.</param>
    /// <returns><see langword="true"/> if the value was successfully extracted and converted; otherwise, <see langword="false"/>.</returns>
    public bool TryGet<T>(string key, [MaybeNullWhen(false)] out T value)
    {
        if (Tags.TryGetValue(key, out var node))
        {
            if (node is T exactMatch)
            {
                value = exactMatch;
                return true;
            }

            if (TryConvertValue(node, out value))
                return true;
        }

        value = default;
        return false;
    }

    /// <summary>
    /// Gets a value of type <typeparamref name="T"/> by key, or returns the specified default fallback.
    /// </summary>
    /// <typeparam name="T">The target primitive type, string, or <see cref="ISnbtNode"/> implementation.</typeparam>
    /// <param name="key">The tag name.</param>
    /// <param name="defaultValue">The fallback value if the key does not exist or cannot be converted.</param>
    /// <returns>The converted value or <paramref name="defaultValue"/>.</returns>
    public T Get<T>(string key, T defaultValue = default!) =>
        TryGet<T>(key, out var value) ? value : defaultValue;

    /// <summary>
    /// Gets an optional value of type <typeparamref name="T"/> by key if present and convertible; otherwise returns <see langword="null"/>.
    /// </summary>
    /// <typeparam name="T">The target primitive type, string, or <see cref="ISnbtNode"/> implementation.</typeparam>
    /// <param name="key">The tag name.</param>
    /// <returns>The converted value if found; otherwise, <see langword="null"/>.</returns>
    public T? GetOptional<T>(string key) =>
        TryGet<T>(key, out var value) ? value : default;

    #endregion

    #region Specific Getters (Reusing Generic Core)

    /// <summary>
    /// Gets a boolean value by key, checking if the byte value is non-zero.
    /// </summary>
    public bool GetBool(string key, bool defaultValue = false) => Get(key, defaultValue);

    /// <summary>
    /// Gets a 32-bit floating-point value, automatically coercing numeric node types.
    /// </summary>
    public float GetFloat(string key, float defaultValue = 0f) => Get(key, defaultValue);

    /// <summary>
    /// Gets a 32-bit integer value, automatically widening byte or short node types.
    /// </summary>
    public int GetInt(string key, int defaultValue = 0) => Get(key, defaultValue);

    /// <summary>
    /// Gets a 64-bit integer value, automatically widening smaller integer node types.
    /// </summary>
    public long GetLong(string key, long defaultValue = 0L) => Get(key, defaultValue);

    /// <summary>
    /// Gets a 64-bit floating-point value, automatically widening numeric node types.
    /// </summary>
    public double GetDouble(string key, double defaultValue = 0.0) => Get(key, defaultValue);

    /// <summary>
    /// Gets a string value by key.
    /// </summary>
    public string GetString(string key, string defaultValue = "") => Get(key, defaultValue);

    #endregion

    #region Specific Nullable Getters (Reusing Generic Core)

    /// <summary>
    /// Gets a boolean value if present and valid; otherwise returns <see langword="null"/>.
    /// </summary>
    public bool? GetOptionalBool(string key)
    {
        var node = GetNode(key);

        if (node is SnbtBool snbtBool)
            return snbtBool.Value;

        return null;
    }

    /// <summary>
    /// Gets a float value if present and numeric; otherwise returns <see langword="null"/>.
    /// </summary>
    public float? GetOptionalFloat(string key)
    {
        return GetNode(key) switch
        {
            SnbtFloat f => f.Value,
            SnbtDouble d => (float)d.Value,
            SnbtInt i => i.Value,
            SnbtByte b => b.Value,
            SnbtShort s => s.Value,
            _ => null
        };
    }

    /// <summary>
    /// Gets an integer value if present and compatible; otherwise returns <see langword="null"/>.
    /// </summary>
    public int? GetOptionalInt(string key)
    {
        return GetNode(key) switch
        {
            SnbtInt i => i.Value,
            SnbtByte b => b.Value,
            SnbtShort s => s.Value,
            SnbtLong l => (int)l.Value,
            SnbtFloat f => (int)f.Value,
            SnbtDouble d => (int)d.Value,
            _ => null
        };
    }

    /// <summary>
    /// Gets a long value if present and compatible; otherwise returns <see langword="null"/>.
    /// </summary>
    public long? GetOptionalLong(string key)
    {
        return GetNode(key) switch
        {
            SnbtLong l => l.Value,
            SnbtInt i => i.Value,
            SnbtByte b => b.Value,
            SnbtShort s => s.Value,
            SnbtFloat f => (long)f.Value,
            SnbtDouble d => (long)d.Value,
            _ => null
        };
    }

    /// <summary>
    /// Gets a double value if present and numeric; otherwise returns <see langword="null"/>.
    /// </summary>
    public double? GetOptionalDouble(string key)
    {
        return GetNode(key) switch
        {
            SnbtDouble d => d.Value,
            SnbtFloat f => f.Value,
            SnbtLong l => l.Value,
            SnbtInt i => i.Value,
            SnbtByte b => b.Value,
            SnbtShort s => s.Value,
            _ => null
        };
    }

    /// <summary>
    /// Gets a string value if present; otherwise returns <see langword="null"/>.
    /// </summary>
    public string? GetOptionalString(string key)
    {
        return GetNode(key) is SnbtString str ? str.Value : null;
    }

    #endregion

    #region Conversion Logic

    private static bool TryConvertValue<T>(ISnbtNode node, [MaybeNullWhen(false)] out T value)
    {
        value = default;

        if (typeof(T) == typeof(bool))
        {
            switch (node)
            {
                case SnbtBool bl:
                    Unsafe.As<T, bool>(ref value!) = bl.Value;
                    return true;
                case SnbtByte b:
                    Unsafe.As<T, bool>(ref value!) = b.Value != 0;
                    return true;
            }
        }
        else if (typeof(T) == typeof(int))
        {
            switch (node)
            {
                case SnbtInt i:
                    Unsafe.As<T, int>(ref value!) = i.Value;
                    return true;
                case SnbtShort s:
                    Unsafe.As<T, int>(ref value!) = s.Value;
                    return true;
                case SnbtByte b:
                    Unsafe.As<T, int>(ref value!) = b.Value;
                    return true;
            }
        }
        else if (typeof(T) == typeof(long))
        {
            switch (node)
            {
                case SnbtLong l:
                    Unsafe.As<T, long>(ref value!) = l.Value;
                    return true;
                case SnbtInt i:
                    Unsafe.As<T, long>(ref value!) = i.Value;
                    return true;
                case SnbtShort s:
                    Unsafe.As<T, long>(ref value!) = s.Value;
                    return true;
                case SnbtByte b:
                    Unsafe.As<T, long>(ref value!) = b.Value;
                    return true;
            }
        }
        else if (typeof(T) == typeof(float))
        {
            switch (node)
            {
                case SnbtFloat f:
                    Unsafe.As<T, float>(ref value!) = f.Value;
                    return true;
                case SnbtDouble d:
                    Unsafe.As<T, float>(ref value!) = (float)d.Value;
                    return true;
                case SnbtInt i:
                    Unsafe.As<T, float>(ref value!) = i.Value;
                    return true;
                case SnbtLong l:
                    Unsafe.As<T, float>(ref value!) = l.Value;
                    return true;
                case SnbtShort s:
                    Unsafe.As<T, float>(ref value!) = s.Value;
                    return true;
                case SnbtByte b:
                    Unsafe.As<T, float>(ref value!) = b.Value;
                    return true;
            }
        }
        else if (typeof(T) == typeof(double))
        {
            switch (node)
            {
                case SnbtDouble d:
                    Unsafe.As<T, double>(ref value!) = d.Value;
                    return true;
                case SnbtFloat f:
                    Unsafe.As<T, double>(ref value!) = f.Value;
                    return true;
                case SnbtInt i:
                    Unsafe.As<T, double>(ref value!) = i.Value;
                    return true;
                case SnbtLong l:
                    Unsafe.As<T, double>(ref value!) = l.Value;
                    return true;
                case SnbtShort s:
                    Unsafe.As<T, double>(ref value!) = s.Value;
                    return true;
                case SnbtByte b:
                    Unsafe.As<T, double>(ref value!) = b.Value;
                    return true;
            }
        }
        else if (typeof(T) == typeof(string))
        {
            if (node is not SnbtString str)
                return false;

            Unsafe.As<T, string?>(ref value!) = str.Value;
            return true;
        }

        return false;
    }

    #endregion
}