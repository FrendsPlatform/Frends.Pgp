namespace Frends.Pgp.SignFile.Definitions;

/// <summary>
/// Result of the task.
/// </summary>
public class Result
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Result"/> class.
    /// </summary>
    /// <param name="success">True if the operation succeeded.</param>
    /// <param name="path">The path to the signed file.</param>
    /// <param name="error">Error details if the operation failed.</param>
    internal Result(bool success, string path, Error error = null)
    {
        Success = success;
        FilePath = path;
        Error = error;
    }

    /// <summary>
    /// Indicates if the task completed successfully.
    /// </summary>
    /// <example>true</example>
    public bool Success { get; set; }

    /// <summary>
    /// File path to the signed file.
    /// </summary>
    /// <example>C:\temp\message.pgp</example>
    public string FilePath { get; set; }

    /// <summary>
    /// Error that occurred during task execution.
    /// </summary>
    /// <example>object { string Message, Exception AdditionalInfo }</example>
    public Error Error { get; set; }
}
