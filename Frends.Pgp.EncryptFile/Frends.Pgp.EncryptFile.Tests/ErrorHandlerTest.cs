using System;
using System.IO;
using Frends.Pgp.EncryptFile.Definitions;
using NUnit.Framework;

namespace Frends.Pgp.EncryptFile.Tests;

[TestFixture]
public class ErrorHandlerTest: EncryptFileTestBase
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
    public void EncryptFile_TestWithoutInputFile()
    {
        input = GetInput();
        input.SourceFilePath = Path.Combine(GetWorkDir(), "nonexistingfile.txt");

        var ex = Assert.Throws<Exception>(() => Pgp.EncryptFile(input, options, default));

        Assert.That(ex.Message, Does.StartWith("File to encrypt does not exists."));

        input.SourceFilePath = string.Empty;

        ex = Assert.Throws<Exception>(() => Pgp.EncryptFile(input, options, default));

        Assert.That(ex.Message, Does.StartWith("File to encrypt does not exists."));
    }

    [Test]
    public void EncryptFile_TestErrorWithoutThrowing()
    {
        input = GetInput();
        input.SourceFilePath = Path.Combine(GetWorkDir(), "nonexistingfile.txt");

        options = GetOptions();
        options.ThrowErrorOnFailure = false;

        var result = Pgp.EncryptFile(input, options, default);
        Assert.That(result.Success, Is.False);

        input.SourceFilePath = string.Empty;

        result = Pgp.EncryptFile(input, options, default);
        Assert.That(result.Success, Is.False);
    }

    [Test]
    public void EncryptFile_TestErrorHandlerWithCustomMessage()
    {
        input = GetInput();
        input.SourceFilePath = Path.Combine(GetWorkDir(), "nonexistingfile.txt");

        options = GetOptions();
        options.ThrowErrorOnFailure = false;
        options.ErrorMessageOnFailure = "Something went wrong.";

        var result = Pgp.EncryptFile(input, options, default);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Error.Message, Does.Contain("Something went wrong."));

        options.ThrowErrorOnFailure = true;

        var ex = Assert.Throws<Exception>(() => Pgp.EncryptFile(input, options, default));
        Assert.That(ex.Message, Does.Contain("Something went wrong."));
    }

    [Test]
    public void EncryptFile_TestWithInvalidAndMissingPublicKey()
    {
        input = GetInput();
        input.PublicKeyID = 1;

        options = GetOptions();

        var ex = Assert.Throws<Exception>(() => Pgp.EncryptFile(input, options, default));
        Assert.That(ex.Message, Does.Match($"No public key found with Key ID {input.PublicKeyID}"));

        input.PublicKeyFile = Path.Combine(GetWorkDir(), "nonexisting.asc");

        ex = Assert.Throws<Exception>(() => Pgp.EncryptFile(input, options, default));
        Assert.That(ex.Message, Does.Contain($"Could not find file"));
    }
}
