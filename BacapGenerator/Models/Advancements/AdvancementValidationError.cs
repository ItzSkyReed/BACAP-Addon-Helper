namespace BacapGenerator.Models.Advancements;

/// <summary>
/// Defines specific reasons why an advancement validation failed.
/// </summary>
public enum AdvancementValidationError
{
    MalformedJson,
    MissingRewardFunction,
    MissingDisplay,
    MissingTab,
    NotParsableTier,
    FailedToLoadAssociatedFunctions,
    Unknown
}