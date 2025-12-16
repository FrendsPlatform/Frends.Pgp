namespace Frends.Pgp.EncryptFile.Definitions;

/// <summary>
/// Enumeration of supported encryption algorithms for PGP encryption.
/// </summary>
public enum PgpEncryptEncryptionAlgorithm
{
    /// <summary>AES with 128-bit key; secure and widely used.</summary>
    Aes128,

    /// <summary>AES with 192-bit key; secure and widely used.</summary>
    Aes192,

    /// <summary>AES with 256-bit key; secure and widely used, recommended for highest security.</summary>
    Aes256,

    /// <summary>Blowfish: symmetric cipher, moderately secure, older algorithm.</summary>
    Blowfish,

    /// <summary>Camellia with 128-bit key; secure, alternative to AES.</summary>
    Camellia128,

    /// <summary>Camellia with 192-bit key; secure, alternative to AES.</summary>
    Camellia192,

    /// <summary>Camellia with 256-bit key; secure, alternative to AES.</summary>
    Camellia256,

    /// <summary>CAST5: older symmetric cipher, moderately secure.</summary>
    Cast5,

    /// <summary>DES: outdated and insecure, provided only for legacy compatibility.</summary>
    Des,

    /// <summary>IDEA: older symmetric cipher, moderately secure, legacy use.</summary>
    Idea,

    /// <summary>Triple DES (3DES): improves DES security, but slower and not recommended for new implementations.</summary>
    TripleDes,

    /// <summary>Twofish: symmetric cipher, secure, alternative to AES.</summary>
    Twofish,
}