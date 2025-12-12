using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.Pgp.EncryptFile.Definitions;

/// <summary>
/// Encryption Signing settings.
/// </summary>
public class PgpEncryptSigningSettings
{
    /// <summary>
    /// Path to private key to sign with
    /// </summary>
    /// <example>C:\temp\privateKeyFile.gpg</example>
    [DisplayFormat(DataFormatString = "Text")]
    public string PrivateKeyFile { get; set; }

    /// <summary>
    /// If the file should be signed with private key then password to private key has to be offered
    /// </summary>
    /// <example>passphrase</example>
    [PasswordPropertyText]
    public string PrivateKeyPassword { get; set; }

    /// <summary>
    /// Hash algorithm to use with signature
    /// </summary>
    /// <example>PgpEncryptSignatureHashAlgorithm.Sha1</example>
    [DefaultValue(PgpEncryptSignatureHashAlgorithm.Sha1)]
    public PgpEncryptSignatureHashAlgorithm SignatureHashAlgorithm { get; set; }
}
