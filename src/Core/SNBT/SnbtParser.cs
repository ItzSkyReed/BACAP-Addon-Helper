using Core.SNBT.Interfaces;
using Core.SNBT.Nodes;
using Pidgin;

namespace Core.SNBT;

/// <summary>
/// Provides parsing functionality for converting Stringified NBT (SNBT) strings into structured node objects.
/// </summary>
public static class SnbtParser
{
    private static readonly Parser<char, Unit> Whitespace = Parser.SkipWhitespaces;
    private static readonly Parser<char, ISnbtNode> Node = Parser.Rec(() => AnyNode!);

    // Escaping
    private static bool IsHex(char c) => char.IsAsciiHexDigit(c);

    /// <summary>
    /// Parses \x, \u, \U escape sequences.
    /// </summary>
    private static Parser<char, string> HexCodePoint(int length) =>
        Parser<char>.Token(IsHex).Repeat(length)
            .Select(chars =>
            {
                var val = 0;
                foreach (var c in chars)
                {
                    val = (val << 4) + (c <= '9' ? c - '0' : (c & ~0x20) - 'A' + 10);
                }
                return char.ConvertFromUtf32(val);
            });

    // Fallback for \N{Name}, as C# doesn't have a built-in Unicode name dictionary
    private static readonly Parser<char, string> UnicodeNameEscape =
        Parser.Char('{').Then(Parser<char>.Token(c => c != '}').ManyString()).Before(Parser.Char('}'))
            .Select(name => $"?{name}?");

    private static readonly Parser<char, string> Escape = Parser.Char('\\').Then(
        Parser.OneOf(
            Parser.Char('b').ThenReturn("\b"),
            Parser.Char('f').ThenReturn("\f"),
            Parser.Char('n').ThenReturn("\n"),
            Parser.Char('r').ThenReturn("\r"),
            Parser.Char('s').ThenReturn(" "),
            Parser.Char('t').ThenReturn("\t"),
            Parser.Char('\\').ThenReturn("\\"),
            Parser.Char('\'').ThenReturn("'"),
            Parser.Char('"').ThenReturn("\""),
            Parser.Char('x').Then(HexCodePoint(2)),
            Parser.Char('u').Then(HexCodePoint(4)),
            Parser.Char('U').Then(HexCodePoint(8)),
            Parser.Char('N').Then(UnicodeNameEscape),
            // Fallback for unknown sequences: just return the character itself
            Parser<char>.Any.Select(c => c.ToString())
        )
    );

    // Strings
    // Regular characters now return strings so they can be concatenated with escape sequences
    private static readonly Parser<char, string> UnescapedDoubleChunk =
        Parser<char>.Token(c => c != '"' && c != '\\').AtLeastOnceString();

    private static readonly Parser<char, string> UnescapedSingleChunk =
        Parser<char>.Token(c => c != '\'' && c != '\\').AtLeastOnceString();

    // Assemble string parts and escape sequences using string.Join
    private static readonly Parser<char, string> DoubleQuotedString =
        Parser.OneOf(Escape, UnescapedDoubleChunk).Many()
            .Select(string.Concat).Between(Parser.Char('"'));

    private static readonly Parser<char, string> SingleQuotedString =
        Parser.OneOf(Escape, UnescapedSingleChunk).Many()
            .Select(string.Concat).Between(Parser.Char('\''));

    private static readonly Parser<char, string> AnyQuotedString =
        Parser.OneOf(DoubleQuotedString, SingleQuotedString);

    private static bool IsUnquotedStart(char c) => char.IsAsciiLetter(c) || c == '_';
    private static bool IsUnquotedChar(char c) => char.IsAsciiLetterOrDigit(c) || c == '_' || c == '-' || c == '.' || c == '+';

    private static readonly Parser<char, string> UnquotedStringText =
        Parser<char>.Token(IsUnquotedStart)
            .Then(Parser<char>.Token(IsUnquotedChar).ManyString(), (first, rest) => first + rest);

    private static readonly Parser<char, ISnbtNode> StringOrBoolNode =
        Parser.OneOf(AnyQuotedString, UnquotedStringText)
            .Select(val => val switch
            {
                "true" => new SnbtBool(true) as ISnbtNode,
                "false" => new SnbtBool(false),
                _ => new SnbtString(val)
            });

