using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;
using Core.SNBT.Interfaces;
using JetBrains.Annotations;

namespace Core.SNBT.Nodes;

/// <summary>
/// Represents an SNBT compound tag containing key-value pairs enclosed in curly braces.
/// </summary>
public sealed record SnbtCompound : ISnbtNode
{
    /// <summary>
    /// Gets the underlying dictionary containing tag names and their associated nodes.
    /// </summary>
    public Dictionary<string, ISnbtNode> Tags { get; init; }

    /// <summary>
    /// Initializes a new empty instance of the <see cref="SnbtCompound"/> record.
    /// </summary>
    [PublicAPI]
    public SnbtCompound() : this(new Dictionary<string, ISnbtNode>())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SnbtCompound"/> record.
    /// </summary>
    /// <param name="tags">The dictionary containing key-node pairs.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="tags"/> or any of its keys or values is null.</exception>
    public SnbtCompound(Dictionary<string, ISnbtNode> tags)
    {
        ArgumentNullException.ThrowIfNull(tags);

        foreach (var (key, value) in tags)
        {
            ArgumentNullException.ThrowIfNull(key, nameof(tags));
            ArgumentNullException.ThrowIfNull(value, $"{nameof(tags)}[{key}]");
        }

        Tags = tags;
    }

    #region Serialization

    /// <summary>
    /// Serializes this compound node into its Stringified NBT (SNBT) representation.
    /// </summary>
    /// <param name="pretty">If <see langword="true"/>, formats the output with indents and line breaks.</param>
    /// <param name="indent">The current indentation prefix for recursive formatting.</param>
    /// <returns>A formatted SNBT compound string.</returns>
    /// <example>
    /// <code>
    /// var compound = new SnbtCompound();
    /// string snbt = compound.ToSnbtString(); // returns "{}"
    /// </code>
    /// </example>
    public string ToSnbtString(bool pretty = false, string indent = "")
    {
        if (Tags.Count == 0)
            return "{}";

        if (!pretty)
        {
            var sb = new StringBuilder("{");
            var isFirst = true;

            foreach (var (key, value) in Tags)
            {
                if (!isFirst)
                    sb.Append(',');

                sb.Append(FormatKey(key));
                sb.Append(':');
                sb.Append(value.ToSnbtString(pretty: false, indent: string.Empty));
                isFirst = false;
            }

            sb.Append('}');
            return sb.ToString();
        }

        var prettySb = new StringBuilder();
        prettySb.AppendLine("{");
        var nextIndent = indent + "  ";
        var count = 0;

        foreach (var (key, value) in Tags)
        {
            prettySb.Append(nextIndent);
            prettySb.Append(FormatKey(key));
            prettySb.Append(": ");
            prettySb.Append(value.ToSnbtString(pretty: true, indent: nextIndent));

            if (++count < Tags.Count)
                prettySb.AppendLine(",");
            else
                prettySb.AppendLine();
        }

        prettySb.Append(indent);
        prettySb.Append('}');
        return prettySb.ToString();
    }

    /// <summary>
    /// Formats an SNBT key according to Minecraft specification rules.
    /// Keys containing only [a-zA-Z0-9_\-\.\+] and not starting with [0-9\-\.\+] can remain unquoted.
    /// </summary>
    /// <param name="key">The raw tag name.</param>
    /// <returns>A properly escaped and quoted string if required; otherwise, the raw key.</returns>
    public static string FormatKey(string key)
    {
        if (string.IsNullOrEmpty(key))
        {
            return "\"\"";
        }

        // The key must not begin with 0-9, -, ., or +
        var first = key[0];
        if (char.IsAsciiDigit(first) || first is '-' or '.' or '+')
            return QuoteAndEscapeString(key);

        // Quote enclosure is optional if the string contains only 0-9, A-Z, a-z, _, -, ., and +
        foreach (var c in key)
        {
            if (!char.IsAsciiLetterOrDigit(c) && c is not ('_' or '-' or '.' or '+'))
                return QuoteAndEscapeString(key);
        }

        return key;
    }

