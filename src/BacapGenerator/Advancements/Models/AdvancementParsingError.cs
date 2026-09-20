namespace BacapGenerator.Advancements.Models;

/// <summary>
/// Defines specific reasons why an advancement validation failed.
/// </summary>
public enum AdvancementParsingError
{
    MalformedJson,
    MissingRewardFunction,
    MissingDisplay,
    NotParsableTier,
    FailedToLoadAssociatedFunctions,
    Unknown
}