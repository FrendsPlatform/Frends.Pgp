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
    private const string PrivateKey = "private_key.asc"; // this key should not be used on anything except testing as it is on the public GitHub repository.
    private const string Passphrase = "mat123";

    private static readonly string WorkDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../TestData/");
    private Input input;
    private Options options;

    [SetUp]
    public void Setup()
    {
        if (File.Exists(Path.Combine(WorkDir, DecryptedFile))) File.Delete(Path.Combine(WorkDir, DecryptedFile));
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
        Assert.That(decryptedText, Is.EqualTo(File.ReadAllText(Path.Combine(WorkDir, OriginalMessageFile))));
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
}