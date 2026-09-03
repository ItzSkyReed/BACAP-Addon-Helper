    using System.Diagnostics.CodeAnalysis;
    using Pidgin;
    using Core.Commands.Impl;
    using Core.Commands.Models.Interfaces;
    using Core.Common;
    using Core.Items;
    using Core.SNBT;
    using Core.SNBT.Nodes;
    using Core.TextComponents.Components;
    using JetBrains.Annotations;

    namespace Core.Commands.Parsers;

    /// <summary>
    /// An extensible command parser registry.
    /// Routes string inputs to specific command implementations based on the first word.
    /// </summary>
    public static class CommandParser
    {
        // A registry holding parsers for specific command literals.
        private static readonly Dictionary<string, Parser<char, ICommand>> Registry = new(StringComparer.OrdinalIgnoreCase);

        static CommandParser()
        {
            // /give parser
            Register("give", Parser.Map(ICommand (target, _, item, countOpt) =>
                {
                    int? parsedCount = countOpt.HasValue ? countOpt.Value : null;
                    return new GiveCommand(target, item, parsedCount);
                },
                CommandArgsParser.TargetSelector,
                CommandArgsParser.Whitespace,
                ItemStackParser.Item,
                Parser.Try(CommandArgsParser.Whitespace.Then(CommandArgsParser.Integer)).Optional()
            ));

            // /tellraw parser
            Register("tellraw", Parser.Map(ICommand (target, _, messageNode) =>
                {
                    var component = TextComponent.Parse(messageNode);
                    return new TellrawCommand(target, component);
                },
                CommandArgsParser.TargetSelector,
                CommandArgsParser.Whitespace,
                SnbtParser.AnyNode
            ));

            // /say parser
            // Reads all remaining characters to the end of the string and maps them to a SayCommand.
            Register("say", Parser<char>.Any.ManyString().Select(ICommand (message) =>
                new SayCommand(message.Trim())
            ));

            // /summon parser
            Register("summon", Parser.Map(ICommand (entity, _, pos, nbtOpt) =>
                    new SummonCommand(entity, pos, nbtOpt.GetValueOrDefault()),
                ParserParts.IdentifierParser,
                CommandArgsParser.Whitespace,
                CommandArgsParser.PositionParser,
                Parser.Try(CommandArgsParser.Whitespace.Then(SnbtParser.AnyNode.Cast<SnbtCompound>())).Optional()
            ));

            // /execute parser
            Register("execute", ExecuteCommandParser.Parser);

            // /advancement parser
            Register("advancement", AdvancementCommandParser.Parser);

            // /experience parser
            Register("experience", ExperienceCommandParser.Parser);

            // /xp alias
            Register("xp", ExperienceCommandParser.Parser);

            // /function
            Register("function", FunctionCommandParser.Parser);

            // /scoreboard parser
            Register("scoreboard", ScoreboardCommandParser.Parser);
        }

        /// <summary>
        /// Registers a custom parser for a specific command keyword.
        /// </summary>
        /// <param name="commandName">The literal command name (e.g., "tp", "kill").</param>
        /// <param name="parser">The parser that evaluates the rest of the command string.</param>
        [PublicAPI]
        public static void Register(string commandName, Parser<char, ICommand> parser)
        {
            Registry[commandName] = parser;
        }

        /// <summary>
        /// The root parser that dynamically routes execution to the correct registered parser.
        /// </summary>
        public static readonly Parser<char, ICommand> Root =
            ParserParts.IdentifierParser.Bind<ICommand>(commandName =>
            {
                if (Registry.TryGetValue(commandName, out var commandArgsParser))
                    return CommandArgsParser.Whitespace.Then(commandArgsParser);

                return Parser<char>.Fail<ICommand>($"Unknown or unregistered command: {commandName}");
            });

        /// <summary>
        /// Parses a raw Minecraft command string into an executable <see cref="ICommand"/> object.
        /// </summary>
        [PublicAPI]
        public static ICommand Parse(string input)
        {
            if (input.StartsWith('/'))
                input = input[1..];

            return Root.ParseOrThrow(input);
        }

        /// <summary>
        /// Attempts to safely parse a Minecraft command string into an <see cref="ICommand"/> object.
        /// </summary>
        [PublicAPI]
        public static bool TryParse(string input, [NotNullWhen(true)] out ICommand? command)
        {
            if (input.StartsWith('/'))
                input = input[1..];

            var result = Root.Parse(input);

            if (result.Success)
            {
                command = result.Value;
                return true;
            }


            // Console.WriteLine($"[yellow]Pidgin Error:[/] {result.Error}\n");
            // Console.WriteLine(input);

            command = null;
            return false;
        }
    }