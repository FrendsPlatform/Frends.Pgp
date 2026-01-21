using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.Pgp.SignFile.Definitions;

/// <summary>
/// Additional parameters.
/// </summary>
public class Options
{
    /// <summary>
    /// Create a detached signature (true) or attached signature (false).
    /// Detached signature creates a separate .sig file, attached signature embeds the signature with the data.
    /// </summary>
    /// <example>true</example>
    [DefaultValue(true)]
    public bool DetachedSignature { get; set; } = true;

    /// <summary>
    /// Use ascii armor or not.
    /// </summary>
    /// <example>true</example>
    [DefaultValue(true)]
    public bool UseArmor { get; set; } = true;

    /// <summary>
    /// Hash algorithm to use with signature
    /// </summary>
    /// <example>PgpSignatureHashAlgorithm.Sha256</example>
    [DefaultValue(PgpSignatureHashAlgorithm.Sha256)]
    public PgpSignatureHashAlgorithm SignatureHashAlgorithm { get; set; } = PgpSignatureHashAlgorithm.Sha256;

    /// <summary>
    /// If true, PrivateKey is treated as a file path. If false, treated as raw key string.
    /// </summary>
    /// <example>true</example>
    [DefaultValue(true)]
    public bool UseFileKey { get; set; } = true;

    /// <summary>
    /// Private key file path or ASCII-armored private key content
    /// </summary>
    /// <example>C:\temp\privatekey.asc</example>
    [DisplayFormat(DataFormatString = "Text")]
    [DefaultValue("")]
    public string PrivateKey { get; set; } = string.Empty;

    /// <summary>
    /// Password/passphrase for the private key
    /// </summary>
    /// <example>MySecurePassphrase123</example>
    [PasswordPropertyText]
    [DefaultValue("")]
    public string PrivateKeyPassword { get; set; }

    /// <summary>
    /// Whether to compress the signed data (only applies to attached signatures)
    /// </summary>
    /// <example>false</example>
    [UIHint(nameof(DetachedSignature), "", false)]
    [DefaultValue(false)]
    public bool UseCompression { get; set; } = false;

    /// <summary>
    /// Buffer size in KB for reading the file during signature generation
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
