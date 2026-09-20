using Pidgin;

namespace Core.Common;

internal static class ParserParts
{
    /// <summary>
    /// Parses an identifier like "minecraft:zombie" or "diamond_sword".
    /// </summary>
    public static readonly Parser<char, string> IdentifierParser =
        Parser<char>.Token(c => char.IsAsciiLetterOrDigit(c) || c is '_' or '-' or '.' or ':')
            .ManyString();
}