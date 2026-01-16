using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.Pgp.VerifySignature.Definitions;

/// <summary>
/// Essential parameters.
/// </summary>
public class Input
{
    /// <summary>
    /// Path to the original file (for detached signature) or signed file (for attached signature).
    /// </summary>
    /// <example>C:\temp\message.txt</example>
    [DisplayFormat(DataFormatString = "Text")]
    [DefaultValue("")]
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// Path to the detached signature file. Required only when IsDetachedSignature is true.
    /// </summary>
    /// <example>C:\temp\message.txt.sig</example>
    [DisplayFormat(DataFormatString = "Text")]
    [DefaultValue("")]
    public string SignatureFilePath { get; set; } = string.Empty;
}
