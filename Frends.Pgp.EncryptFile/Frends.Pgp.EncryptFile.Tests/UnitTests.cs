using System;
using System.IO;
using System.Text.RegularExpressions;
using Frends.Pgp.EncryptFile.Definitions;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Frends.Pgp.EncryptFile.Tests;

[TestFixture]
public class UnitTests
{
    // following keys should not be used on anything except testing as both private key and password are on public GitHub repository.
    private static readonly string WorkDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../TestData/");
    private static readonly string PublicKeyFile = "pub.asc";
    private static readonly string SourceFile = "original_message.txt";
    private static readonly string EncryptedFile = "encrypted_message.pgp";

    private readonly string privateKey = "privatekey.gpg";
    private readonly string privateKeyPassword = "veijo666";

    private Options options = new Options();
    private Input input = new Input();

    [SetUp]
    public void Setup()
    {
        input = new Input
        {
            SourceFilePath = Path.Combine(WorkDir, SourceFile),
            OutputFilePath = Path.Combine(WorkDir, EncryptedFile),
            PublicKeyFile = Path.Combine(WorkDir, PublicKeyFile),
            UseIntegrityCheck = true,
            UseArmor = true,
            UseCompression = true,
        };
    }

    [TearDown]
    public void DeleteTmpFile()
    {
        File.Delete(Path.Combine(WorkDir, EncryptedFile));
    }

    [Test]
    public void EncryptFile()
    {
        var result = Pgp.EncryptFile(input, options, default);

        var resultContent = File.ReadAllText(result.FilePath);

        string expected = "-----BEGINPGPMESSAGE-----Version:BouncyCastle.NETCryptography(OpenPGP-only,net6.0)v2.0.0.1hIwDzoB5W4N7pN4B";

        // Rest of the file is random.
        Assert.That(Regex.Replace(resultContent, @"[\s+]", string.Empty), Does.StartWith(Regex.Replace(expected, @"[\s+]", string.Empty)));
    }

    [Test]
    public void EncryptFile_WithPublicKeyID()
    {
        input.PublicKeyID = 0xce80795b837ba4de;
        var result = Pgp.EncryptFile(input, options, default);

        var resultContent = File.ReadAllText(result.FilePath);

        string expected = "-----BEGINPGPMESSAGE-----Version:BouncyCastle.NETCryptography(OpenPGP-only,net6.0)v2.0.0.1hIwDzoB5W4N7pN4B";

        // Rest of the file is random.
        Assert.That(Regex.Replace(resultContent, @"[\s+]", string.Empty), Does.StartWith(Regex.Replace(expected, @"[\s+]", string.Empty)));
    }

    [Test]
    public void EncryptFile_ShouldSignAndEncryptWithDefaultValues()
    {
        input.SignWithPrivateKey = true;
        input.SigningSettings = new PgpEncryptSigningSettings
        {
            PrivateKeyFile = Path.Combine(WorkDir, privateKey),
            PrivateKeyPassword = privateKeyPassword,
        };

        var result = Pgp.EncryptFile(input, options, default);
        string textResult = File.ReadAllText(result.FilePath);

        StringAssert.StartsWith($"-----BEGIN PGP MESSAGE-----\r\nVersion: BouncyCastle.NET Cryptography (OpenPGP-only, net6.0) v2.0.0.1\r\n\r\nhIwDzoB5W4N7pN4B", textResult);
        StringAssert.EndsWith($"-----END PGP MESSAGE-----{Environment.NewLine}", textResult);
    }

    [Test(Description = "Encryption algorithm, compression type and signature hash combination tests")]
    public void EncryptFile_ShouldSignAndEncryptWithAllAlgorithmCombinations(
        [Values(
        PgpEncryptEncryptionAlgorithm.Aes128,
        PgpEncryptEncryptionAlgorithm.Aes192,
        PgpEncryptEncryptionAlgorithm.Aes256,
        PgpEncryptEncryptionAlgorithm.Blowfish,
        PgpEncryptEncryptionAlgorithm.Camellia128,
        PgpEncryptEncryptionAlgorithm.Camellia192,
        PgpEncryptEncryptionAlgorithm.Camellia256,
        PgpEncryptEncryptionAlgorithm.Cast5,
        PgpEncryptEncryptionAlgorithm.Des,
        PgpEncryptEncryptionAlgorithm.Idea,
        PgpEncryptEncryptionAlgorithm.TripleDes,
        PgpEncryptEncryptionAlgorithm.Twofish)]
        PgpEncryptEncryptionAlgorithm encryptionAlgorithm,
        [Values(
        PgpEncryptCompressionType.BZip2,
        PgpEncryptCompressionType.Uncompressed,
        PgpEncryptCompressionType.Zip,
        PgpEncryptCompressionType.ZLib)]
        PgpEncryptCompressionType compressionType,
        [Values(
        PgpEncryptSignatureHashAlgorithm.Md2,
        PgpEncryptSignatureHashAlgorithm.Md5,
        PgpEncryptSignatureHashAlgorithm.RipeMd160,
        PgpEncryptSignatureHashAlgorithm.Sha1,
        PgpEncryptSignatureHashAlgorithm.Sha224,
        PgpEncryptSignatureHashAlgorithm.Sha256,
        PgpEncryptSignatureHashAlgorithm.Sha384,
        PgpEncryptSignatureHashAlgorithm.Sha512)]
        PgpEncryptSignatureHashAlgorithm signatureHash)
    {
        input.EncryptionAlgorithm = encryptionAlgorithm;
        input.CompressionType = compressionType;
        input.SignWithPrivateKey = true;
        input.SigningSettings = new PgpEncryptSigningSettings
        {
            PrivateKeyFile = Path.Combine(WorkDir, privateKey),
            PrivateKeyPassword = privateKeyPassword,
            SignatureHashAlgorithm = signatureHash,
        };

        var result = Pgp.EncryptFile(input, options, default);
        string textResult = File.ReadAllText(result.FilePath);

        // result has to start with pgp prefix, version comment and almost static 16 chars
        StringAssert.IsMatch(@"^-----BEGIN PGP MESSAGE-----[\r\n]+Version: BouncyCastle\.NET Cryptography \(OpenPGP-only, net6\.0\) v2\.0\.0\.1[\r\n]+hI(s|w)DzoB5W4N7pN4B", textResult);
        StringAssert.EndsWith($"-----END PGP MESSAGE-----{Environment.NewLine}", textResult);
    }

    [Test]
    public void EncryptFile_ShouldEncryptWithoutCompression()
    {
        input.EncryptionAlgorithm = PgpEncryptEncryptionAlgorithm.Cast5;
        input.UseCompression = false;
        input.SignWithPrivateKey = true;
        input.SigningSettings = new PgpEncryptSigningSettings
        {
            PrivateKeyFile = Path.Combine(WorkDir, privateKey),
            PrivateKeyPassword = privateKeyPassword,
            SignatureHashAlgorithm = PgpEncryptSignatureHashAlgorithm.Sha256,
        };

        var result = Pgp.EncryptFile(input, options, default);
        string textResult = File.ReadAllText(result.FilePath);

        // result has to start with pgp prefix, version comment and almost static 16 chars
        StringAssert.IsMatch(@"^-----BEGIN PGP MESSAGE-----[\r\n]+Version: BouncyCastle\.NET Cryptography \(OpenPGP-only, net6\.0\) v2\.0\.0\.1[\r\n]+hI(s|w)DzoB5W4N7pN4B", textResult);
        StringAssert.EndsWith($"-----END PGP MESSAGE-----{Environment.NewLine}", textResult);
    }
}
