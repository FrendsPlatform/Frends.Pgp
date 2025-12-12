using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.Pgp.EncryptFile.Definitions;

/// <summary>
/// Essential parameters.
/// </summary>
public class Input
{
    /// <summary>
    /// Full path to the file for enccryption.
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
    /// Full path for the public key.
    /// </summary>
    /// <example>C:\temp\publicKet.asc</example>
    [DisplayFormat(DataFormatString = "Text")]
    public string PublicKeyFile { get; set; }

    /// <summary>
    /// ID of the correct key in the key ring (Optional).
    /// If left empty first suitable key in key ring is used.
    /// </summary>
    /// <example>0x1234567890ABCDEF</example>
    public ulong PublicKeyID { get; set; }

    /// <summary>
    /// Use ascii armor or not.
    /// </summary>
    /// <example>true</example>
    [DefaultValue(true)]
    public bool UseArmor { get; set; }

    /// <summary>
    /// Check integrity of output file or not.
    /// </summary>
    /// <example>true</example>
    [DefaultValue(true)]
    public bool UseIntegrityCheck { get; set; }

    /// <summary>
    /// Should compression be used?
    /// </summary>
    /// <example>true</example>
    [DefaultValue(true)]
    public bool UseCompression { get; set; }

    /// <summary>
    /// Type of compression to use
    /// </summary>
    /// <example>PgpEncryptCompressionType.Zip</example>
    [DefaultValue(PgpEncryptCompressionType.Zip)]
    [UIHint(nameof(UseCompression), "", true)]
    public PgpEncryptCompressionType CompressionType { get; set; }

    /// <summary>
    /// Encryption algorithm to use
    /// </summary>
    /// <example>PgpEncryptEncryptionAlgorithm.Cast5</example>
    [DefaultValue(PgpEncryptEncryptionAlgorithm.Cast5)]
    public PgpEncryptEncryptionAlgorithm EncryptionAlgorithm { get; set; }

    /// <summary>
    /// Should the encrypted file be signed with private key?
    /// </summary>
    /// <example>true</example>
    public bool SignWithPrivateKey { get; set; }

    /// <summary>
    /// File signing related settings
    /// </summary>
    /// <example>PrivateKeyFile: C:\temp\privateKeyFile.gpg, PrivateKeyPassword: passphrase, SignatureHashAlgorithm: PgpEncryptSignatureHashAlgorithm.Sha1</example>
    [UIHint(nameof(SignWithPrivateKey), "", true)]
    public PgpEncryptSigningSettings SigningSettings { get; set; }
}
