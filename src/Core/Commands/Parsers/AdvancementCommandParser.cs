using Core.Commands.Impl;
using Core.Commands.Models.Interfaces;
using Core.Common;
using JetBrains.Annotations;
using Pidgin;
using static Pidgin.Parser;
using static Pidgin.Parser<char>;

namespace Core.Commands.Parsers;

/// <summary>
/// Handles the parsing of the /advancement command.
/// </summary>
public static class AdvancementCommandParser
{
    /// <summary>
    /// Parses the advancement action (grant or revoke).
    /// </summary>
    private static readonly Parser<char, AdvancementAction> ActionParser =
        OneOf(
            String("grant").Map(_ => AdvancementAction.Grant),
            String("revoke").Map(_ => AdvancementAction.Revoke)
        );

    /// <summary>
    /// Parses the scope mode of the advancement modification.
    /// </summary>
    private static readonly Parser<char, AdvancementMode> ModeParser =
        OneOf(
            String("everything").Map(_ => AdvancementMode.Everything),
            String("only").Map(_ => AdvancementMode.Only),
            String("from").Map(_ => AdvancementMode.From),
            String("through").Map(_ => AdvancementMode.Through),
            String("until").Map(_ => AdvancementMode.Until)
        );

    /// <summary>
    /// A parser that evaluates the arguments of the /advancement command.
    /// </summary>
    /// <returns>A parser yielding an <see cref="ICommand"/> instance (specifically <see cref="AdvancementCommand"/>).</returns>
    /// <exception cref="ParseException">Thrown implicitly by Pidgin if the command syntax or arguments are malformed.</exception>
    /// <example>
    /// <code>
    /// ICommand cmd = AdvancementCommandParser.Parser.ParseOrThrow("grant @a only minecraft:story/root");
    /// </code>
    /// </example>
    [PublicAPI]
    public static readonly Parser<char, ICommand> Parser =
        Map((action, _, target, _, mode) => (Action: action, Target: target, Mode: mode),
            ActionParser,
            CommandArgsParser.Whitespace,
            CommandArgsParser.TargetSelector,
            CommandArgsParser.Whitespace,
            ModeParser
        ).Bind(ctx =>
        {
            // If mode is "everything", there are no more arguments to parse
            if (ctx.Mode == AdvancementMode.Everything)
            {
                return Return<ICommand>(new AdvancementCommand(ctx.Action, ctx.Target, ctx.Mode));
            }

            // Otherwise, we expect a resource location (the advancement id)
            return CommandArgsParser.Whitespace
                .Then(ParserParts.IdentifierParser)
                .Bind(advancement =>
                {
                    // "only" mode can have an optional greedy string for the criterion
                    if (ctx.Mode == AdvancementMode.Only)
                    {
                        return Try(CommandArgsParser.Whitespace.Then(Any.ManyString()))
                            .Optional()
                            .Map(ICommand (criterionOpt) =>
                            {
                                var crit = criterionOpt.GetValueOrDefault().Trim();
                                return new AdvancementCommand(
                                    ctx.Action,
                                    ctx.Target,
                                    ctx.Mode,
                                    advancement,
                                    string.IsNullOrEmpty(crit) ? null : crit
                                );
                            });
                    }

                    // For "from", "through", and "until", we just return the advancement
                    return Return<ICommand>(new AdvancementCommand(ctx.Action, ctx.Target, ctx.Mode, advancement));
                });
        });
}