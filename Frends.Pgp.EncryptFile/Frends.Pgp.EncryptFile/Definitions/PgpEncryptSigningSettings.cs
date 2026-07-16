using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.Pgp.EncryptFile.Definitions;

/// <summary>
/// Encryption Signing settings.
/// </summary>
public class PgpEncryptSigningSettings
{
    /// <summary>
    /// Source of the private key - file path or string.
    /// </summary>
    /// <example>PrivateKeySource.File</example>
    public PrivateKeySource PrivateKeySource { get; set; } = PrivateKeySource.File;

    /// <summary>
    /// Path to the private key file.
    /// </summary>
    /// <example>C:\temp\privateKeyFile.gpg</example>
    [UIHint(nameof(PrivateKeySource), "", PrivateKeySource.File)]
    [DisplayFormat(DataFormatString = "Text")]
    public string PrivateKeyPath { get; set; }

    /// <summary>
    /// Private key as a string, including the BEGIN/END PGP PRIVATE KEY BLOCK lines.
    /// </summary>
    /// <example>
    /// -----BEGIN PGP PRIVATE KEY BLOCK-----
    /// ...
    /// -----END PGP PRIVATE KEY BLOCK-----
    /// </example>
    [UIHint(nameof(PrivateKeySource), "", PrivateKeySource.String)]
    [DisplayFormat(DataFormatString = "Text")]
    [PasswordPropertyText]
    public string PrivateKeyString { get; set; }

    /// <summary>
    /// Encoding used for the private key passphrase. Defaults to Utf8.
    /// Use Legacy if the passphrase contains non-ASCII characters and the key was created with an older PGP tool.
    /// </summary>
    /// <example>PassphraseEncoding.Utf8</example>
    public PassphraseEncoding PassphraseEncoding { get; set; } = PassphraseEncoding.Utf8;

    /// <summary>
    /// Passphrase for the private key.
    /// </summary>
    /// <example>passphrase</example>
    [PasswordPropertyText]
    public string PrivateKeyPassphrase { get; set; }

    /// <summary>
    /// Hash algorithm to use with signature
    /// </summary>
    /// <example>PgpEncryptSignatureHashAlgorithm.Sha1</example>
    [DefaultValue(PgpEncryptSignatureHashAlgorithm.Sha256)]
    public PgpEncryptSignatureHashAlgorithm SignatureHashAlgorithm { get; set; } = PgpEncryptSignatureHashAlgorithm.Sha256;
}
