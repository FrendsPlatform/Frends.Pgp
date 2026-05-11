using System;
using System.IO;
using System.Threading;
using Frends.Pgp.DecryptFile.Definitions;
using NUnit.Framework;
using Org.BouncyCastle.Bcpg;
using Org.BouncyCastle.Bcpg.OpenPgp;
using Org.BouncyCastle.Security;

namespace Frends.Pgp.DecryptFile.Tests;

[TestFixture]
public class UnitTests
{
    private const string OriginalMessageFile = "original_message.txt";
    private const string DecryptedFile = "decrypted.txt";
    private const string EncryptedFile = "encrypted.gpg";
    private const string SignedEncryptedFile = "signed_encrypted.gpg";

    private const string
        PrivateKey =
            "private_key.asc"; // this key should not be used on anything except testing as it is on the public GitHub repository.

    private const string Passphrase = "mat123";

    private static readonly string WorkDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../TestData/");
    private Input input;
    private Options options;

    [SetUp]
    public void Setup()
    {
        if (File.Exists(Path.Combine(WorkDir, DecryptedFile))) File.Delete(Path.Combine(WorkDir, DecryptedFile));
        // if (File.Exists(Path.Combine(WorkDir, SignedEncryptedFile)))
        //     File.Delete(Path.Combine(WorkDir, SignedEncryptedFile));
        input = new Input
        {
            SourceFilePath = Path.Combine(WorkDir, EncryptedFile),
            OutputFilePath = Path.Combine(WorkDir, DecryptedFile),
            PrivateKeyPath = Path.Combine(WorkDir, PrivateKey),
            PrivateKeyPassphrase = Passphrase,
            DecryptBufferSize = 64,
        };
        options = new Options
        {
            ThrowErrorOnFailure = false,
        };
    }

    [Test]
    public void DecryptFile_Runs_Correctly()
    {
        var result = Pgp.DecryptFile(input, options, CancellationToken.None);

        Assert.That(result.Success, Is.True);
        Assert.That(File.Exists(Path.Combine(WorkDir, DecryptedFile)), Is.True);

        var decryptedText = File.ReadAllText(Path.Combine(WorkDir, DecryptedFile));
        Assert.That(
            NormalizeLineEndings(decryptedText),
            Is.EqualTo(NormalizeLineEndings(File.ReadAllText(Path.Combine(WorkDir, OriginalMessageFile)))));
    }

    [Test]
    public void DecryptFile_Runs_Correctly_When_Message_Is_Signed()
    {
        var signedEncryptedPath = Path.Combine(WorkDir, SignedEncryptedFile);

        // CreateSignedAndEncryptedMessage(
        //     Path.Combine(WorkDir, OriginalMessageFile),
        //     signedEncryptedPath,
        //     Path.Combine(WorkDir, PrivateKey),
        //     Passphrase);

        input.SourceFilePath = signedEncryptedPath;

        var result = Pgp.DecryptFile(input, options, CancellationToken.None);

        Assert.That(result.Success, Is.True);
        Assert.That(File.Exists(Path.Combine(WorkDir, DecryptedFile)), Is.True);

        var decryptedText = File.ReadAllText(Path.Combine(WorkDir, DecryptedFile));
        Assert.That(NormalizeLineEndings(decryptedText),
            Is.EqualTo(NormalizeLineEndings(File.ReadAllText(Path.Combine(WorkDir, OriginalMessageFile)))));
    }

