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
    /// <example>C:\secretes\message.gpg</example>
    [DisplayFormat(DataFormatString = "Text")]
    public string SourceFilePath { get; set; }

    /// <summary>
    /// Path where the decrypted file will be saved to.
    /// </summary>
    /// <example>C:\messages\result.txt</example>
    [DisplayFormat(DataFormatString = "Text")]
    public string OutputFilePath { get; set; }

    /// <summary>
    /// Buffer size in KB that will be used when encrypting the file.
    /// </summary>
    /// <example>64</example>
    [DefaultValue(64)]
    public int DecryptBufferSize { get; init; } = 64;

    /// <summary>
    /// Path to the private key file.
    /// </summary>
    /// <example>C:\keys\my_key.asc</example>
    public string PrivateKeyPath { get; set; }

    /// <summary>
    /// Passphrase for the private key.
    /// </summary>
    /// <example>P@ssw0rd</example>
    public string PrivateKeyPassphrase { get; set; }
}