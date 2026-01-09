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
    /// <example>HashAlgorithmType.Sha256</example>
    [DefaultValue(PgpSignatureHashAlgorithm.Sha256)]
    public PgpSignatureHashAlgorithm SignatureHashAlgorithm { get; set; } = PgpSignatureHashAlgorithm.Sha256;

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
