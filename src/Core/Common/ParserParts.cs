using Pidgin;

namespace Core.Common;

/// <summary>
/// Provides shared parser combinators for Minecraft syntax elements.
/// </summary>
internal static class ParserParts
{
    private static bool IsNamespaceChar(char c) =>
        char.IsAsciiLetterOrDigit(c) || c is '_' or '-' or '.';

    private static bool IsPathChar(char c) =>
        char.IsAsciiLetterOrDigit(c) || c is '_' or '-' or '.' or '/';

    private static readonly Parser<char, string> NamespaceToken =
        Parser<char>.Token(IsNamespaceChar).AtLeastOnceString();

    private static readonly Parser<char, string> PathToken =
        Parser<char>.Token(IsPathChar).AtLeastOnceString();

    /// <summary>
    /// Parses a valid Minecraft ResourceLocation identifier with an optional namespace (e.g., "minecraft:diamond_sword", "stick", or "bacap:item/diamond_sword").
    /// </summary>
    /// <remarks>
    /// Rejects empty inputs, standalone colons, and leading/trailing colons like ":stick" or "minecraft:".
    /// </remarks>
    public static readonly Parser<char, string> IdentifierParser =
        Parser.Try(NamespaceToken.Before(Parser.Char(':')))
            .Optional()
            .Then(PathToken, (ns, path) => ns.HasValue ? $"{ns.Value}:{path}" : path);
}