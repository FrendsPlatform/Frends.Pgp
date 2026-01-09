using System.ComponentModel;

namespace Frends.Pgp.SignFile.Definitions;

/// <summary>
/// Connection parameters.
/// </summary>
public class Connection
{
    /// <summary>
    /// Private key file path or ASCII-armored private key content
    /// </summary>
    /// <example>C:\temp\privatekey.asc</example>
    public string PrivateKey { get; set; }

    /// <summary>
    /// Password/passphrase for the private key
    /// </summary>
    /// <example>MySecurePassphrase123</example>
    public string PrivateKeyPassword { get; set; }

    /// <summary>
    /// If true, PrivateKey is treated as a file path. If false, treated as raw key string.
    /// </summary>
    /// <example>true</example>
    [DefaultValue(true)]
    public bool UseFileKey { get; set; } = true;
}
