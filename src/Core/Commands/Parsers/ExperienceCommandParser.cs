using Core.Commands.Impl;
using Core.Commands.Models.Interfaces;
using JetBrains.Annotations;
using Pidgin;
using static Pidgin.Parser;

namespace Core.Commands.Parsers;

/// <summary>
/// Handles the parsing of the /experience and /xp commands.
/// </summary>
public static class ExperienceCommandParser
{
    /// <summary>
    /// Parses the experience action (add, set, or query).
    /// </summary>
    private static readonly Parser<char, ExperienceAction> ActionParser =
        OneOf(
            String("add").Map(_ => ExperienceAction.Add),
            String("set").Map(_ => ExperienceAction.Set),
            String("query").Map(_ => ExperienceAction.Query)
        );

    /// <summary>
    /// Parses the unit of experience (levels or points).
    /// </summary>
    private static readonly Parser<char, ExperienceUnit> UnitParser =
        OneOf(
            String("levels").Map(_ => ExperienceUnit.Levels),
            String("points").Map(_ => ExperienceUnit.Points)
        );

    /// <summary>
    /// A parser that evaluates the arguments of the /experience command.
    /// </summary>
    /// <returns>A parser yielding an <see cref="ICommand"/> instance (specifically <see cref="ExperienceCommand"/>).</returns>
    /// <exception cref="ParseException">Thrown implicitly by Pidgin if the command syntax or arguments are malformed.</exception>
    /// <example>
    /// <code>
    /// ICommand cmd = ExperienceCommandParser.Parser.ParseOrThrow("add @a 10 levels");
    /// </code>
    /// </example>
    [PublicAPI]
    public static readonly Parser<char, ICommand> Parser =
        ActionParser
            .Bind(action => CommandArgsParser.Whitespace
            .Then(CommandArgsParser.TargetSelector)
            .Bind(target =>
            {
                // If the action is "query", we expect exactly a unit (levels or points)
                if (action == ExperienceAction.Query)
                {
                    return CommandArgsParser.Whitespace
                        .Then(UnitParser)
                        .Map(ICommand (unit) => new ExperienceCommand(action, target, Unit: unit));
                }

                // If the action is "add" or "set", we expect an amount and an optional unit
                return CommandArgsParser.Whitespace
                    .Then(CommandArgsParser.Integer) // Assuming this exists based on your GiveCommand parser
                    .Bind(amount => Try(CommandArgsParser.Whitespace.Then(UnitParser))
                        .Optional()
                        .Map(ICommand (unitOpt) => new ExperienceCommand(
                            action,
                            target,
                            amount,
                            unitOpt.HasValue ? unitOpt.Value : null
                        )));
            }));
}