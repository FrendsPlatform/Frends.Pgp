using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.Pgp.EncryptFile.Definitions;

/// <summary>
/// Additional parameters.
/// </summary>
public class Options
{
    /// <summary>
    /// Whether to throw an error on failure. True by default.
    /// </summary>
    /// <example>false</example>
    [DefaultValue(true)]
    public bool ThrowErrorOnFailure { get; set; } = true;

    /// <summary>
    /// Use ascii armor or not.
    /// </summary>
    /// <example>true</example>
    [DefaultValue(true)]
    public bool UseArmor { get; set; } = true;

    /// <summary>
    /// Check integrity of output file or not.
    /// </summary>
    /// <example>true</example>
    [DefaultValue(true)]
    public bool UseIntegrityCheck { get; set; } = true;

    /// <summary>
    /// Should compression be used?
    /// </summary>
    /// <example>true</example>
    [DefaultValue(true)]
    public bool UseCompression { get; set; } = true;

    /// <summary>
    /// Type of compression to use
    /// </summary>
    /// <example>PgpEncryptCompressionType.Zip</example>
    [DefaultValue(PgpEncryptCompressionType.Zip)]
    [UIHint(nameof(UseCompression), "", true)]
    public PgpEncryptCompressionType CompressionType { get; set; } = PgpEncryptCompressionType.Zip;

    /// <summary>
    /// Buffer size in KB that will be used when encrypting the file.
    /// </summary>
    /// <example>64</example>
    [DefaultValue(64)]
    public int EncryptBufferSize { get; set; } = 64;

    /// <summary>
    /// Should the encrypted file be signed with private key?
    /// </summary>
    /// <example>true</example>
    [DefaultValue(false)]
    public bool SignWithPrivateKey { get; set; } = false;

    /// <summary>
    /// File signing related settings
    /// </summary>
    /// <example>PrivateKeyFile: C:\temp\privateKeyFile.gpg, PrivateKeyPassword: passphrase, SignatureHashAlgorithm: PgpEncryptSignatureHashAlgorithm.Sha1</example>
    [UIHint(nameof(SignWithPrivateKey), "", true)]
    public PgpEncryptSigningSettings SigningSettings { get; set; } = new PgpEncryptSigningSettings();

    /// <summary>
    /// Overrides the error message on failure.
    /// </summary>
    /// <example>Failed to encrypt file.</example>
    [DisplayFormat(DataFormatString = "Text")]
    public string ErrorMessageOnFailure { get; set; }
}
