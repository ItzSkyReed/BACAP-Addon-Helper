namespace BacapGenerator.Exceptions;

/// <summary>
/// Exception thrown when a parsed advancement does not meet BACAP requirements.
/// </summary>
public class BacapAdvancementValidationException(string message) : Exception(message);