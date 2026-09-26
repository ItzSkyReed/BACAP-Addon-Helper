using Core.SNBT.Interfaces;
using Core.SNBT.Nodes;
using Pidgin;

namespace Core.SNBT;

/// <summary>
/// Provides parsing functionality for converting Stringified NBT (SNBT) strings into structured node objects.
/// Supports standard Minecraft Java Edition data types, escape sequences, number formats, and nested structures.
/// </summary>
public static class SnbtParser
{
    private static readonly Parser<char, Unit> Whitespace = Parser.SkipWhitespaces;
    private static readonly Parser<char, ISnbtNode> Node = Parser.Rec(() => AnyNode!);

    #region Escapes

    private static bool IsHex(char c) => char.IsAsciiHexDigit(c);

    /// <summary>
    /// Parses \x, \u, \U escape sequences into valid Unicode characters.
    /// </summary>
    private static Parser<char, string> HexCodePoint(int length) =>
        Parser<char>.Token(IsHex).Repeat(length)
            .Select(chars =>
            {
                uint val = 0;
                foreach (var c in chars)
                {
                    val = (val << 4) + (uint)(c <= '9' ? c - '0' : (c & ~0x20) - 'A' + 10);
                }

                return val <= 0x10FFFF ? char.ConvertFromUtf32((int)val) : "\uFFFD";
            });

    // Fallback for \N{Name}, as .NET BCL does not have a built-in Unicode name dictionary
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
            // Fallback for unknown sequences: return the character verbatim
            Parser<char>.Any.Select(c => c.ToString())
        )
    );

    #endregion

    #region Strings & Booleans

    private static readonly Parser<char, string> UnescapedDoubleChunk =
        Parser<char>.Token(c => c != '"' && c != '\\').AtLeastOnceString();

    private static readonly Parser<char, string> UnescapedSingleChunk =
        Parser<char>.Token(c => c != '\'' && c != '\\').AtLeastOnceString();

    private static readonly Parser<char, string> DoubleQuotedString =
        Parser.OneOf(Escape, UnescapedDoubleChunk).Many()
            .Select(string.Concat).Between(Parser.Char('"'));

    private static readonly Parser<char, string> SingleQuotedString =
        Parser.OneOf(Escape, UnescapedSingleChunk).Many()
            .Select(string.Concat).Between(Parser.Char('\''));

    private static readonly Parser<char, string> AnyQuotedString =
        Parser.OneOf(DoubleQuotedString, SingleQuotedString);

    // Quoted strings ALWAYS produce SnbtString nodes, even if content is "true" or "false"
    private static readonly Parser<char, ISnbtNode> QuotedStringNode =
        AnyQuotedString.Select<ISnbtNode>(val => new SnbtString(val));

    private static bool IsUnquotedStart(char c) => char.IsAsciiLetter(c) || c == '_';
    private static bool IsUnquotedChar(char c) => char.IsAsciiLetterOrDigit(c) || c is '_' or '-' or '.' or '+';

    private static readonly Parser<char, string> UnquotedStringText =
        Parser<char>.Token(IsUnquotedStart)
            .Then(Parser<char>.Token(IsUnquotedChar).ManyString(), (first, rest) => first + rest);

    // Unquoted strings resolve to SnbtBool if matching true/false, otherwise SnbtString
    private static readonly Parser<char, ISnbtNode> UnquotedStringOrBoolNode =
        UnquotedStringText.Select<ISnbtNode>(val => val switch
        {
            "true" => new SnbtBool(true),
            "false" => new SnbtBool(false),
            _ => new SnbtString(val)
        });

    #endregion

    #region Numbers

    private static bool IsNumberStart(char c) => char.IsAsciiDigit(c) || c is '-' or '+' or '.';

    private static readonly Parser<char, ISnbtNode> NumberNode =
        Parser<char>.Token(IsNumberStart)
            .Then(Parser<char>.Token(IsUnquotedChar).ManyString(), (first, rest) => first + rest)
            .Select(SnbtNumberParser.Parse);

    #endregion

    #region Arrays & Lists

    private static readonly Parser<char, IEnumerable<ISnbtNode>> Elements =
        Node.Separated(Parser.Char(',').Between(Whitespace));

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

    #endregion

    #region Compounds

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
                var dict = new Dictionary<string, ISnbtNode>();
                foreach (var kvp in kvs)
                    dict[kvp.Key] = kvp.Value;

                return new SnbtCompound(dict);
            });

    #endregion

    #region Root Parser

    internal static readonly Parser<char, ISnbtNode> AnyNode =
        Parser.OneOf(
            Compound,
            ByteArray,
            IntArray,
            LongArray,
            SnbtList,
            NumberNode,
            QuotedStringNode,
            UnquotedStringOrBoolNode
        ).Between(Whitespace);

    private static readonly Parser<char, ISnbtNode> FullInputParser =
        AnyNode.Before(Parser<char>.End);

    /// <summary>
    /// Parses a Stringified NBT (SNBT) string and converts it into its corresponding <see cref="ISnbtNode"/> representation.
    /// </summary>
    /// <param name="input">The SNBT string to parse (e.g., "{Damage:10, id:\"minecraft:stick\"}").</param>
    /// <returns>An instance of <see cref="ISnbtNode"/> representing the root of the parsed data.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the input string is null.</exception>
    /// <exception cref="ParseException">Thrown when the input string contains invalid SNBT syntax or unparsed trailing tokens.</exception>
    /// <example>
    /// <code>
    /// string snbt = "{name:\"Sword\", damage:15.5f, tags:[\"weapon\", \"metal\"]}";
    /// ISnbtNode result = SnbtParser.Parse(snbt);
    /// </code>
    /// </example>
    public static ISnbtNode Parse(string input)
    {
        ArgumentNullException.ThrowIfNull(input);
        return FullInputParser.ParseOrThrow(input);
    }

    #endregion
}