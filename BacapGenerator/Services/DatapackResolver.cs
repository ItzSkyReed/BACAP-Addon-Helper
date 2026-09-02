using BacapGenerator.Models.Datapacks;
using JetBrains.Annotations;

namespace BacapGenerator.Services;

/// <summary>
/// Resolves override flags for advancements across loaded datapacks.
/// </summary>
public static class DatapackResolver
{
    /// <summary>
    /// Cross-references a child datapack with its parent to mark overriding advancements.
    /// New advancements unique to the child pack will remain marked as standard.
    /// </summary>
    /// <param name="childPack">The addon datapack (e.g., Bacaped Hardcore).</param>
    /// <param name="parentPack">The base datapack (e.g., Bacaped).</param>
    /// <exception cref="ArgumentNullException">Thrown when either pack is null.</exception>
    [PublicAPI]
    public static void ResolveOverrides(Datapack childPack, Datapack parentPack)
    {
        ArgumentNullException.ThrowIfNull(childPack);
        ArgumentNullException.ThrowIfNull(parentPack);

        var parentAdvancementIds = parentPack.Advancements
            .Select(a => a.McPath)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        // Mark advancements that already exist in the parent
        foreach (var adv in childPack.Advancements)
        {
            if (parentAdvancementIds.Contains(adv.McPath))
                adv.IsOverride = true;
        }
    }
}