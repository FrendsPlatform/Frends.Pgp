using System;
using System.IO;
using Frends.Pgp.SignFile.Definitions;
using Org.BouncyCastle.Bcpg;
using Org.BouncyCastle.Bcpg.OpenPgp;

namespace Frends.Pgp.SignFile.Helpers;

internal static class PgpServices
{
    internal static PgpSignatureGenerator InitSignatureGenerator(
        string privateKeySource,
        string privateKeyPassword,
        PgpSignatureHashAlgorithm hashAlgorithm,
        bool useFileKey)
    {
        HashAlgorithmTag hashTag = hashAlgorithm.ConvertEnum<HashAlgorithmTag>();
        PgpSecretKey secretKey = ReadSecretKey(privateKeySource, useFileKey);

        if (string.IsNullOrWhiteSpace(privateKeyPassword))
            throw new ArgumentException("Private key password is required for signing.", nameof(privateKeyPassword));

        try
        {
            PgpPrivateKey privateKey = secretKey.ExtractPrivateKey(privateKeyPassword.ToCharArray());
            var signatureGenerator = new PgpSignatureGenerator(secretKey.PublicKey.Algorithm, hashTag);
            signatureGenerator.InitSign(PgpSignature.BinaryDocument, privateKey);

            foreach (string userId in secretKey.PublicKey.GetUserIds())
            {
                PgpSignatureSubpacketGenerator spGen = new PgpSignatureSubpacketGenerator();
                spGen.SetSignerUserId(false, userId);
                signatureGenerator.SetHashedSubpackets(spGen.Generate());
                break;
            }

            return signatureGenerator;
        }
        catch (PgpException e)
        {
            throw new Exception(
            $"Private key extraction failed: {e.Message}",
            e);
        }
    }

    internal static PgpSignatureGenerator InitSignatureGeneratorWithOnePass(
        Stream outputStream,
        string privateKey,
        string privateKeyPassword,
        PgpSignatureHashAlgorithm hashAlgorithm,
        bool useFileKey)
    {
        var signatureGenerator = InitSignatureGenerator(privateKey, privateKeyPassword, hashAlgorithm, useFileKey);

        signatureGenerator.GenerateOnePassVersion(false).Encode(outputStream);

        return signatureGenerator;
    }

    private static PgpSecretKey ReadSecretKey(string privateKey, bool useFileKey)
    {
        Stream secretKeyStream;

        if (useFileKey)
        {
            if (!File.Exists(privateKey))
                throw new FileNotFoundException($"Private key file not found: {privateKey}");

            secretKeyStream = File.OpenRead(privateKey);
        }
        else
        {
            byte[] keyBytes = System.Text.Encoding.UTF8.GetBytes(privateKey);
            secretKeyStream = new MemoryStream(keyBytes);
        }

        try
        {
            using (secretKeyStream)
            {
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
        }
        catch (Exception ex) when (ex.Message?.Contains("Can't find signing key") != true)
        {
            throw new Exception("Failed to read private key. Ensure the key is valid and properly formatted.", ex);
        }
    }
}
