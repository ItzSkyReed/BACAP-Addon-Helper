using Core.Commands.Impl;
using Core.Commands.Models.Interfaces;
using Core.SNBT;
using Core.TextComponents.Components;
using JetBrains.Annotations;
using Pidgin;
using static Pidgin.Parser;
using static Pidgin.Parser<char>;

namespace Core.Commands.Parsers;

/// <summary>
/// Handles the parsing of the /scoreboard command for Java Edition.
/// </summary>
public static class ScoreboardCommandParser
{
    // Objective names allow characters: -, +, ., _, A-Z, a-z, and 0-9
    private static readonly Parser<char, string> ObjectiveName =
        Token(static c => char.IsLetterOrDigit(c) || c is '_' or '-' or '+' or '.')
            .AtLeastOnceString();

    private static readonly Parser<char, string> Identifier =
        Token(static c => char.IsLetterOrDigit(c) || c is '_' or '-' or '.' or ':')
            .AtLeastOnceString();

    #region Objectives Parsers

    private static readonly Parser<char, ICommand> ObjectivesList =
        String("list").Map(static ICommand (_) => new ScoreboardObjectivesListCommand());

    private static readonly Parser<char, ICommand> ObjectivesAdd =
        Map(static ICommand (_, obj, _, crit, dispNode) =>
            {
                var disp = dispNode.HasValue ? TextComponent.Parse(dispNode.Value) : null;
                return new ScoreboardObjectivesAddCommand(obj, crit, disp);
            },
            String("add").Then(CommandArgsParser.Whitespace),
            ObjectiveName,
            CommandArgsParser.Whitespace,
            Identifier,
            Try(CommandArgsParser.Whitespace.Then(SnbtParser.AnyNode)).Optional()
        );

    private static readonly Parser<char, ICommand> ObjectivesRemove =
        Map(static ICommand (_, obj) => new ScoreboardObjectivesRemoveCommand(obj),
            String("remove").Then(CommandArgsParser.Whitespace),
            ObjectiveName
        );

    private static readonly Parser<char, ICommand> ObjectivesSetDisplay =
        Map(static ICommand (_, slot, obj) => new ScoreboardObjectivesSetDisplayCommand(slot, obj.GetValueOrDefault()),
            String("setdisplay").Then(CommandArgsParser.Whitespace),
            Identifier,
            Try(CommandArgsParser.Whitespace.Then(ObjectiveName)).Optional()
        );

    private static readonly Parser<char, ICommand> ObjectivesModify =
        Map(static ICommand (_, obj, _, prop, _, val) => new ScoreboardObjectivesModifyCommand(obj, prop, val.Trim()),
            String("modify").Then(CommandArgsParser.Whitespace),
            ObjectiveName,
            CommandArgsParser.Whitespace,
            Identifier,
            CommandArgsParser.Whitespace,
            Any.ManyString()
        );

    private static readonly Parser<char, ICommand> ObjectivesParser =
        String("objectives").Then(CommandArgsParser.Whitespace)
            .Then(OneOf(
                ObjectivesList,
                ObjectivesAdd,
                ObjectivesRemove,
                ObjectivesSetDisplay,
                ObjectivesModify
            ));

    #endregion

    #region Players Parsers

    private static readonly Parser<char, ICommand> PlayersList =
        Map(static ICommand (_, target) => new ScoreboardPlayersListCommand(target.GetValueOrDefault()),
            String("list"),
            Try(CommandArgsParser.Whitespace.Then(CommandArgsParser.TargetSelector)).Optional()
        );

    private static readonly Parser<char, ICommand> PlayersGet =
        Map(static ICommand (_, target, _, obj) => new ScoreboardPlayersGetCommand(target, obj),
            String("get").Then(CommandArgsParser.Whitespace),
            CommandArgsParser.TargetSelector,
            CommandArgsParser.Whitespace,
            ObjectiveName
        );

    private static readonly Parser<char, ICommand> PlayersMath =
        Map(static ICommand (op, _, target, _, obj, _, score) => new ScoreboardPlayersMathCommand(op, target, obj, score),
            OneOf(
                String("set").Map(static _ => ScoreboardMathOperation.Set),
                String("add").Map(static _ => ScoreboardMathOperation.Add),
                String("remove").Map(static _ => ScoreboardMathOperation.Remove)
            ),
            CommandArgsParser.Whitespace,
            CommandArgsParser.TargetSelector,
            CommandArgsParser.Whitespace,
            ObjectiveName,
            CommandArgsParser.Whitespace,
            CommandArgsParser.Integer
        );

    private static readonly Parser<char, ICommand> PlayersReset =
        Map(static ICommand (_, target, obj) => new ScoreboardPlayersResetCommand(target, obj.GetValueOrDefault()),
            String("reset").Then(CommandArgsParser.Whitespace),
            CommandArgsParser.TargetSelector,
            Try(CommandArgsParser.Whitespace.Then(ObjectiveName)).Optional()
        );

    private static readonly Parser<char, ICommand> PlayersEnable =
        Map(static ICommand (_, target, _, obj) => new ScoreboardPlayersEnableCommand(target, obj),
            String("enable").Then(CommandArgsParser.Whitespace),
            CommandArgsParser.TargetSelector,
            CommandArgsParser.Whitespace,
            ObjectiveName
        );

    // Operator tokens ordered so longer prefixes match first (e.g., += before + if applicable)
    private static readonly Parser<char, string> OperationToken =
        OneOf(
            Try(String("+=")),
            Try(String("-=")),
            Try(String("*=")),
            Try(String("/=")),
            Try(String("%=")),
            Try(String("><")),
            String("="),
            String("<"),
            String(">")
        );

    private static readonly Parser<char, ICommand> PlayersOperation =
        Map(static ICommand (_, target, targetObj, op, src, srcObj) =>
                new ScoreboardPlayersOperationCommand(target, targetObj, op, src, srcObj),
            String("operation").Then(CommandArgsParser.Whitespace),
            CommandArgsParser.TargetSelector.Before(CommandArgsParser.Whitespace),
            ObjectiveName.Before(CommandArgsParser.Whitespace),
            OperationToken.Before(CommandArgsParser.Whitespace),
            CommandArgsParser.TargetSelector.Before(CommandArgsParser.Whitespace),
            ObjectiveName
        );

    private static readonly Parser<char, ICommand> PlayersDisplay =
        Map(static ICommand (_, dispType, _, target, _, obj, val) =>
                new ScoreboardPlayersDisplayCommand(target, obj, dispType, val.GetValueOrDefault()),
            String("display").Then(CommandArgsParser.Whitespace),
            OneOf(String("name"), String("numberformat")),
            CommandArgsParser.Whitespace,
            CommandArgsParser.TargetSelector,
            CommandArgsParser.Whitespace,
            ObjectiveName,
            Try(CommandArgsParser.Whitespace.Then(Any.ManyString().Map(static s => s.Trim()))).Optional()
        );

    private static readonly Parser<char, ICommand> PlayersParser =
        String("players").Then(CommandArgsParser.Whitespace)
            .Then(OneOf(
                PlayersList,
                PlayersGet,
                PlayersMath,
                PlayersReset,
                PlayersEnable,
                PlayersOperation,
                PlayersDisplay
            ));

    #endregion

    /// <summary>
    /// Parsed root /scoreboard command.
    /// </summary>
    [PublicAPI] public static readonly Parser<char, ICommand> Parser =
        OneOf(ObjectivesParser, PlayersParser);
}