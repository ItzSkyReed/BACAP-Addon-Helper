
using BacapGenerator.Models;
using BacapGenerator.Models.Advancements;
using BacapGenerator.Models.Datapacks;
using BacapGenerator.Utils;
using Core.Advancements.Models;

namespace BacapGenerator.Factories;

/// <summary>
/// Factory responsible for parsing JSON files and instantiating the correct ManagedAdvancement subtype.
/// </summary>
public static class AdvancementFactory
{
    /// <summary>
    /// Parses an advancement JSON file and creates the appropriate <see cref="ManagedAdvancement"/> instance.
    /// </summary>
    /// <param name="file">The physical file information.</param>
    /// <param name="jsonContent">The raw JSON string content of the file.</param>
    /// <returns>A <see cref="BacapAdvancement"/>, <see cref="TechnicalAdvancement"/>, or <see cref="InvalidAdvancement"/>.</returns>
    /// <example>
    /// <code>
    /// var adv = AdvancementFactory.Create(new FileInfo("path/to/adv.json"), "{...}");
    /// </code>
    /// </example>
    public static ManagedAdvancement Create(FileInfo file, string jsonContent, Datapack datapack)
    {
        Advancement? parsedData;

        try
        {
            // Try to parse JSON using the Core model
            parsedData = Advancement.Parse(jsonContent);

            if (parsedData == null)
                return new InvalidAdvancement(file, null, "JSON parsed to null.");
        }
        catch (Exception ex)
        {
            return new InvalidAdvancement(file, null, $"JSON Parsing error: {ex.Message}");
        }


        // Check if it lacks Display completely (common for technical advancements/triggers)
        if (parsedData.Display == null)
            return new TechnicalAdvancement(file, parsedData);

        // Ask BacapAdvancement if it can be created
        if (BacapAdvancement.TryCreate(file, parsedData, datapack,  out var bacapAdv, out var errorMessage))
            return bacapAdv!;

        // If not, return invalid
        return new InvalidAdvancement(file, parsedData, errorMessage!);
    }
}