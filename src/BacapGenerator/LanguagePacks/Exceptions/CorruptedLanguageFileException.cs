using JetBrains.Annotations;

namespace BacapGenerator.LanguagePacks.Exceptions;

/// <summary>
/// Exception thrown when a Minecraft language JSON file is corrupted or contains invalid syntax.
/// </summary>
[PublicAPI]
public class CorruptedLanguageFileException : Exception
{
    /// <summary>
    /// Gets the absolute or relative path to the corrupted language file.
    /// </summary>
    public string FilePath { get; }

    /// <summary>
    /// Gets the 1-based line number where the JSON syntax error occurred, if available.
    /// </summary>
    public long? LineNumber { get; }

    /// <summary>
    /// Gets the 0-based byte position in the line where the error occurred, if available.
    /// </summary>
    public long? BytePositionInLine { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CorruptedLanguageFileException"/> class.
    /// </summary>
    /// <param name="filePath">The filesystem path to the invalid file.</param>
    /// <param name="message">A description of the parsing error.</param>
    /// <param name="lineNumber">Optional line number where the parser failed.</param>
    /// <param name="bytePositionInLine">Optional byte position in line.</param>
    /// <param name="innerException">The underlying parser exception.</param>
    public CorruptedLanguageFileException(
        string filePath,
        string message,
        long? lineNumber = null,
        long? bytePositionInLine = null,
        Exception? innerException = null)
        : base(message, innerException)
    {
        FilePath = filePath;
        LineNumber = lineNumber;
        BytePositionInLine = bytePositionInLine;
    }
}