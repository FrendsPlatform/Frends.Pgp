using System;
using System.Globalization;
using System.IO;
using System.Text;
using Org.BouncyCastle.Bcpg;
using Org.BouncyCastle.Bcpg.OpenPgp;
using Org.BouncyCastle.Security;

namespace Frends.Pgp.EncryptFile.Definitions;

internal static class PgpServices
{
    /// <summary>
    /// Helper for getting encryption stream
    /// </summary>
    /// <param name="stream">Stream to chain for encryption</param>
    /// <param name="input">Task input settings</param>
    /// <param name="options">Task optional settings</param>
    /// <returns>Encryption chained stream</returns>
    internal static Stream GetEncryptionStream(Stream stream, Input input, Options options)
    {
        SymmetricKeyAlgorithmTag algorithmTag = input.EncryptionAlgorithm.ConvertEnum<SymmetricKeyAlgorithmTag>();
        PgpPublicKey publicKey = ReadPublicKey(input.PublicKeyFile, input.PublicKeyId);
        PgpEncryptedDataGenerator encryptedDataGenerator = new PgpEncryptedDataGenerator(algorithmTag, options.UseIntegrityCheck, new SecureRandom());
        encryptedDataGenerator.AddMethod(publicKey);
        return encryptedDataGenerator.Open(stream, new byte[options.EncryptBufferSize * 1024]);
    }

    /// <summary>
    /// Gets compression stream if compression is needed, otherwise returns original stream
    /// </summary>
    /// <param name="stream">Source stream</param>
    /// <param name="options">Task optional settings</param>
    /// <returns>Compression chained stream or original source</returns>
    internal static Stream GetCompressionStream(Stream stream, Options options)
    {
        if (options.UseCompression)
        {
            CompressionAlgorithmTag compressionTag = options.CompressionType.ConvertEnum<CompressionAlgorithmTag>();
            PgpCompressedDataGenerator compressedDataGenerator = new PgpCompressedDataGenerator(compressionTag);
            return compressedDataGenerator.Open(stream);
        }

        return stream;
    }

    /// <summary>
    /// Helper for creating a PgpSignatureGenerator from private key file and its password
    /// </summary>
    /// <param name="stream">Stream to use for signature initialization</param>
    /// <param name="options">Task optional settings</param>
    /// <returns>PgpSignatureGenerator to be used when signing a file</returns>
    internal static PgpSignatureGenerator InitPgpSignatureGenerator(Stream stream, Options options)
    {
        HashAlgorithmTag hashAlgorithm = options.SigningSettings.SignatureHashAlgorithm.ConvertEnum<HashAlgorithmTag>();

        try
        {
            PgpSecretKey secretKey = ReadSecretKey(options.SigningSettings.PrivateKeyFile);
            PgpPrivateKey privateKey = secretKey.ExtractPrivateKey(options.SigningSettings.PrivateKeyPassword.ToCharArray());

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

    private static PgpSecretKey ReadSecretKey(string privateKeyFile)
    {
        using Stream secretKeyStream = File.OpenRead(privateKeyFile);
        var secretKeyRingBundle = new PgpSecretKeyRingBundle(PgpUtilities.GetDecoderStream(secretKeyStream));

        foreach (PgpSecretKeyRing keyRing in secretKeyRingBundle.GetKeyRings())
        {
            foreach (PgpSecretKey key in keyRing.GetSecretKeys())
            {
                if (key.IsSigningKey)
                    return key;
            }
        }

        throw new Exception("Wrong private key - Can't find signing key in key ring.");
    }

    private static PgpPublicKey ReadPublicKey(string publicKeyFile, string keyId)
    {
        using Stream publicKeyStream = File.OpenRead(publicKeyFile);
        using Stream decoderStream = PgpUtilities.GetDecoderStream(publicKeyStream);
        var pgpPub = new PgpPublicKeyRingBundle(decoderStream);
        PgpPublicKey key = null;

        if (!string.IsNullOrEmpty(keyId))
        {
            keyId = keyId.ToLower();
            if (keyId.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                keyId = keyId.Substring(2);

            if (ulong.TryParse(keyId, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out ulong value))
                key = pgpPub.GetPublicKey((long)value) ?? throw new Exception($"No public key found with Key ID {keyId:X}");
            else
                throw new Exception("Public key is invalid format");

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

        throw new ArgumentException("Can't find valid encryption key in key ring.");
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
