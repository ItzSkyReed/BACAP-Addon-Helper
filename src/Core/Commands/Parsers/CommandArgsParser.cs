using Pidgin;
using Core.Commands.Models;
using JetBrains.Annotations;

namespace Core.Commands.Parsers;

/// <summary>
/// Provides parsers for common Minecraft command arguments.
/// </summary>
public static class CommandArgsParser
{
    [PublicAPI]
    public static readonly Parser<char, Unit> Whitespace = Parser.SkipWhitespaces;


    /// <summary>
    /// Parses a target selector. Reads characters until a whitespace is encountered.
    /// This naively handles "@s", "@e[type=pig]", or player names.
    /// </summary>
    public static readonly Parser<char, Selector> TargetSelector =
        Parser<char>.Token(c => !char.IsWhiteSpace(c))
            .AtLeastOnceString()
            .Select(Selector.Custom);

    /// <summary>
    /// Parses a single coordinate component (e.g., "~", "^", or "-15.5").
    /// </summary>
    private static readonly Parser<char, string> CoordinatePart =
        Parser<char>.Token(c => !char.IsWhiteSpace(c)).AtLeastOnceString();

    private static readonly Parser<char, Position> Position3D =
        Parser.Map(
            (x, _, y, _, z) => new Position(x, y, z),
            CoordinatePart, Whitespace,
            CoordinatePart, Whitespace,
            CoordinatePart
        );

    private static readonly Parser<char, Position> Position2D =
        Parser.Map(
            (x, _, z) => new Position(x, z),
            CoordinatePart, Whitespace,
            CoordinatePart
        );

    /// <summary>
    /// Parses either a 3D position ("X Y Z") or a 2D position ("X Z").
    /// </summary>
    public static readonly Parser<char, Position> PositionParser =
        Parser.Try(Position3D).Or(Position2D);

    /// <summary>
    /// Parses an integer value.
    /// </summary>
    public static readonly Parser<char, int> Integer =
        Parser.Digit.AtLeastOnceString().Select(int.Parse);
}