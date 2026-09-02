using BacapGenerator.Models.Advancements;
using BacapGenerator.Models.Advancements.Functions;
using BacapGenerator.Models.Advancements.Functions.Trophy;
using BacapGenerator.Models.Interfaces;
using BacapGenerator.Utils;
using Core.Advancements.Models;
using Core.McFunctions.Models;

namespace BacapGenerator.Factories;

/// <summary>
/// Factory responsible for reading files, parsing JSON/McFunction ASTs,
/// and instantiating the correct <see cref="ManagedAdvancement"/> subtype.
/// </summary>
public static class AdvancementFactory
{
    /// <summary>
    /// Creates a ManagedAdvancement from a raw JSON string.
    /// </summary>
    public static ManagedAdvancement Create(FileInfo file, string jsonContent, IReadOnlyDatapack datapack)
    {
        try
        {
            var parsedData = Advancement.Parse(jsonContent);
            if (parsedData == null)
                return new InvalidAdvancement(file, null, datapack, AdvancementValidationError.MalformedJson);

            return Create(file, parsedData, datapack);
        }
        catch
        {
            return new InvalidAdvancement(file, null, datapack, AdvancementValidationError.MalformedJson);
        }
    }

    /// <summary>
    /// Creates a ManagedAdvancement from an already parsed or modified Advancement model.
    /// </summary>
    public static ManagedAdvancement Create(FileInfo file, Advancement parsedData, IReadOnlyDatapack datapack)
    {
        if (parsedData.Display == null)
            return new TechnicalAdvancement(file, parsedData, datapack);

        if (!BacapAdvancement.TryCreate(file, parsedData, datapack, out var bacapAdv, out var error))
            return new InvalidAdvancement(file, parsedData, datapack, error);

        try
        {
            LoadFunctions(bacapAdv!);
            return bacapAdv!;
        }
        catch
        {
            return new InvalidAdvancement(file, parsedData, datapack, AdvancementValidationError.FailedToLoadAssociatedFunctions);
        }
    }

    /// <summary>
    /// Locates, reads, and parses the five function files associated with a BACAP advancement,
    /// injecting them directly into the model.
    /// </summary>
    private static void LoadFunctions(BacapAdvancement adv)
    {
        var relativePath = $"{MinecraftUtils.StripNamespace(adv.Advancement.Rewards!.Function!)}.mcfunction";
        var basePath = Path.Combine(adv.Datapack.DatapackDataPath.FullName, adv.Datapack.Settings.RewardNamespace, "function");

        var macroFile = new FileInfo(Path.Combine(basePath, relativePath));
        var msgFile = new FileInfo(Path.Combine(basePath, "msg", relativePath));
        var expFile = new FileInfo(Path.Combine(basePath, "exp", relativePath));
        var itemFile = new FileInfo(Path.Combine(basePath, "reward", relativePath));
        var trophyFile = new FileInfo(Path.Combine(basePath, "trophy", relativePath));

        adv.MacroFunction = new MacroFunction(macroFile, ParseMcFunction(macroFile), adv);
        adv.MsgFunction = new MsgFunction(msgFile, ParseMcFunction(msgFile), adv);
        adv.ExpRewardFunction = new ExpRewardFunction(expFile, ParseMcFunction(expFile), adv);
        adv.ItemRewardFunction = new ItemRewardFunction(itemFile, ParseMcFunction(itemFile), adv);
        adv.TrophyRewardFunction = new TrophyRewardFunction(trophyFile, ParseMcFunction(trophyFile), adv);
    }

    /// <summary>
    /// Reads the physical file if it exists and parses it into an AST.
    /// Returns an empty AST if the file is missing.
    /// </summary>
    private static McFunction ParseMcFunction(FileInfo file)
    {
        var content = file.Exists ? File.ReadAllText(file.FullName) : string.Empty;

        return McFunction.Parse(content);
    }
}