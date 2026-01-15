using System;
using System.IO;
using System.Text;
using Org.BouncyCastle.Bcpg.OpenPgp;

namespace Frends.Pgp.VerifySignature.Helpers
{
    internal static class PgpVerificationServices
    {
        internal static PgpPublicKey GetPublicKey(
               string publicKeySource,
               bool useFileKey,
               long keyId)
        {
            Stream publicKeyStream;

            if (useFileKey)
            {
                if (!File.Exists(publicKeySource))
                    throw new FileNotFoundException($"Public key file not found: {publicKeySource}");

                publicKeyStream = File.OpenRead(publicKeySource);
            }
            else
            {
                byte[] keyBytes = Encoding.UTF8.GetBytes(publicKeySource);
                publicKeyStream = new MemoryStream(keyBytes);
            }

            try
            {
                using (publicKeyStream)
                using (Stream decoderStream = PgpUtilities.GetDecoderStream(publicKeyStream))
                {
                    var publicKeyRingBundle = new PgpPublicKeyRingBundle(decoderStream);

                    PgpPublicKey key = publicKeyRingBundle.GetPublicKey(keyId);

                    if (key != null)
                        return key;

                    throw new Exception($"Public key with ID {keyId:X} not found in keyring.");
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("not found in keyring"))
                    throw;
                throw new Exception("Failed to read public key. Ensure the key is valid and properly formatted.", ex);
            }
        }
    }
}
