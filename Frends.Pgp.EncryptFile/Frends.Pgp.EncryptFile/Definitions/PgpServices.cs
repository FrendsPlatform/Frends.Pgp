using System;
using System.IO;
using Org.BouncyCastle.Bcpg;
using Org.BouncyCastle.Bcpg.OpenPgp;
using Org.BouncyCastle.Security;

namespace Frends.Pgp.EncryptFile.Definitions;

internal class PgpServices
{
    /// <summary>
    /// Helper for getting encryption stream
    /// </summary>
    /// <param name="stream">Stream to chain for encryption</param>
    /// <param name="input">Task settings</param>
    /// <returns>Encryption chained stream</returns>
    internal static Stream GetEncryptionStream(Stream stream, Input input)
    {
        SymmetricKeyAlgorithmTag algorithmTag = input.EncryptionAlgorithm.ConvertEnum<SymmetricKeyAlgorithmTag>();
        PgpPublicKey publicKey = ReadPublicKey(input.PublicKeyFile, input.PublicKeyID);
        PgpEncryptedDataGenerator encryptedDataGenerator = new PgpEncryptedDataGenerator(algorithmTag, input.UseIntegrityCheck, new SecureRandom());
        encryptedDataGenerator.AddMethod(publicKey);
        return encryptedDataGenerator.Open(stream, new byte[input.EncryptBufferSize * 1024]);
    }

    /// <summary>
    /// Gets compression stream if compression is needed, otherwise returns original stream
    /// </summary>
    /// <param name="stream">Source stream</param>
    /// <param name="input">Task input</param>
    /// <returns>Compression chained stream or original source</returns>
    internal static Stream GetCompressionStream(Stream stream, Input input)
    {
        if (input.UseArmor)
        {
            CompressionAlgorithmTag compressionTag = input.CompressionType.ConvertEnum<CompressionAlgorithmTag>();
            PgpCompressedDataGenerator compressedDataGenerator = new PgpCompressedDataGenerator(compressionTag);
            return compressedDataGenerator.Open(stream);
        }

        return stream;
    }

    /// <summary>
    /// Find first suitable public key for encryption.
    /// </summary>
    /// <param name="publicKeyFile">Path to public key file</param>
    /// <param name="keyId">ID of the key</param>
    /// <returns>PgpPublicKey from public key file location</returns>
    internal static PgpPublicKey ReadPublicKey(string publicKeyFile, ulong keyId = 0)
    {
        using (Stream publicKeyStream = File.OpenRead(publicKeyFile))
        using (Stream decoderStream = PgpUtilities.GetDecoderStream(publicKeyStream))
        {
            var pgpPub = new PgpPublicKeyRingBundle(decoderStream);

            if (keyId > 0)
            {
                var key = pgpPub.GetPublicKey((long)keyId);

                if (key == null)
                    throw new Exception($"No public key found with Key ID {keyId:X}");

                ValidateUsableEncryptionKey(key);

                return key;
            }

            foreach (PgpPublicKeyRing kRing in pgpPub.GetKeyRings())
            {
                foreach (PgpPublicKey k in kRing.GetPublicKeys())
                {
                    if (k.IsEncryptionKey && !k.IsRevoked() && !IsExpired(k))
                        return k;
                }
            }
        }

        throw new ArgumentException("Can't find valid encryption key in key ring.");
    }

    /// <summary>
    /// Helper for creating a PgpSignatureGenerator from private key file and its password
    /// </summary>
    /// <param name="stream">Stream to use for signature initialization</param>
    /// <param name="input">Encryption task input</param>
    /// <returns>PgpSignatureGenerator to be used when signing a file</returns>
    internal static PgpSignatureGenerator InitPgpSignatureGenerator(Stream stream, Input input)
    {
        HashAlgorithmTag hashAlgorithm = input.SigningSettings.SignatureHashAlgorithm.ConvertEnum<HashAlgorithmTag>();

        try
        {
            PgpSecretKey secretKey = ReadSecretKey(input.SigningSettings.PrivateKeyFile);
            PgpPrivateKey privateKey = secretKey.ExtractPrivateKey(input.SigningSettings.PrivateKeyPassword.ToCharArray());

            var pgpSignatureGenerator = new PgpSignatureGenerator(secretKey.PublicKey.Algorithm, hashAlgorithm);
            pgpSignatureGenerator.InitSign(PgpSignature.BinaryDocument, privateKey);

            foreach (string userId in secretKey.PublicKey.GetUserIds())
            {
                PgpSignatureSubpacketGenerator spGen = new PgpSignatureSubpacketGenerator();
                spGen.SetSignerUserId(false, userId);
                pgpSignatureGenerator.SetHashedSubpackets(spGen.Generate());

                // Just the first one!
                break;
            }

            pgpSignatureGenerator.GenerateOnePassVersion(false).Encode(stream);
            return pgpSignatureGenerator;
        }
        catch (PgpException e)
        {
            throw new Exception("Private key extraction failed, password might be incorrect", e);
        }
    }

    /// <summary>
    /// Reads secret key from given privateKey
    /// </summary>
    /// <param name="privateKeyFile">Path to private key file</param>
    /// <returns>PgpSecretKey of the given privateKey</returns>
    internal static PgpSecretKey ReadSecretKey(string privateKeyFile)
    {
        PgpSecretKey secretKey = null;

        using (Stream secretKeyStream = File.OpenRead(privateKeyFile))
        {
            var secretKeyRingBundle = new PgpSecretKeyRingBundle(PgpUtilities.GetDecoderStream(secretKeyStream));

            foreach (PgpSecretKeyRing keyRing in secretKeyRingBundle.GetKeyRings())
            {
                foreach (PgpSecretKey key in keyRing.GetSecretKeys())
                {
                    if (key.IsSigningKey)
                        secretKey = key;
                }
            }

            if (secretKey == null)
                throw new Exception("Wrong private key - Can't find signing key in key ring.");
        }

        return secretKey;
    }

    private static void ValidateUsableEncryptionKey(PgpPublicKey key)
    {
        if (!key.IsEncryptionKey)
            throw new Exception($"Key {key.KeyId:X} is not valid for encryption.");

        if (key.IsRevoked())
            throw new Exception($"Key {key.KeyId:X} has been revoked and cannot be used.");

        if (IsExpired(key))
            throw new Exception($"Key {key.KeyId:X} is expired and cannot be used.");
    }

    private static bool IsExpired(PgpPublicKey key)
    {
        long validSeconds = key.GetValidSeconds();

        if (validSeconds == 0) // 0 means “never expires”
            return false;

        DateTime creation = key.CreationTime.ToUniversalTime();
        DateTime expires = creation.AddSeconds(validSeconds);

        return DateTime.UtcNow > expires;
    }
}
