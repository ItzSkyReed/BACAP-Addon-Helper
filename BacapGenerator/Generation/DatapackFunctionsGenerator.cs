using BacapGenerator.Advancements.Models;
using BacapGenerator.Common;
using BacapGenerator.Datapacks.Models;
using BacapGenerator.Utils;
using Core.Commands.Impl;
using Core.Commands.Models;
using Core.Commands.Models.Interfaces;
using Core.McFunctions.Models;
using Core.McFunctions.Models.Interfaces;

namespace BacapGenerator.Generation;

public static class DatapackFunctionsGenerator
{
    /// <summary>
    /// Generates a function that increments the player's advancement score counter for each completed advancement.
    /// </summary>
    /// <param name="advancements">The list of advancements to generate score update commands for.</param>
    /// <returns>A configured <see cref="McFunction"/> containing score update commands.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="advancements"/> is <see langword="null"/>.</exception>
    public static McFunction GenerateUpdateScore(IReadOnlyList<BacapAdvancement> advancements)
    {
        ArgumentNullException.ThrowIfNull(advancements);

        var scoreboardCommand = new ScoreboardPlayersMathCommand(
            ScoreboardMathOperation.Add,
            Selector.SelectedPlayer,
            DatapackDefaults.AdvancementsScoreboard,
            1);

        return BuildFunction(advancements, advancement =>
            CreateAdvancementExecuteCommand("@a", advancement.McPath, scoreboardCommand));
    }

    /// <summary>
    /// Generates a function that adds points to the player based on each completed advancement's tier.
    /// </summary>
    /// <param name="advancements">The list of advancements to calculate points for.</param>
    /// <returns>A configured <see cref="McFunction"/> containing points update commands.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="advancements"/> is <see langword="null"/>.</exception>
    public static McFunction GenerateUpdatePoints(IReadOnlyList<BacapAdvancement> advancements)
    {
        ArgumentNullException.ThrowIfNull(advancements);

        return BuildFunction(advancements, advancement =>
        {
            var scoreboardCommand = new ScoreboardPlayersOperationCommand(
                Selector.SelectedPlayer,
                DatapackDefaults.PointsScoreboard,
                "+=",
                Selector.Custom(advancement.Tier.TechnicalName()),
                "bac_points");

            return CreateAdvancementExecuteCommand("@a", advancement.McPath, scoreboardCommand);
        });
    }

    /// <summary>
    /// Generates a function that grants coop advancements to all players when score conditions are met.
    /// </summary>
    /// <param name="advancements">The list of advancements to synchronize across all players.</param>
    /// <returns>A configured <see cref="McFunction"/> containing coop grant commands.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="advancements"/> is <see langword="null"/>.</exception>
    public static McFunction GenerateUpdateCoop(IReadOnlyList<BacapAdvancement> advancements) =>
        GenerateCoopInternal(advancements, Selector.AllPlayers, DatapackDefaults.CoopBaseScoreboard);

    /// <summary>
    /// Generates a function that grants coop advancements specifically to members of the given team.
    /// </summary>
    /// <param name="advancements">The list of advancements to synchronize.</param>
    /// <param name="team">The target team whose members should receive advancements.</param>
    /// <returns>A configured <see cref="McFunction"/> containing team coop grant commands.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="advancements"/> or <paramref name="team"/> is <see langword="null"/>.</exception>
    public static McFunction GenerateUpdateCoopTeam(IReadOnlyList<BacapAdvancement> advancements, BacapTeam team)
    {
        ArgumentNullException.ThrowIfNull(team);
        return GenerateCoopInternal(advancements, Selector.Custom($"@a[team=bac_team{team.Color}]"), $"{DatapackDefaults.CoopBaseScoreboard}_{team.Color}");
    }

    /// <summary>
    /// Generates a function that awards reward trophies to players who unlocked the corresponding advancements.
    /// </summary>
    /// <param name="advancements">The list of advancements mapped to trophy rewards.</param>
    /// <returns>A configured <see cref="McFunction"/> containing trophy grant commands.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="advancements"/> is <see langword="null"/>.</exception>
    public static McFunction GenerateGrantTrophies(IReadOnlyList<BacapAdvancement> advancements)
    {
        ArgumentNullException.ThrowIfNull(advancements);

        return BuildFunction(advancements, advancement =>
        {
            var functionCommand = new FunctionCommand(
                $"[{advancement.Datapack.Settings.RewardNamespace}]:trophy/[{MinecraftUtils.StripNamespace(advancement.McPath)}]");

            return CreateAdvancementExecuteCommand("@s", advancement.McPath, functionCommand);
        });
    }

    /// <summary>
    /// Core generator for coop advancement synchronization lines targeting a specific player selector and scoreboard objective.
    /// </summary>
    /// <param name="advancements">The advancements to evaluate.</param>
    /// <param name="targetSelector">The selector for players who should receive the advancement.</param>
    /// <param name="scoreboardObjective">The scoreboard objective checked for advancement completion.</param>
    /// <returns>A configured <see cref="McFunction"/> with conditional advancement grant commands.</returns>
    private static McFunction GenerateCoopInternal(
        IReadOnlyList<BacapAdvancement> advancements,
        Selector targetSelector,
        string scoreboardObjective)
    {
        ArgumentNullException.ThrowIfNull(advancements);

        return BuildFunction(advancements, advancement =>
        {
            var advancementCommand = new AdvancementCommand(
                AdvancementAction.Grant,
                targetSelector,
                AdvancementMode.Only,
                advancement.McPath);

            return new ExecuteCommand(
                $"if score {advancement.McPath} {scoreboardObjective} matches 1..",
                advancementCommand);
        });
    }

    /// <summary>
    /// Wraps a command inside an execute selector filtered by advancement completion status.
    /// </summary>
    /// <param name="targetSelector">The selector target (e.g. <c>@a</c> or <c>@s</c>).</param>
    /// <param name="mcPath">The Minecraft resource location path of the advancement.</param>
    /// <param name="subcommand">The command to execute if the condition matches.</param>
    /// <returns>An <see cref="ExecuteCommand"/> instance targeting players with the specified advancement.</returns>
    private static ExecuteCommand CreateAdvancementExecuteCommand(string targetSelector, string mcPath, ICommand subcommand) =>
        new($"as {targetSelector}[advancements={{[{mcPath}]=true}}]", subcommand);

    /// <summary>
    /// Pre-allocates lines and populates a new <see cref="McFunction"/> using the provided line-command factory.
    /// </summary>
    /// <param name="advancements">The source advancement collection.</param>
    /// <param name="commandFactory">The factory mapping an advancement to its corresponding <see cref="ICommand"/>.</param>
    /// <returns>A populated <see cref="McFunction"/>.</returns>
    private static McFunction BuildFunction(
        IReadOnlyList<BacapAdvancement> advancements,
        Func<BacapAdvancement, ICommand> commandFactory)
    {
        var lines = new List<IMcFunctionLine>(advancements.Count);
        lines.AddRange(advancements.Select(advancement => new ExecutableLine(commandFactory(advancement))));

        return new McFunction(lines);
    }
}