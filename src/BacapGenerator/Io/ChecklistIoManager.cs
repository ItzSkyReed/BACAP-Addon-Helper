using System.Text;
using BacapGenerator.Datapacks.Models;
using BacapGenerator.Datapacks.Models.Settings.Checklists;
using BacapGenerator.Generation;
using JetBrains.Annotations;

namespace BacapGenerator.Io;

/// <summary>
/// Service responsible for orchestrating file I/O operations for generated checklist functions.
/// </summary>
public static class ChecklistIoManager
{
    private static readonly UTF8Encoding Utf8NoBom = new(encoderShouldEmitUTF8Identifier: false);

    /// <summary>
    /// Generates and persists all files (trigger callback and category checks) defined in the checklist.
    /// </summary>
    /// <param name="datapack">The target datapack model where functions will be written.</param>
    /// <param name="definition">The checklist configuration settings.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="datapack"/> or <paramref name="definition"/> is <see langword="null"/>.</exception>
    /// <example>
    /// <code>
    /// ChecklistIoManager.GenerateAndSave(datapack, checklistDef);
    /// </code>
    /// </example>
    [PublicAPI]
    public static void GenerateAndSave(Datapack datapack, ChecklistDefinitionSettings definition)
    {
        ArgumentNullException.ThrowIfNull(datapack);
        ArgumentNullException.ThrowIfNull(definition);

        var targetNamespace = !string.IsNullOrWhiteSpace(definition.TargetNamespace)
            ? definition.TargetNamespace
            : datapack.Settings.MainNamespace;

        var functionsRoot = Path.Combine(datapack.DatapackDataPath.FullName, targetNamespace, "function");

        // Generate and save trigger callback: data/{namespace}/function/{checklist_triggers_folder}/{filename}
        var triggerDirectory = Path.Combine(functionsRoot, datapack.Settings.ChecklistTriggersFolder);
        Directory.CreateDirectory(triggerDirectory);

        var triggerCallbackPath = Path.Combine(triggerDirectory, definition.TriggerFilename);
        var triggerFunction = ChecklistGenerator.GenerateTriggerCallback(definition);
        File.WriteAllText(triggerCallbackPath, triggerFunction.Build(), Utf8NoBom);

        // Generate and save category advancement checks: data/{namespace}/function/{subfolder}/{category}_check.mcfunction
        var checkDirectory = string.IsNullOrWhiteSpace(definition.CheckSubfolder)
            ? functionsRoot
            : Path.Combine(functionsRoot, definition.CheckSubfolder);

        Directory.CreateDirectory(checkDirectory);

        foreach (var category in definition.Categories)
        {
            if (string.IsNullOrWhiteSpace(category.Advancement))
                continue;

            var checkFunctionPath = Path.Combine(checkDirectory, $"{category.Name}_check.mcfunction");
            var checkFunction = ChecklistGenerator.GenerateCategoryCheck(definition, category);
            File.WriteAllText(checkFunctionPath, checkFunction.Build(), Utf8NoBom);
        }
    }
}