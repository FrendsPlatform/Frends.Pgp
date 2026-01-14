namespace Frends.Pgp.VerifySignature.Definitions;

/// <summary>
/// Result of the task.
/// </summary>
public class Result
{
    internal Result(bool success, bool isValid, string signerKeyId, Error error = null)
    {
        Success = success;
        IsValid = isValid;
        SignerKeyId = signerKeyId;
        Error = error;
    }

    /// <summary>
    /// Indicates if the task completed successfully.
    /// </summary>
    /// <example>true</example>
    public bool Success { get; set; }

    /// <summary>
    /// Indicates whether the signature is valid.
    /// </summary>
    /// <example>true</example>
    public bool IsValid { get; set; }

    /// <summary>
    /// Key ID that was used for signing (in hexadecimal format).
    /// </summary>
    /// <example>A1234B5678...</example>
    public string SignerKeyId { get; set; }

    /// <summary>
    /// Error that occurred during task execution.
    /// </summary>
    /// <example>object { string Message, Exception AdditionalInfo }</example>
    public Error Error { get; set; }
}