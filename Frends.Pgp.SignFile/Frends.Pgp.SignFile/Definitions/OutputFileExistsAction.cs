namespace Frends.Pgp.SignFile.Definitions;

/// <summary>
/// Enumeration for the action if the output file already exists.
/// </summary>
public enum OutputFileExistsAction
{
    /// <summary>
    /// Output file is overwritten if it exists.
    /// </summary>
    Overwrite,

    /// <summary>
    /// Exception is thrown if the output file already exists.
    /// </summary>
    Error,
}