    [Test]
    public void DecryptFile_Fails_When_Passphrase_Is_Wrong()
    {
        input.PrivateKeyPassphrase = "invalid passphrase";
        var result = Pgp.DecryptFile(input, options, CancellationToken.None);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Error.Message, Contains.Substring("Private key passphrase is invalid."));
    }

    [Test]
    public void DecryptFile_Fails_When_Correct_Key_Is_Missing()
    {
        input.PrivateKeyPath = Path.Combine(WorkDir, "invalid_key.asc");
        var result = Pgp.DecryptFile(input, options, CancellationToken.None);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Error.Message, Contains.Substring("Matching private key not found."));
    }

    [Test]
    public void DecryptFile_Fails_When_SourceFile_Is_Missing()
    {
        input.SourceFilePath = Path.Combine(WorkDir, "invalid_path.pgp");
        var result = Pgp.DecryptFile(input, options, CancellationToken.None);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Error.Message, Contains.Substring("File to decrypt does not exist."));
    }

    [Test]
    public void DecryptFile_Fails_When_SourceFile_Is_Invalid()
    {
        input.SourceFilePath = Path.Combine(WorkDir, OriginalMessageFile);
        var result = Pgp.DecryptFile(input, options, CancellationToken.None);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Error.Message, Contains.Substring("Source file is not a valid PGP encrypted file."));
    }

    [Test]
    public void DecryptFile_Fails_When_OutputFilePath_Is_NotEmpty()
    {
        input.OutputFilePath = Path.Combine(WorkDir, OriginalMessageFile);
        var result = Pgp.DecryptFile(input, options, CancellationToken.None);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Error.Message, Contains.Substring("Output file already exists."));
    }

    // private static void CreateSignedAndEncryptedMessage(string sourcePath, string outputPath, string privateKeyPath,
    //     string passphrase)
    // {
    //     using var keyStream = File.OpenRead(privateKeyPath);
    //     using var decoderStream = PgpUtilities.GetDecoderStream(keyStream);
    //     var secretKeyRingBundle = new PgpSecretKeyRingBundle(decoderStream);
    //
    //     PgpSecretKey signingKey = null;
    //     PgpSecretKey encryptionKey = null;
    //
    //     foreach (PgpSecretKeyRing keyRing in secretKeyRingBundle.GetKeyRings())
    //     {
    //         foreach (PgpSecretKey secretKey in keyRing.GetSecretKeys())
    //         {
    //             if (signingKey == null && secretKey.IsSigningKey)
    //                 signingKey = secretKey;
    //
    //             if (encryptionKey == null && secretKey.PublicKey.IsEncryptionKey)
    //                 encryptionKey = secretKey;
    //
    //             if (signingKey != null && encryptionKey != null)
    //                 break;
    //         }
    //
    //         if (signingKey != null && encryptionKey != null)
    //             break;
    //     }
    //
    //     signingKey ??= encryptionKey;
    //     encryptionKey ??= signingKey;
    //
    //     if (signingKey == null || encryptionKey == null)
    //         throw new InvalidOperationException("Failed to resolve signing/encryption keys for test data generation.");
    //
    //     var privateKey = signingKey.ExtractPrivateKey(passphrase.ToCharArray());
    //     var encryptedDataGenerator =
    //         new PgpEncryptedDataGenerator(SymmetricKeyAlgorithmTag.Aes256, true, new SecureRandom());
    //     encryptedDataGenerator.AddMethod(encryptionKey.PublicKey);
    //
    //     using var outputStream = File.Create(outputPath);
    //     using var encryptedOut = encryptedDataGenerator.Open(outputStream, new byte[1 << 16]);
    //
    //     var signatureGenerator = new PgpSignatureGenerator(signingKey.PublicKey.Algorithm, HashAlgorithmTag.Sha256);
    //     signatureGenerator.InitSign(PgpSignature.BinaryDocument, privateKey);
    //
    //     var signatureSubpacketGenerator = new PgpSignatureSubpacketGenerator();
    //
    //     foreach (string userId in signingKey.PublicKey.GetUserIds())
    //     {
    //         signatureSubpacketGenerator.AddSignerUserId(false, userId);
    //
    //         break;
    //     }
    //
    //     signatureGenerator.SetHashedSubpackets(signatureSubpacketGenerator.Generate());
    //     signatureGenerator.GenerateOnePassVersion(false).Encode(encryptedOut);
    //
    //     var literalDataGenerator = new PgpLiteralDataGenerator();
    //
    //     using (var literalOut = literalDataGenerator.Open(encryptedOut, PgpLiteralData.Binary,
    //                Path.GetFileName(sourcePath), new FileInfo(sourcePath).Length, DateTime.UtcNow))
    //     using (var inputStream = File.OpenRead(sourcePath))
    //     {
    //         var buffer = new byte[8192];
    //         int read;
    //
    //         while ((read = inputStream.Read(buffer, 0, buffer.Length)) > 0)
    //         {
    //             literalOut.Write(buffer, 0, read);
    //             signatureGenerator.Update(buffer, 0, read);
    //         }
    //     }
    //
    //     signatureGenerator.Generate().Encode(encryptedOut);
    // }

    private static string NormalizeLineEndings(string value) => value.Replace("\r\n", "\n").Replace("\r", "\n");
}
