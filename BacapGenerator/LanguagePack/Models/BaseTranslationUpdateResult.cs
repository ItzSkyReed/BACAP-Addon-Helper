using BacapGenerator.Datapacks.Models;

namespace BacapGenerator.LanguagePack.Models;

/// <summary>
/// Result entry for a base translation generation run for a single parent datapack.
/// </summary>
/// <param name="Datapack">The primary parent datapack.</param>
/// <param name="CompatibilityAddons">The discovered compatibility addons included in the generation.</param>
/// <param name="OutputFile">The generated <c>base_translation.json</c> file, or <see langword="null"/> if failed.</param>
/// <param name="ErrorMessage">Error description if the generation failed.</param>
public record BaseTranslationUpdateResult(
    Datapack Datapack,
    IReadOnlyList<Datapack> CompatibilityAddons,
    FileInfo? OutputFile,
    string? ErrorMessage = null
)
{
    /// <summary>
    /// Gets a value indicating whether the generation completed successfully.
    /// </summary>
    public bool IsSuccess => OutputFile is not null && ErrorMessage is null;
}