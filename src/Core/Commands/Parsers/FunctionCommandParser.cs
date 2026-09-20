using Core.Commands.Impl;
using Core.Commands.Models.Interfaces;
using Core.SNBT;
using Core.SNBT.Nodes;
using JetBrains.Annotations;
using Pidgin;
using static Pidgin.Parser;
using static Pidgin.Parser<char>;

namespace Core.Commands.Parsers;

/// <summary>
/// Handles the parsing of the /function command.
/// </summary>
public static class FunctionCommandParser
{
    private static readonly Parser<char, string> IdentifierParser =
        Token(c => char.IsLetterOrDigit(c) || c == '_' || c == '-' || c == '.' || c == ':')
            .AtLeastOnceString();

    private static readonly Parser<char, string> NameParser =
        Map((hash, id) => (hash.HasValue ? "#" : "") + id,
            Try(Char('#')).Optional(),
            IdentifierParser);

    private static readonly Parser<char, FunctionWithSourceType> WithSourceTypeParser =
        OneOf(
            String("block").Map(_ => FunctionWithSourceType.Block),
            String("entity").Map(_ => FunctionWithSourceType.Entity),
            String("storage").Map(_ => FunctionWithSourceType.Storage)
        );

    private static Parser<char, string> GetSourceParser(FunctionWithSourceType type)
    {
        return type switch
        {
            FunctionWithSourceType.Block =>
                CommandArgsParser.PositionParser.Map(pos => pos.ToString()),
            FunctionWithSourceType.Entity =>
                CommandArgsParser.TargetSelector.Map(sel => sel.ToString()),
            FunctionWithSourceType.Storage =>
                IdentifierParser,
            _ => throw new ArgumentOutOfRangeException(nameof(type))
        };
    }


    /// <summary>
    /// Parses an SNBT node and ensures it is specifically a Compound tag.
    /// </summary>
    private static readonly Parser<char, SnbtCompound> SnbtCompoundArgsParser =
        SnbtParser.AnyNode
            .Where(node => node is SnbtCompound)
            .Map(node => (SnbtCompound)node);

    [PublicAPI]
    public static readonly Parser<char, ICommand> Parser =
        NameParser.Bind(name =>
        {
            // 'with <type> <source> [<path>]'
            var withParser = Try(String("with").Then(CommandArgsParser.Whitespace))
                .Then(WithSourceTypeParser)
                .Bind(type =>
                    CommandArgsParser.Whitespace
                    .Then(GetSourceParser(type))
                    .Bind(source =>
                        Try(CommandArgsParser.Whitespace.Then(Any.ManyString()))
                        .Optional()
                        .Map(ICommand (pathOpt) =>
                        {
                            var path = pathOpt.GetValueOrDefault().Trim();
                            return new FunctionCommand(
                                CommandName: name,
                                WithSourceType: type,
                                WithSource: source,
                                WithPath: string.IsNullOrEmpty(path) ? null : path
                            );
                        })
                    )
                );

            var snbtParser = SnbtCompoundArgsParser
                .Map(ICommand (compound) => new FunctionCommand(
                    CommandName: name,
                    Arguments: compound
                ));

            var optionalArgsParser = Try(CommandArgsParser.Whitespace.Then(
                OneOf(withParser, snbtParser)
            ));

            return optionalArgsParser.Or(Return<ICommand>(new FunctionCommand(CommandName: name)));
        });
}