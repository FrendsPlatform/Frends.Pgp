using System;
using System.IO;
using System.Threading;
using Frends.Pgp.DecryptFile.Definitions;
using NUnit.Framework;

namespace Frends.Pgp.DecryptFile.Tests;

[TestFixture]
public class UnitTests
{
    private const string OriginalMessageFile = "original_message.txt";
    private const string DecryptedFile = "decrypted.txt";
    private const string EncryptedFile = "encrypted.gpg";
    private const string SignedEncryptedFile = "signed_encrypted.gpg";

    private const string OriginalMessageUtf8File = "original_message_utf8.txt";
    private const string DecryptedUtf8File = "decrypted_utf8.txt";
    private const string EncryptedUtf8File = "encrypted_utf8.gpg";
    private const string PrivateKeyUtf8 = "private_key_utf8.asc"; // testing only, public GitHub repo
    private const string PassphraseUtf8 = "test123ä";

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
        if (File.Exists(Path.Combine(WorkDir, DecryptedUtf8File))) File.Delete(Path.Combine(WorkDir, DecryptedUtf8File));
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

        input.SourceFilePath = signedEncryptedPath;

        var result = Pgp.DecryptFile(input, options, CancellationToken.None);

        Assert.That(result.Success, Is.True);
        Assert.That(File.Exists(Path.Combine(WorkDir, DecryptedFile)), Is.True);

        var decryptedText = File.ReadAllText(Path.Combine(WorkDir, DecryptedFile));
        Assert.That(
            NormalizeLineEndings(decryptedText),
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

    [Test]
    public void DecryptFile_With_NonAscii_Passphrase_Runs_Correctly()
    {
        var utf8Input = new Input
        {
            SourceFilePath = Path.Combine(WorkDir, EncryptedUtf8File),
            OutputFilePath = Path.Combine(WorkDir, DecryptedUtf8File),
            PrivateKeyPath = Path.Combine(WorkDir, PrivateKeyUtf8),
            PrivateKeyPassphrase = PassphraseUtf8,
            DecryptBufferSize = 64,
        };

        if (File.Exists(utf8Input.OutputFilePath)) File.Delete(utf8Input.OutputFilePath);

        var result = Pgp.DecryptFile(utf8Input, options, CancellationToken.None);

        Assert.That(result.Success, Is.True, $"Decryption failed: {result.Error?.Message}");
        Assert.That(File.Exists(utf8Input.OutputFilePath), Is.True);
        var decryptedText = File.ReadAllText(utf8Input.OutputFilePath);
        Assert.That(
            NormalizeLineEndings(decryptedText),
            Is.EqualTo(NormalizeLineEndings(File.ReadAllText(Path.Combine(WorkDir, OriginalMessageUtf8File)))));
    }

    [TearDown]
    public void TearDown()
    {
        if (File.Exists(Path.Combine(WorkDir, DecryptedFile))) File.Delete(Path.Combine(WorkDir, DecryptedFile));
        if (File.Exists(Path.Combine(WorkDir, DecryptedUtf8File))) File.Delete(Path.Combine(WorkDir, DecryptedUtf8File));
    }

    private static string NormalizeLineEndings(string value) => value.Replace("\r\n", "\n").Replace("\r", "\n");
}
