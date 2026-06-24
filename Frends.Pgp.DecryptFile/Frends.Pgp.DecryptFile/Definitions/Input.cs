using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.Pgp.DecryptFile.Definitions;

/// <summary>
/// Essential parameters.
/// </summary>
public class Input
{
    /// <summary>
    /// Path to the file containing the data to be decrypted.
    /// </summary>
    /// <example>C:\secrets\message.gpg</example>
    [DisplayFormat(DataFormatString = "Text")]
    public string SourceFilePath { get; set; }

    /// <summary>
    /// Path where the decrypted file will be saved to.
    /// </summary>
    /// <example>C:\messages\result.txt</example>
    [DisplayFormat(DataFormatString = "Text")]
    public string OutputFilePath { get; set; }

    /// <summary>
    /// Buffer size in KB that will be used when decrypting the file.
    /// </summary>
    /// <example>64</example>
    [DefaultValue(64)]
    public int DecryptBufferSize { get; init; } = 64;

    /// <summary>
    /// Source of the private key - file path or string.
    /// </summary>
    /// <example>PrivateKeySource.File</example>
    public PrivateKeySource PrivateKeySource { get; set; } = PrivateKeySource.File;

    /// <summary>
    /// Path to the private key file.
    /// </summary>
    /// <example>C:\keys\my_key.asc</example>
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
    /// <example>P@ssw0rd</example>
    [PasswordPropertyText]
    public string PrivateKeyPassphrase { get; set; }
}