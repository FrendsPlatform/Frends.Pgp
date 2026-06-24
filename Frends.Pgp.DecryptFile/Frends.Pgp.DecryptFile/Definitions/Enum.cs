namespace Frends.Pgp.DecryptFile.Definitions
{
    /// <summary>
    /// Passphrase encoding options.
    /// </summary>
    public enum PassphraseEncoding
    {
        /// <summary>
        /// Standard encoding used by modern PGP tools (GnuPG, Kleopatra).
        /// </summary>
        Utf8,

        /// <summary>
        /// Older encoding, only needed for keys created with legacy tools.
        /// </summary>
        Legacy,
    }

    /// <summary>
    /// Source of the private key.
    /// </summary>
    public enum PrivateKeySource
    {
        /// <summary>
        /// Private key is read from a file.
        /// </summary>
        File,

        /// <summary>
        /// Private key is provided directly as a string.
        /// </summary>
        String,
    }
}
