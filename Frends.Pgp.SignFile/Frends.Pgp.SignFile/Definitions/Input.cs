using System.ComponentModel.DataAnnotations;

namespace Frends.Pgp.SignFile.Definitions;

/// <summary>
/// Essential parameters.
/// </summary>
public class Input
{
    /// <summary>
    /// Full path to the file for signing.
    /// </summary>
    /// <example>C:\temp\message.txt</example>
    [DisplayFormat(DataFormatString = "Text")]
    public string SourceFilePath { get; set; }

    /// <summary>
    /// Full path for the signing file.
    /// </summary>
    /// <example>C:\temp\message.pgp</example>
    [DisplayFormat(DataFormatString = "Text")]
    public string OutputFilePath { get; set; }

    /// <summary>
    /// Action for if the output file exists.
    /// </summary>
    /// <example>OutputFileExistsAction.Overwrite</example>
    public OutputFileExistsAction OutputFileExistsAction { get; set; } = OutputFileExistsAction.Overwrite;
}
