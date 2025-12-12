using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Frends.Pgp.EncryptFile.Definitions;
using NUnit.Framework;

namespace Frends.Pgp.EncryptFile.Tests;

[TestFixture]
public class UnitTests : EncryptFileTestBase
{
    private Input input;
    private Options options;

    [SetUp]
    public void SetParams()
    {
        input = GetInput();
        options = GetOptions();
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
            PrivateKeyFile = Path.Combine(GetWorkDir(), GetPrivateKey()),
            PrivateKeyPassword = GetPrivateKeyPassword(),
        };

        var result = Pgp.EncryptFile(input, options, default);
        string textResult = File.ReadAllText(result.FilePath);

        Assert.That(textResult, Does.StartWith($"-----BEGIN PGP MESSAGE-----\r\nVersion: BouncyCastle.NET Cryptography (OpenPGP-only, net6.0) v2.0.0.1\r\n\r\nhIwDzoB5W4N7pN4B"));
        Assert.That(textResult, Does.EndWith($"-----END PGP MESSAGE-----{Environment.NewLine}"));
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
            PrivateKeyFile = Path.Combine(GetWorkDir(), GetPrivateKey()),
            PrivateKeyPassword = GetPrivateKeyPassword(),
            SignatureHashAlgorithm = signatureHash,
        };

        var result = Pgp.EncryptFile(input, options, default);
        string textResult = File.ReadAllText(result.FilePath);

        // result has to start with pgp prefix, version comment and almost static 16 chars
        Assert.That(textResult, Does.Match(@"^-----BEGIN PGP MESSAGE-----[\r\n]+Version: BouncyCastle\.NET Cryptography \(OpenPGP-only, net6\.0\) v2\.0\.0\.1[\r\n]+hI(s|w)DzoB5W4N7pN4B"));
        Assert.That(textResult, Does.EndWith($"-----END PGP MESSAGE-----{Environment.NewLine}"));
    }

    [Test]
    public void EncryptFile_ShouldEncryptWithoutCompression()
    {
        input.EncryptionAlgorithm = PgpEncryptEncryptionAlgorithm.Cast5;
        input.UseCompression = false;
        input.SignWithPrivateKey = true;
        input.SigningSettings = new PgpEncryptSigningSettings
        {
            PrivateKeyFile = Path.Combine(GetWorkDir(), GetPrivateKey()),
            PrivateKeyPassword = GetPrivateKeyPassword(),
            SignatureHashAlgorithm = PgpEncryptSignatureHashAlgorithm.Sha256,
        };

        var result = Pgp.EncryptFile(input, options, default);
        string textResult = File.ReadAllText(result.FilePath);

        // result has to start with pgp prefix, version comment and almost static 16 chars
        Assert.That(textResult, Does.Match(@"^-----BEGIN PGP MESSAGE-----[\r\n]+Version: BouncyCastle\.NET Cryptography \(OpenPGP-only, net6\.0\) v2\.0\.0\.1[\r\n]+hI(s|w)DzoB5W4N7pN4B"));
        Assert.That(textResult, Does.EndWith($"-----END PGP MESSAGE-----{Environment.NewLine}"));
    }

    [Test]
    public void EncryptFile_TestWithArmor()
    {
        input.UseArmor = false;

        var result = Pgp.EncryptFile(input, options, default);
        var bytes = File.ReadAllBytes(result.FilePath);

        // 1. File should not start with ASCII armor header
        var text = Encoding.UTF8.GetString(bytes);
        Assert.That(text, Does.Not.StartWith("-----BEGIN PGP MESSAGE-----"));

        // 2. File must not be entirely ASCII (binary output always contains non-printable bytes)
        bool hasNonAscii = bytes.Any(b => b < 32 || b > 126);
        Assert.That(hasNonAscii, Is.True, "Output should contain binary bytes when UseArmor=false");

        // 3. File should not contain the ASCII ending footer
        Assert.That(text, Does.Not.Contain("-----END PGP MESSAGE-----"));
    }
}
