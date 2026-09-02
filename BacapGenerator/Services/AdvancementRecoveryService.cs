using System.Diagnostics.CodeAnalysis;
using BacapGenerator.Factories;
using BacapGenerator.Models.Advancements;
using BacapGenerator.Models.Datapacks;
using BacapGenerator.Utils;
using Core.Advancements.Models;
using JetBrains.Annotations;

namespace BacapGenerator.Services;

/// <summary>
/// Attempts to automatically fix and promote invalid advancements into valid ones.
/// </summary>
public static class AdvancementRecoveryService
{
    /// <summary>
    /// Scans a datapack for invalid advancements and attempts to recover and replace them in-place.
    /// </summary>
    /// <param name="datapack">The datapack to process.</param>
    /// <returns>A collection of successfully recovered advancements.</returns>
    [PublicAPI]
    public static IReadOnlyList<BacapAdvancement> RecoverAdvancements(Datapack datapack)
    {
        ArgumentNullException.ThrowIfNull(datapack);

        var invalidAdvancements = datapack.Advancements
            .OfType<InvalidAdvancement>()
            .ToList();

        var recovered = new List<BacapAdvancement>();

        foreach (var invalidAdv in invalidAdvancements)
        {
            if (!TryRecover(invalidAdv, out var recoveredAdv))
                continue;

            if (datapack.ReplaceAdvancement(invalidAdv, recoveredAdv))
                recovered.Add(recoveredAdv);
        }

        return recovered;
    }

    /// <summary>
    /// Tries to recover an invalid advancement based on its error type.
    /// </summary>
    /// <param name="invalidAdv">The invalid advancement containing the original data.</param>
    /// <param name="recoveredAdvancement">
    /// When this method returns, contains the promoted <see cref="BacapAdvancement"/> if recovery succeeded; otherwise, <see langword="null"/>.
    /// </param>
    /// <returns><see langword="true"/> if the advancement was successfully recovered and promoted; otherwise, <see langword="false"/>.</returns>
    [PublicAPI]
    public static bool TryRecover(
        InvalidAdvancement invalidAdv,
        [NotNullWhen(true)] out BacapAdvancement? recoveredAdvancement)
    {
        recoveredAdvancement = invalidAdv.ErrorReason switch
        {
            AdvancementValidationError.MissingRewardFunction => FixMissingRewardFunction(invalidAdv),
            _ => null
        };

        return recoveredAdvancement is not null;
    }

    private static BacapAdvancement? FixMissingRewardFunction(InvalidAdvancement invalidAdv)
    {
        var rawModel = invalidAdv.Advancement;
        if (rawModel is null) return null;

        if (!BacapUtils.TryExtractTab(invalidAdv.McPath, out var tab))
            return null;

        var fileNameWithoutExt = Path.GetFileNameWithoutExtension(invalidAdv.File.Name);
        var expectedFunctionPath = $"{invalidAdv.Datapack.Settings.RewardNamespace}:{tab}/{fileNameWithoutExt}";

        var fixedRewards = (rawModel.Rewards ?? new AdvancementRewards()) with
        {
            Function = expectedFunctionPath
        };

        var fixedModel = rawModel with
        {
            Rewards = fixedRewards
        };

        var recovered = AdvancementFactory.Create(invalidAdv.File, fixedModel, invalidAdv.Datapack);

        return recovered as BacapAdvancement;
    }
}