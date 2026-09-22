using BacapGenerator.Datapacks.Models;
using BacapGenerator.Datapacks.Models.Settings;
using BacapGenerator.Io;
using JetBrains.Annotations;

namespace BacapGenerator.Checklists;

/// <summary>
/// Service coordinating the synthesis and persistence of all configured checklist functions for a datapack.
/// </summary>
public static class ChecklistsService
{
    /// <summary>
    /// Generates and writes all checklist trigger callbacks and category verification functions
    /// defined in the datapack configuration.
    /// </summary>
    /// <param name="datapack">The target datapack model.</param>
    /// <returns>The total number of processed checklist definitions.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="datapack"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when attempting to generate files for a Reference (read-only) datapack.
    /// </exception>
    /// <example>
    /// <code>
    /// int generatedCount = ChecklistsService.GenerateAndSaveAll(addonDatapack);
    /// </code>
    /// </example>
    [PublicAPI]
    public static int GenerateAndSaveAll(Datapack datapack)
    {
        ArgumentNullException.ThrowIfNull(datapack);

        if (datapack.Settings.DatapackType == DatapackType.Reference)
        {
            throw new InvalidOperationException(
                $"Cannot generate checklists for datapack '{datapack.Id}' because its access mode is Reference (read-only).");
        }

        var checklists = datapack.Settings.Checklists;
        if (checklists is not { Count: > 0 })
            return 0;

        foreach (var checklist in checklists)
            ChecklistIoManager.GenerateAndSave(datapack, checklist);

        return checklists.Count;
    }
}