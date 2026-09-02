using Core.Commands.Parsers;
using Core.McFunctions.Models;
using Core.McFunctions.Models.Interfaces;
using JetBrains.Annotations;
using Pidgin;
using static Pidgin.Parser;

namespace Core.McFunctions;

/// <summary>
/// A parser for Minecraft .mcfunction files.
/// </summary>
public static class McFunctionParser
{
    //  Match ONLY spaces and tabs (not newlines, otherwise it eats the whole file)
    private static readonly Parser<char, Unit> SkipSpaces =
        OneOf(' ', '\t').SkipMany();

    // Safely match Windows (\r\n) or Linux (\n) line endings
    // We use Try on \r\n so if it matches \r but not \n, it backtracks safely
    private static readonly Parser<char, Unit> EndOfLine =
        Try(String("\r\n")).Or(String("\n")).IgnoreResult();

    // A continuation is a '\' followed by optional spaces, an EOL, and more spaces.
    // It returns an empty string, effectively erasing the continuation from the final output.
    private static readonly Parser<char, string> LineContinuation =
        Try(
            Char('\\')
                .Then(SkipSpaces)
                .Then(EndOfLine)
                .Then(SkipSpaces)
                .ThenReturn(string.Empty) // Use ThenReturn to avoid delegate allocation
        );

    // A normal character is anything EXCEPT a newline. We convert it to a string
    // so its type matches LineContinuation (Parser<char, string>).
    private static readonly Parser<char, string> NormalTextChunk =
        AnyCharExcept('\r', '\n', '\\').AtLeastOnceString();

    // A logical character is EITHER a continuation (ignored) OR a normal char.
    // Since both return string, .Or() compiles perfectly without ambiguity.
    private static readonly Parser<char, string> LiteralBackslash =
        Char('\\').ThenReturn("\\");

    // A logical line is a sequence of these strings joined together
    private static readonly Parser<char, string> LogicalLine =
        OneOf(LineContinuation, NormalTextChunk, LiteralBackslash)
            .Many()
            .Select(string.Concat)
            .Select(s => s.Trim());

    // Categorize the parsed string into our IMcFunctionLine models
    private static readonly Parser<char, IMcFunctionLine> LineParser =
        LogicalLine.Select<IMcFunctionLine>(line =>
        {
            if (string.IsNullOrEmpty(line)) return new EmptyLine();

            // Using index access avoids generating short-lived substrings for the StartsWith check
            if (line[0] == '#') return new CommentLine(line[1..]);

            var isMacro = line[0] == '$';
            var cmdText = isMacro ? line[1..] : line;


            if (!CommandParser.TryParse(cmdText, out var parsedCommand))
                // Fallback: If the command is not registered yet (e.g. /execute, /scoreboard),
                // fallback to RawCommandLine so the file parsing continues without breaking.
                return new RawCommandLine(cmdText, isMacro);

            // If it was prefixed with '$', update the immutable record state
            if (isMacro)
                parsedCommand = parsedCommand.WithMacro(true);

            // Strictly parse the command into a concrete model (e.g. TellrawCommand)
            return new ExecutableLine(parsedCommand);
        });

    // The full file is multiple lines separated by EOL, optionally ending with an EOL
    private static readonly Parser<char, McFunction> FunctionParser =
        LineParser
            .SeparatedAndOptionallyTerminated(EndOfLine)
            .Select(lines => new McFunction([..lines]));

    /// <summary>
    /// Parses the raw text of an .mcfunction file using Pidgin.
    /// </summary>
    [PublicAPI]
    public static McFunction Parse(string rawText)
    {
        return FunctionParser.ParseOrThrow(rawText);
    }
}