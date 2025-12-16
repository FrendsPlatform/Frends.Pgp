using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.Pgp.EncryptFile.Definitions;

/// <summary>
/// Essential parameters.
/// </summary>
public class Input
{
    /// <summary>
    /// Full path to the file for encryption.
    /// </summary>
    /// <example>C:\temp\message.txt</example>
    [DisplayFormat(DataFormatString = "Text")]
    public string SourceFilePath { get; set; }

    /// <summary>
    /// Full path for the encrypted file.
    /// </summary>
    /// <example>C:\temp\message.pgp</example>
    [DisplayFormat(DataFormatString = "Text")]
    public string OutputFilePath { get; set; }

    /// <summary>
    /// Action for if the output file exists.
    /// </summary>
    /// <example>OutputFileExistsAction.Overwrite</example>
    public OutputFileExistsAction OutputFileExistsAction { get; set; } = OutputFileExistsAction.Overwrite;

    /// <summary>
    /// Full path for the public key.
    /// </summary>
    /// <example>C:\temp\publicKey.asc</example>
    [DisplayFormat(DataFormatString = "Text")]
    public string PublicKeyFile { get; set; }

    /// <summary>
    /// ID of the correct key in the key ring (Optional).
    /// If left empty first suitable key in key ring is used.
    /// </summary>
    /// <example>
    /// 0x1234567890ABCDEF
    /// 1234567890ABCDEF
    /// </example>
    [DisplayFormat(DataFormatString = "Text")]
    public string PublicKeyId { get; set; }

    /// <summary>
    /// Encryption algorithm to use
    /// </summary>
    /// <example>PgpEncryptEncryptionAlgorithm.Cast5</example>
    [DefaultValue(PgpEncryptEncryptionAlgorithm.Cast5)]
    public PgpEncryptEncryptionAlgorithm EncryptionAlgorithm { get; set; } = PgpEncryptEncryptionAlgorithm.Cast5;
}
