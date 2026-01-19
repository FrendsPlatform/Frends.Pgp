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
    /// Whether the signature is detached (separate .sig file) or attached (signature embedded in signed file).
    /// </summary>
    /// <example>true</example>
    [DefaultValue(true)]
    public bool IsDetachedSignature { get; set; } = true;

    /// <summary>
    /// Path to the detached signature file. Required only when IsDetachedSignature is true.
    /// </summary>
    /// <example>C:\temp\message.txt.sig</example>
    [UIHint(nameof(IsDetachedSignature), "", true)]
    [DisplayFormat(DataFormatString = "Text")]
    [DefaultValue("")]
    public string SignatureFilePath { get; set; } = string.Empty;
}
