using BacapGenerator.Advancements.Filtering;
using BacapGenerator.Advancements.Models;
using BacapGenerator.Common;
using BacapGenerator.Datapacks.Models;
using BacapGenerator.Generation;
using BacapGenerator.Io;
using Core.McFunctions.Models;

namespace BacapGenerator.Datapacks.Services;

/// <summary>
/// Orchestrates the generation and saving of global datapack functions, custom counters, and their tags.
/// </summary>
public static class GlobalFunctionsService
{
    /// <summary>
    /// Generates standard and custom datapack functions (scores, points, coop, trophies) and writes them to disk,
    /// grouping score and point functions into their respective fanpack tags.
    /// </summary>
    /// <param name="datapack">The target datapack to generate functions for.</param>
    /// <param name="allAdvancementsPool">
    /// An optional collection of all advancements across all loaded datapacks used to resolve multi-pack filters.
    /// When <see langword="null"/>, only advancements belonging to <paramref name="datapack"/> are evaluated.
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="datapack"/> is <see langword="null"/>.</exception>
    /// <example>
    /// <code>
    /// GlobalFunctionsService.GenerateAndSaveAll(datapack, allAdvancements);
    /// </code>
    /// </example>
    public static void GenerateAndSaveAll(Datapack datapack, IReadOnlyList<BacapAdvancement>? allAdvancementsPool = null)
    {
        ArgumentNullException.ThrowIfNull(datapack);

        var localAdvancements = datapack.Advancements.OfType<BacapAdvancement>().ToList();
        var advancementPool = allAdvancementsPool ?? localAdvancements;

        var scoreAdvancements = localAdvancements.GetForUpdateScore().ToList();
        var trophyAdvancements = localAdvancements.GetWithTrophies().ToList();

        var settings = datapack.Settings;

        var fanpacksTags = Path.Combine(datapack.DatapackDataPath.FullName, DatapackDefaults.BacapFanpacksNamespace, "tags", "function");
        var rewardFuncPath = Path.Combine(datapack.DatapackDataPath.FullName, settings.RewardNamespace, "function");

        var configTags = Path.Combine(fanpacksTags, "config");
        var configFuncPath = Path.Combine(datapack.DatapackDataPath.FullName, settings.MainNamespace, "function", "config");

        // Process Scores (Base + Custom) -> All merged into DatapackDefaults.UpdateScoreFileName tag
        var scoreCallPaths = new List<string> { $"{settings.RewardNamespace}:{DatapackDefaults.UpdateScoreFileName}" };

        SaveFunction(
            DatapackFunctionsGenerator.GenerateUpdateScore(scoreAdvancements),
            rewardFuncPath,
            DatapackDefaults.UpdateScoreFileName);

        foreach (var customScore in settings.CustomScores)
        {
            var functionName = GetNormalizedFunctionName(customScore.FilePath);
            var function = DatapackFunctionsGenerator.GenerateUpdateScore(advancementPool, customScore, datapack.Id);

            SaveFunction(function, rewardFuncPath, functionName);
            scoreCallPaths.Add($"{settings.RewardNamespace}:{functionName}");
        }

        SaveTag(fanpacksTags, DatapackDefaults.UpdateScoreFileName, scoreCallPaths);

        // Process Points (Base + Custom) -> All merged into DatapackDefaults.UpdatePointsFileName tag
        var pointsCallPaths = new List<string> { $"{settings.RewardNamespace}:{DatapackDefaults.UpdatePointsFileName}" };

        SaveFunction(
            DatapackFunctionsGenerator.GenerateUpdatePoints(localAdvancements),
            rewardFuncPath,
            DatapackDefaults.UpdatePointsFileName);

        foreach (var customPoint in settings.CustomPoints)
        {
            var functionName = GetNormalizedFunctionName(customPoint.FilePath);
            var function = DatapackFunctionsGenerator.GenerateUpdatePoints(advancementPool, customPoint, datapack.Id);

            SaveFunction(function, rewardFuncPath, functionName);
            pointsCallPaths.Add($"{settings.RewardNamespace}:{functionName}");
        }

        SaveTag(fanpacksTags, DatapackDefaults.UpdatePointsFileName, pointsCallPaths);

        // Process Coop and Trophy functions (1-to-1 function and tag)
        SaveFunctionAndTag(
            DatapackFunctionsGenerator.GenerateUpdateCoop(localAdvancements),
            configFuncPath,
            DatapackDefaults.CoopUpdateFileName,
            configTags,
            $"{settings.MainNamespace}:config/{DatapackDefaults.CoopUpdateFileName}");

        SaveFunctionAndTag(
            DatapackFunctionsGenerator.GenerateGrantTrophies(trophyAdvancements),
            configFuncPath,
            DatapackDefaults.GrandTrophiesFileName,
            configTags,
            $"{settings.MainNamespace}:config/{DatapackDefaults.GrandTrophiesFileName}");

        foreach (var team in BacapTeam.All)
        {
            var name = $"{DatapackDefaults.CoopTeamUpdateFileName}_{team.Color}";
            SaveFunctionAndTag(
                DatapackFunctionsGenerator.GenerateUpdateCoopTeam(localAdvancements, team),
                configFuncPath,
                name,
                configTags,
                $"{settings.MainNamespace}:config/{name}");
        }

        return;

        void SaveFunction(McFunction function, string funcDir, string fileName)
        {
            var funcFile = new FileInfo(Path.Combine(funcDir, $"{fileName}.mcfunction"));
            DatapackIoManager.WriteFunction(function, funcFile);
        }

        void SaveTag(string tagDir, string tagName, IReadOnlyList<string> callPaths)
        {
            var tagFile = new FileInfo(Path.Combine(tagDir, $"{tagName}.json"));
            DatapackIoManager.WriteTag(tagFile, callPaths);
        }

        void SaveFunctionAndTag(McFunction function, string funcDir, string fileName, string tagDir, string callPath)
        {
            SaveFunction(function, funcDir, fileName);
            SaveTag(tagDir, fileName, [callPath]);
        }

        static string GetNormalizedFunctionName(string filePath)
        {
            var normalized = filePath.Replace('\\', '/').TrimStart('/');
            return normalized.EndsWith(".mcfunction", StringComparison.OrdinalIgnoreCase)
                ? normalized[..^".mcfunction".Length]
                : normalized;
        }
    }
}