using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.Pgp.VerifySignature.Definitions;

/// <summary>
/// Additional parameters.
/// </summary>
public class Options
{
    /// <summary>
    /// Whether the signature is detached (separate .sig file) or attached (signature embedded in signed file).
    /// </summary>
    /// <example>true</example>
    [DefaultValue(true)]
    public bool IsDetachedSignature { get; set; } = true;

    /// <summary>
    /// Public key file path or ASCII-armored public key content.
    /// </summary>
    /// <example>C:\temp\publickey.asc</example>
    [DisplayFormat(DataFormatString = "Text")]
    [DefaultValue("")]
    public string PublicKey { get; set; } = string.Empty;

    /// <summary>
    /// If true, PublicKey is treated as a file path. If false, treated as raw key string.
    /// </summary>
    /// <example>true</example>
    [DefaultValue(true)]
    public bool UseFileKey { get; set; } = true;

    /// <summary>
    /// Buffer size in KB for reading the file during signature verification
    /// </summary>
    /// <example>16</example>
    [DefaultValue(16)]
    public int SignatureBufferSize { get; set; } = 16;

    /// <summary>
    /// Whether to throw an error on failure.
    /// </summary>
    /// <example>true</example>
    [DefaultValue(true)]
    public bool ThrowErrorOnFailure { get; set; } = true;

    /// <summary>
    /// Overrides the error message on failure.
    /// </summary>
    /// <example>Custom error message</example>
    [DisplayFormat(DataFormatString = "Text")]
    [DefaultValue("")]
    public string ErrorMessageOnFailure { get; set; } = string.Empty;
}
