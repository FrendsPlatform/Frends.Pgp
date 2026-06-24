using System;
using System.IO;
using System.Text;
using Frends.Pgp.DecryptFile.Definitions;
using Org.BouncyCastle.Bcpg.OpenPgp;

namespace Frends.Pgp.DecryptFile.Helpers;

internal static class PgpService
{
    internal static PgpPrivateKey FindPrivateKey(Input input, long keyId)
    {
        if (input.PrivateKeyPassphrase == null)
            throw new ArgumentException("Private key passphrase is not configured.");

        Stream keyStream;
        if (input.PrivateKeySource == PrivateKeySource.String)
        {
            if (string.IsNullOrEmpty(input.PrivateKeyString))
                throw new ArgumentException("Private key string was not given.");
            keyStream = new MemoryStream(Encoding.UTF8.GetBytes(input.PrivateKeyString));
        }
        else
        {
            if (string.IsNullOrEmpty(input.PrivateKeyPath))
                throw new ArgumentException("Private key path is not configured.");
            if (!File.Exists(input.PrivateKeyPath))
                throw new FileNotFoundException("Private key file not found.", input.PrivateKeyPath);
            keyStream = File.OpenRead(input.PrivateKeyPath);
        }

        using (keyStream)
        using (var decoderStream = PgpUtilities.GetDecoderStream(keyStream))
        {
            var secretKeyRingBundle = new PgpSecretKeyRingBundle(decoderStream);
            var secretKey = secretKeyRingBundle.GetSecretKey(keyId);
            return input.PassphraseEncoding == PassphraseEncoding.Utf8
                ? secretKey?.ExtractPrivateKeyUtf8(input.PrivateKeyPassphrase.ToCharArray())
                : secretKey?.ExtractPrivateKey(input.PrivateKeyPassphrase.ToCharArray());
        }
    }
}