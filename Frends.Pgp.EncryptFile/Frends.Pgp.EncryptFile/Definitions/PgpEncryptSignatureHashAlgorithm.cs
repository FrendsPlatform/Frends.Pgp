namespace Frends.Pgp.EncryptFile.Definitions;

/// <summary>
/// Enumeration of the available signature hash algorithms.
/// </summary>
/// <remarks>
/// Note: MD2 and MD5 are cryptographically broken and provided only for legacy compatibility.
/// For new implementations, use SHA256 or stronger.
/// </remarks>
public enum PgpEncryptSignatureHashAlgorithm
{
    /// <summary>MD2 (deprecated - cryptographically broken, use only for legacy compatibility)</summary>
    Md2,
    /// <summary>MD5 (deprecated - cryptographically broken, use only for legacy compatibility)</summary>
    Md5,
    RipeMd160,
    Sha1,
    Sha224,
    Sha256,
    Sha384,
    Sha512,
}
