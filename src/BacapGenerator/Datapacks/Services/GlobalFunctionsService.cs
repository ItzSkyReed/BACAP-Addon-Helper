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
    /// Generates standard and custom datapack functions (scores, points, coop, trophies) and writes them to disk along with their tags.
    /// </summary>
    /// <param name="datapack">The target datapack to generate functions for.</param>
    /// <param name="allAdvancementsPool">
    /// An optional collection of all advancements across all loaded datapacks used to resolve multi-pack filters.
    /// When <see langword="null"/>, only advancements belonging to <paramref name="datapack"/> are evaluated.
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="datapack"/> is <see langword="null"/>.</exception>
    /// <example>
    /// <code>
    /// // Generate using only local advancements:
    /// GlobalFunctionsService.GenerateAndSaveAll(datapack);
    ///
    /// // Or with a cross-datapack pool:
    /// GlobalFunctionsService.GenerateAndSaveAll(datapack, allLoadedAdvancements);
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

        var fanpacksTags = Path.Combine(datapack.DatapackDataPath.FullName, "bacap_fanpacks", "tags", "function");
        var rewardFuncPath = Path.Combine(datapack.DatapackDataPath.FullName, settings.RewardNamespace, "function");

        var configTags = Path.Combine(fanpacksTags, "config");
        var configFuncPath = Path.Combine(datapack.DatapackDataPath.FullName, settings.MainNamespace, "function", "config");

        // Built-in standard functions
        Save(DatapackFunctionsGenerator.GenerateUpdateScore(scoreAdvancements),
            rewardFuncPath, "update_score", fanpacksTags, $"{settings.RewardNamespace}:update_score");

        Save(DatapackFunctionsGenerator.GenerateUpdatePoints(localAdvancements),
            rewardFuncPath, "update_points", fanpacksTags, $"{settings.RewardNamespace}:update_points");

        Save(DatapackFunctionsGenerator.GenerateUpdateCoop(localAdvancements),
            configFuncPath, "coop_update", configTags, $"{settings.MainNamespace}:config/coop_update");

        Save(DatapackFunctionsGenerator.GenerateGrantTrophies(trophyAdvancements),
            configFuncPath, "grant_trophies", configTags, $"{settings.MainNamespace}:config/grant_trophies");

        foreach (var team in BacapTeam.All)
        {
            var name = $"coop_update_team_{team.Color}";
            Save(DatapackFunctionsGenerator.GenerateUpdateCoopTeam(localAdvancements, team),
                configFuncPath, name, configTags, $"{settings.MainNamespace}:config/{name}");
        }

        // Custom score counters defined in datapack configuration
        foreach (var customScore in settings.CustomScores)
        {
            var functionName = GetNormalizedFunctionName(customScore.FilePath);
            var function = DatapackFunctionsGenerator.GenerateUpdateScore(advancementPool, customScore, datapack.Id);

            Save(function, rewardFuncPath, functionName, fanpacksTags, $"{settings.RewardNamespace}:{functionName}");
        }

        // Custom point counters defined in datapack configuration
        foreach (var customPoint in settings.CustomPoints)
        {
            var functionName = GetNormalizedFunctionName(customPoint.FilePath);
            var function = DatapackFunctionsGenerator.GenerateUpdatePoints(advancementPool, customPoint, datapack.Id);

            Save(function, rewardFuncPath, functionName, fanpacksTags, $"{settings.RewardNamespace}:{functionName}");
        }

        return;

        void Save(McFunction function, string funcDir, string fileName, string tagDir, string callPath)
        {
            var funcFile = new FileInfo(Path.Combine(funcDir, $"{fileName}.mcfunction"));
            var tagFile = new FileInfo(Path.Combine(tagDir, $"{fileName}.json"));

            DatapackIoManager.WriteFunctionAndTag(function, funcFile, tagFile, callPath);
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