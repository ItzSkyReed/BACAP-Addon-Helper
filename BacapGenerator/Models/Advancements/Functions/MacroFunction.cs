using JetBrains.Annotations;
using Core.Commands.Impl;
using Core.McFunctions.Models.Extensions;
using Core.SNBT;

namespace BacapGenerator.Models.Advancements.Functions;

/// <summary>
/// Represents the main function file that executes the core logic,
/// specifically ensuring that the reward macro command is present.
/// </summary>
public sealed class MacroFunction : BaseFunction
{
    public const string MacroCommandName = "bacaped_rewards:advancement_made_macro";
    private const string ExpectedMacroPrefix = $"function {MacroCommandName}";

    /// <summary>
    /// Initializes a new instance of the <see cref="MacroFunction"/> class.
    /// </summary>
    /// <param name="file">The physical file information.</param>
    /// <param name="bacapAdvancement">The BACAP advancement model associated with this function.</param>
    public MacroFunction(FileInfo file, BacapAdvancement bacapAdvancement)
        : base(file, bacapAdvancement)
    {
    }

    [PublicAPI]
    public override void Update()
    {

        var advId = BacapAdvancement.McPath;
        var rewardId = BacapAdvancement.Advancement.Rewards!.Function!;
        var tier = BacapAdvancement.Tier.ToString().ToLower();

        var functionArguments = Snbt.Compound()
            .Put("adv_id", advId)
            .Put("reward_id", rewardId)
            .Put("tier", tier)
            .Build();

        var newCommand = new FunctionCommand(MacroCommandName, functionArguments);

        var existingIndex = Function.Lines.FindIndex(line =>
            line.Build().TrimStart().StartsWith(ExpectedMacroPrefix, StringComparison.OrdinalIgnoreCase));

        if (existingIndex >= 0)
            Function.Lines[existingIndex] = newCommand.ToLine();
        else
            Function.Lines.Add(newCommand.ToLine());
    }

}