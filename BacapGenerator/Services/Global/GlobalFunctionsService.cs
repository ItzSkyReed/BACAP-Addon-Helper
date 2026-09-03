using BacapGenerator.Models;
using BacapGenerator.Models.Advancements;
using BacapGenerator.Models.Datapacks;
using BacapGenerator.Services.Generators;
using BacapGenerator.Services.IO;
using BacapGenerator.Services.Selectors;
using Core.McFunctions.Models;

namespace BacapGenerator.Services.Global;

/// <summary>
/// Orchestrates the generation and saving of global datapack functions and their tags.
/// </summary>
public static class GlobalFunctionsService
{
    public static void GenerateAndSaveAll(Datapack datapack)
    {
        var allAdvancements = datapack.Advancements.OfType<BacapAdvancement>().ToList();
        var scoreAdvancements = allAdvancements.GetForUpdateScore().ToList();
        var trophyAdvancements = allAdvancements.GetWithTrophies().ToList();

        var settings = datapack.Settings;

        var fanpacksTags = Path.Combine(settings.DatapackPath, "data", "bacap_fanpacks", "tags", "function");
        var rewardFuncPath = Path.Combine(settings.DatapackPath, "data", settings.RewardNamespace, "function");

        var configTags = Path.Combine(fanpacksTags, "config");
        var configFuncPath = Path.Combine(settings.DatapackPath, "data", settings.MainNamespace, "function", "config");

        Save(DatapackFunctionsGenerator.GenerateUpdateScore(scoreAdvancements),
            rewardFuncPath, "update_score", fanpacksTags, $"{settings.RewardNamespace}:update_score");

        Save(DatapackFunctionsGenerator.GenerateUpdatePoints(allAdvancements),
            rewardFuncPath, "update_points", fanpacksTags, $"{settings.RewardNamespace}:update_points");

        Save(DatapackFunctionsGenerator.GenerateUpdateCoop(allAdvancements),
            configFuncPath, "coop_update", configTags, $"{settings.MainNamespace}:config/coop_update");

        Save(DatapackFunctionsGenerator.GenerateGrantTrophies(trophyAdvancements),
            configFuncPath, "grant_trophies", configTags, $"{settings.MainNamespace}:config/grant_trophies");

        foreach (var team in BacapTeam.All)
        {
            var name = $"coop_update_team_{team.Color}";
            Save(DatapackFunctionsGenerator.GenerateUpdateCoopTeam(allAdvancements, team),
                configFuncPath, name, configTags, $"{settings.MainNamespace}:config/{name}");
        }

        return;

        void Save(McFunction function, string funcDir, string fileName, string tagDir, string callPath)
        {
            var funcFile = new FileInfo(Path.Combine(funcDir, $"{fileName}.mcfunction"));
            var tagFile = new FileInfo(Path.Combine(tagDir, $"{fileName}.json"));

            DatapackIoManager.WriteFunctionAndTag(function, funcFile, tagFile, callPath);
        }
    }
}