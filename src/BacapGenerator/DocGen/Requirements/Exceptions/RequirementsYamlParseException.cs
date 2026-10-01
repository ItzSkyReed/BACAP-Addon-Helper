namespace BacapGenerator.DocGen.Requirements.Exceptions;

/// <summary>
/// Exception thrown when the user-edited requirements YAML file contains invalid syntax.
/// </summary>
public class RequirementsYamlParseException(string filePath, string message, long lineNumber, long column, Exception innerException)
    : Exception($"Syntax error in requirements file '{filePath}' at Line {lineNumber}, Col {column}: {message}", innerException)
{
    public string FilePath { get; } = filePath;
    public long LineNumber { get; } = lineNumber;
    public long Column { get; } = column;
}