    private static string QuoteAndEscapeString(string value)
    {
        var sb = new StringBuilder(value.Length + 4);
        sb.Append('"');

        foreach (var c in value)
        {
            switch (c)
            {
                case '\\':
                    sb.Append(@"\\");
                    break;
                case '"':
                    sb.Append("\\\"");
                    break;
                case '\b':
                    sb.Append("\\b");
                    break;
                case '\f':
                    sb.Append("\\f");
                    break;
                case '\n':
                    sb.Append("\\n");
                    break;
                case '\r':
                    sb.Append("\\r");
                    break;
                case '\t':
                    sb.Append("\\t");
                    break;
                default:
                    sb.Append(c);
                    break;
            }
        }

        sb.Append('"');
        return sb.ToString();
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
    /// <param name="key">The tag name.</param>
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
    public T? GetOptional<T>(string key) where T : struct =>
        TryGet<T>(key, out var value) ? value : null;

    #endregion

    #region Specific Getters

    /// <summary>
    /// Gets a boolean value by key, converting byte values to boolean (non-zero is true).
    /// </summary>
    public bool GetBool(string key, bool defaultValue = false) => Get(key, defaultValue);

    /// <summary>
    /// Gets a 32-bit floating-point value, automatically widening numeric node types.
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
    public string GetString(string key, string defaultValue = "") =>
        TryGet<string>(key, out var value) ? value : defaultValue;

    #endregion

    #region Specific Nullable Getters

    /// <summary>
    /// Gets a boolean value if present and valid; otherwise returns <see langword="null"/>.
    /// </summary>
    public bool? GetOptionalBool(string key) => GetOptional<bool>(key);

    /// <summary>
    /// Gets a float value if present and numeric; otherwise returns <see langword="null"/>.
    /// </summary>
    public float? GetOptionalFloat(string key) => GetOptional<float>(key);

    /// <summary>
    /// Gets an integer value if present and compatible; otherwise returns <see langword="null"/>.
    /// </summary>
    public int? GetOptionalInt(string key) => GetOptional<int>(key);

    /// <summary>
    /// Gets a long value if present and compatible; otherwise returns <see langword="null"/>.
    /// </summary>
    public long? GetOptionalLong(string key) => GetOptional<long>(key);

    /// <summary>
    /// Gets a double value if present and numeric; otherwise returns <see langword="null"/>.
    /// </summary>
    public double? GetOptionalDouble(string key) => GetOptional<double>(key);

    /// <summary>
    /// Gets a string value if present; otherwise returns <see langword="null"/>.
    /// </summary>
    public string? GetOptionalString(string key) =>
        TryGet<string>(key, out var str) ? str : null;

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
            if (node is not SnbtString str) return false;

            Unsafe.As<T, string?>(ref value!) = str.Value;
            return true;
        }

        return false;
    }

    #endregion

    #region Equality

    /// <summary>
    /// Determines whether the specified compound contains identical key-node pairs.
    /// </summary>
    /// <param name="other">The other compound to compare with.</param>
    /// <returns><see langword="true"/> if both compounds contain equal key-value pairs; otherwise, <see langword="false"/>.</returns>
    public bool Equals(SnbtCompound? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        if (Tags.Count != other.Tags.Count)
            return false;

        foreach (var (key, value) in Tags)
        {
            if (!other.Tags.TryGetValue(key, out var otherValue))
                return false;

            if (!EqualityComparer<ISnbtNode>.Default.Equals(value, otherValue))
                return false;
        }

        return true;
    }

    /// <summary>
    /// Computes a hash code based on the compound's key-value pairs in order-independent fashion.
    /// </summary>
    /// <returns>An integer hash code.</returns>
    public override int GetHashCode()
    {
        var hash = 0;
        foreach (var (key, value) in Tags)
        {
            var entryHash = HashCode.Combine(key, value);
            hash ^= entryHash; // XOR provides order-independent combination
        }

        return hash;
    }

    #endregion
}