using Core.Commands.Impl;
using Core.Commands.Models.Interfaces;
using Core.DataComponents.Components;
using Core.McFunctions;
using Core.McFunctions.Models;
using Core.TextComponents.Components;

namespace UI.Examples;

public static class ParserIntegrationDemo
{
    /// <summary>
    /// Demonstrates the full parsing pipeline: McFunction -> Commands -> TextComponents/SNBT/Items.
    /// Analyzes a complex raw function string and extracts deeply nested components.
    /// </summary>
    /// <param name="rawFunctionText">The raw content of an .mcfunction file.</param>
    /// <returns>Displays the extracted components and command details to the console.</returns>
    /// <exception cref="ArgumentException">Thrown if the input text is null or whitespace.</exception>
    /// <example>
    /// <code>
    /// string fileText = "$tellraw @a {\"text\":\"Hi!\"}";
    /// ParserIntegrationDemo.RunDemo(fileText);
    /// </code>
    /// </example>
    public static void RunDemo(string rawFunctionText)
    {
        if (string.IsNullOrWhiteSpace(rawFunctionText))
            throw new ArgumentException("Input function text cannot be null or empty.", nameof(rawFunctionText));

        Console.WriteLine("=== Starting Full Pipeline Parsing ===");

        // 1. Preprocessor & Function Parser
        // Resolves line continuations '\', strips comments, and builds logical lines
        var mcFunction = McFunctionParser.Parse(rawFunctionText);

        Console.WriteLine($"Successfully parsed {mcFunction.Lines.Count} logical lines.\n");

        // 2. Iterate through the parsed lines
        foreach (var line in mcFunction.Lines)
        {
            switch (line)
            {
                case CommentLine comment:
                    Console.WriteLine($"Comment Line: {comment.Text}");
                    break;

                // 3. Fallback for unregistered commands (e.g., /scoreboard)
                case RawCommandLine rawCmd:
                    Console.WriteLine($"Raw Command (IsMacro: {rawCmd.IsMacro}): {rawCmd.RawText}");
                    break;

                // 4. Fully strongly-typed command!
                case ExecutableLine { Command: var typedCommand }:
                    AnalyzeCommand(typedCommand);
                    break;
            }
        }
    }

    /// <summary>
    /// Analyzes a strongly-typed command and extracts specific SNBT or Text Component data.
    /// </summary>
    /// <param name="command">The parsed command instance.</param>
    private static void AnalyzeCommand(ICommand command)
    {
        Console.WriteLine($"\nType: {command.GetType().Name}, IsMacro: {command.IsMacro}");

        // C# Pattern Matching magic
        switch (command)
        {
            case TellrawCommand tellraw:
                Console.WriteLine($"  Target: {tellraw.Target}");

                // Extracting text from our TextComponent models
                if (tellraw.Message is PlainTextComponent plainText)
                {
                    Console.WriteLine($"  Message Text: '{plainText.Text}'");
                    Console.WriteLine($"  Color: {plainText.Style?.Color ?? "Default"}");
                    Console.WriteLine($"  Is Bold: {plainText.Style?.Bold ?? false}");
                }
                break;

            case ExecuteCommand execute:
                Console.WriteLine($"  Execute: {execute.RunCommand}");
                Console.WriteLine(execute.RawModifiers);

                // Extracting text from our TextComponent models
                if (execute.RunCommand != null)
                {
                    Console.WriteLine($"  Sub command: '{execute.RunCommand.Build()}'");
                }
                break;

            case GiveCommand give:
                Console.WriteLine($"  Target: {give.Target}");
                Console.WriteLine($"  Item ID: {give.Item.Id}");
                Console.WriteLine($"  Count: {give.Item.Count}");

                // Digging into the ItemStack components (SNBT AST)
                if (give.Item.Components.IsEmpty)
                {
                    Console.WriteLine("  No item components found.");
                    break;
                }

                // E.g., Extracting the custom_name text component from the item!
                var customNameNode = give.Item.Components.Get<CustomNameComponent>();
                if (customNameNode != null)
                {
                    // custom_name is stored as an SNBT string containing JSON.
                    // Our parser handles this beautifully.
                    Console.WriteLine($"  Extracted Custom Name SNBT: {customNameNode.ToSnbt()}");
                }

                // Extracting a numeric value (e.g. damage)
                var damageNode = give.Item.Components.Get<DamageComponent>();
                if (damageNode != null)
                {
                    Console.WriteLine($"  Extracted Damage Value: {damageNode.ToSnbt()}");
                }
                break;
        }
    }
}