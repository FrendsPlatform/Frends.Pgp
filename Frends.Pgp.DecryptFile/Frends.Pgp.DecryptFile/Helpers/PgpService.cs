using System;
using System.IO;
using Frends.Pgp.DecryptFile.Definitions;
using Org.BouncyCastle.Bcpg.OpenPgp;

namespace Frends.Pgp.DecryptFile.Helpers;

internal static class PgpService
{
    internal static PgpPrivateKey FindPrivateKey(Input input, long keyId)
    {
        if (string.IsNullOrEmpty(input.PrivateKeyPath))
            throw new ArgumentException("Private key path is not configured.");

        if (!File.Exists(input.PrivateKeyPath))
            throw new FileNotFoundException("Private key file not found.", input.PrivateKeyPath);

        if (input.PrivateKeyPassphrase == null)
            throw new ArgumentException("Private key passphrase is not configured.");

        using var keyStream = File.OpenRead(input.PrivateKeyPath);
        using var decoderStream = PgpUtilities.GetDecoderStream(keyStream);

        var secretKeyRingBundle = new PgpSecretKeyRingBundle(decoderStream);
        var secretKey = secretKeyRingBundle.GetSecretKey(keyId);

        return secretKey == null
            ? null
            : ExtractPrivateKey(secretKey, input.PrivateKeyPassphrase);
    }

    private static PgpPrivateKey ExtractPrivateKey(PgpSecretKey secretKey, string passphrase)
    {
        try
        {
            return secretKey.ExtractPrivateKey(passphrase.ToCharArray());
        }
        catch (PgpException ex) when (ex.Message.Contains("Checksum mismatch", StringComparison.Ordinal))
        {
            var trimmedPassphrase = passphrase.Trim();

            if (!string.Equals(passphrase, trimmedPassphrase, StringComparison.Ordinal))
            {
                try
                {
                    return secretKey.ExtractPrivateKey(trimmedPassphrase.ToCharArray());
                }
                catch (PgpException trimmedEx) when (trimmedEx.Message.Contains("Checksum mismatch", StringComparison.Ordinal))
                {
                    throw new ArgumentException("Failed to unlock private key. Check passphrase value and key compatibility.", trimmedEx);
                }
            }

            throw new ArgumentException("Failed to unlock private key. Check passphrase value and key compatibility.", ex);
        }
    }
}