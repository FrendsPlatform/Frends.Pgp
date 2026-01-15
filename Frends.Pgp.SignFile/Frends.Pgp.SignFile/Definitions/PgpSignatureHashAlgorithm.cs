namespace Frends.Pgp.SignFile.Definitions;

/// <summary>
/// Enumeration of supported signature hash algorithms for PGP signing.
/// </summary>
/// <remarks>
/// MD2 and MD5 are deprecated and insecure; use only for legacy compatibility.
/// For new implementations, prefer SHA-256 or stronger algorithms.
/// </remarks>
public enum PgpSignatureHashAlgorithm
{
    /// <summary>MD2: deprecated, insecure, legacy use only.</summary>
    Md2,

    /// <summary>MD5: deprecated, insecure, legacy use only.</summary>
    Md5,

    /// <summary>RIPEMD-160: moderately secure, older algorithm.</summary>
    RipeMd160,

    /// <summary>SHA-1: insecure for new use, provided for legacy compatibility.</summary>
    Sha1,

    /// <summary>SHA-224: secure, part of the SHA-2 family.</summary>
    Sha224,

    /// <summary>SHA-256: recommended, secure, widely used.</summary>
    Sha256,

    /// <summary>SHA-384: secure, part of SHA-2 family, longer hash size.</summary>
    Sha384,

    /// <summary>SHA-512: secure, part of SHA-2 family, longest hash size.</summary>
    Sha512,
}