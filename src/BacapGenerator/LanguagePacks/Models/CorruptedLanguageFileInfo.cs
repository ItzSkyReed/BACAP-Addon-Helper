namespace BacapGenerator.LanguagePacks.Models;

/// <summary>
/// Describes a language file that failed parsing due to syntax or structure errors.
/// </summary>
/// <param name="File">The file descriptor.</param>
/// <param name="ErrorMessage">Human-readable error details.</param>
/// <param name="LineNumber">The line where the error occurred, if available.</param>
public record CorruptedLanguageFileInfo(
    FileInfo File,
    string ErrorMessage,
    long? LineNumber = null
);