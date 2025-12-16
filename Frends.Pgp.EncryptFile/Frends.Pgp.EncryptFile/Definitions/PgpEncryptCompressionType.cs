namespace Frends.Pgp.EncryptFile.Definitions;

/// <summary>
/// Enumeration of supported compression types for PGP encryption.
/// </summary>
public enum PgpEncryptCompressionType
{
    /// <summary>BZip2 compression: high compression ratio, slower performance.</summary>
    BZip2,

    /// <summary>No compression applied; plaintext data is stored as-is.</summary>
    Uncompressed,

    /// <summary>ZIP compression: moderate compression ratio and speed, widely supported.</summary>
    Zip,

    /// <summary>ZLib compression: fast and efficient, good trade-off between speed and compression.</summary>
    ZLib,
}