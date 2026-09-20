using Core.Commands.Impl;
using Core.Commands.Models.Interfaces;
using JetBrains.Annotations;
using Pidgin;

namespace Core.Commands.Parsers;

/// <summary>
/// Handles the parsing of the /execute command using a hybrid approach.
/// It splits the raw command string by the "run" keyword to isolate modifiers
/// and recursively parses the nested command.
/// </summary>
public static class ExecuteCommandParser
{
    /// <summary>
    /// A parser that consumes the remaining execute command string, splits it to find the 'run' command,
    /// and recursively evaluates the nested command using <see cref="CommandParser.Root"/>.
    /// </summary>
    /// <returns>A parser yielding an <see cref="ICommand"/> instance (specifically <see cref="ExecuteCommand"/>).</returns>
    /// <exception cref="ParseException">Thrown implicitly by Pidgin if the nested run command fails to parse.</exception>
    /// <example>
    /// <code>
    /// ICommand cmd = ExecuteCommandParser.Parser.ParseOrThrow("as @a at @s run give @s diamond 1");
    /// </code>
    /// </example>
    [PublicAPI] public static readonly Parser<char, ICommand> Parser =
        Parser<char>.Any.ManyString().Bind(rest =>
        {
            var input = rest.Trim();
            var modifiers = string.Empty;
            var runString = string.Empty;

            // Find the boundary between modifiers and the 'run' command
            if (input.StartsWith("run ", StringComparison.OrdinalIgnoreCase))
                runString = input[4..].Trim();
            else
            {
                var runIndex = input.IndexOf(" run ", StringComparison.OrdinalIgnoreCase);
                if (runIndex != -1)
                {
                    modifiers = input[..runIndex].Trim();
                    runString = input[(runIndex + 5)..].Trim();
                }
                else if (input.EndsWith(" run", StringComparison.OrdinalIgnoreCase))
                    modifiers = input[..^4].Trim();
                else
                    modifiers = input; // No 'run' command found
            }


            if (string.IsNullOrEmpty(runString))
                // Return execute without a run block
                return Parser<char>.Return<ICommand>(new ExecuteCommand(modifiers, null));

            // If there is a run part, recursively parse it using the Root parser
            var runResult = CommandParser.Root.Parse(runString);

            // If the nested command parses successfully, return the combined ExecuteCommand
            return runResult.Success
                ? Parser<char>.Return<ICommand>(new ExecuteCommand(modifiers, runResult.Value))
                // If nested parsing fails, fail the whole execute block so it falls back to RawCommandLine
                : Parser<char>.Fail<ICommand>($"Failed to parse nested run command: {runString}");
        });
}