    // Numbers
    private static bool IsNumberStart(char c) => char.IsAsciiDigit(c) || c is '-' or '+' or '.';

    private static readonly Parser<char, ISnbtNode> NumberNode =
        Parser<char>.Token(IsNumberStart)
            .Then(Parser<char>.Token(IsUnquotedChar).ManyString(), (first, rest) => first + rest)
            .Select(SnbtNumberParser.Parse); // Relies on SnbtNumberParser for strict numeric types

    // Collects
    private static readonly Parser<char, IEnumerable<ISnbtNode>> Elements =
        Node.Separated(Parser.Char(',').Between(Whitespace));

    // Uses .ToList() to ensure strict lists instead of lazy IEnumerable evaluations
    private static readonly Parser<char, ISnbtNode> ByteArray =
        Elements.Between(Parser.Try(Parser.String("[B;")).Between(Whitespace), Parser.Char(']').Between(Whitespace))
            .Select<ISnbtNode>(items => new SnbtByteArray(items.ToList()));

    private static readonly Parser<char, ISnbtNode> IntArray =
        Elements.Between(Parser.Try(Parser.String("[I;")).Between(Whitespace), Parser.Char(']').Between(Whitespace))
            .Select<ISnbtNode>(items => new SnbtIntArray(items.ToList()));

    private static readonly Parser<char, ISnbtNode> LongArray =
        Elements.Between(Parser.Try(Parser.String("[L;")).Between(Whitespace), Parser.Char(']').Between(Whitespace))
            .Select<ISnbtNode>(items => new SnbtLongArray(items.ToList()));

    private static readonly Parser<char, ISnbtNode> SnbtList =
        Elements.Between(Parser.Char('[').Between(Whitespace), Parser.Char(']').Between(Whitespace))
            .Select<ISnbtNode>(items => new SnbtList(items.ToList()));

    // Compounds
    private static readonly Parser<char, string> CompoundKey =
        Parser.OneOf(
            AnyQuotedString,
            Parser<char>.Token(IsUnquotedChar).AtLeastOnceString()
        );

    private static readonly Parser<char, KeyValuePair<string, ISnbtNode>> KeyValuePair =
        CompoundKey
            .Before(Parser.Char(':').Between(Whitespace))
            .Then(Node, (key, value) => new KeyValuePair<string, ISnbtNode>(key, value));

    private static readonly Parser<char, ISnbtNode> Compound =
        KeyValuePair
            .Separated(Parser.Char(',').Between(Whitespace))
            .Between(Parser.Char('{').Between(Whitespace), Parser.Char('}').Between(Whitespace))
            .Select<ISnbtNode>(kvs =>
            {
                // Do not use the `new Dictionary(IEnumerable)` constructor.
                // If a compound has duplicate keys (common in manual / messy SNBTs),
                // the Dictionary constructor throws an ArgumentException and crashes the parser.
                // A foreach loop safely overwrites old keys (acting like Minecraft does).
                var dict = new Dictionary<string, ISnbtNode>();
                foreach (var kvp in kvs)
                    dict[kvp.Key] = kvp.Value;
                return new SnbtCompound(dict);
            });

    // Main switch
    internal static readonly Parser<char, ISnbtNode> AnyNode =
        Parser.OneOf(
            Compound,
            ByteArray,
            IntArray,
            LongArray,
            SnbtList,
            NumberNode,
            StringOrBoolNode
        ).Between(Whitespace);

    /// <summary>
    /// Parses a Stringified NBT (SNBT) string and converts it into its corresponding <see cref="ISnbtNode"/> representation.
    /// </summary>
    /// <param name="input">The SNBT string to parse (e.g., "{Damage:10, id:\"minecraft:stick\"}").</param>
    /// <returns>An instance of <see cref="ISnbtNode"/> representing the root of the parsed data (typically an <see cref="SnbtCompound"/>).</returns>
    /// <exception cref="Pidgin.ParseException">Thrown when the input string contains invalid SNBT syntax.</exception>
    /// <exception cref="System.ArgumentNullException">Thrown when the input string is null.</exception>
    /// <example>
    /// <code>
    /// string snbt = "{name:\"Sword\", damage:15.5f, tags:[\"weapon\", \"metal\"]}";
    /// ISnbtNode result = SnbtParser.Parse(snbt);
    /// </code>
    /// </example>
    public static ISnbtNode Parse(string input) => AnyNode.ParseOrThrow(input);
}