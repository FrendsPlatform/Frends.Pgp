using System;
using System.IO;
using Frends.Pgp.SignFile.Definitions;
using NUnit.Framework;

namespace Frends.Pgp.SignFile.Tests;

[TestFixture]
public class ErrorHandlerTest : SignFileTestBase
{
    private Input input;
    private Connection connection;
    private Options options;

    [SetUp]
    public void SetParams()
    {
        input = GetInput();
        connection = GetConnection();
        options = GetOptions();
    }

    [Test]
    public void SignFile_TestWithoutInputFile()
    {
        input.SourceFilePath = Path.Combine(GetWorkDir(), "nonexistingfile.txt");
        var ex = Assert.Throws<Exception>(() => Pgp.SignFile(input, connection, options, default));
        Assert.That(ex.Message, Does.StartWith("File to sign does not exist."));

        input.SourceFilePath = string.Empty;
        ex = Assert.Throws<Exception>(() => Pgp.SignFile(input, connection, options, default));
        Assert.That(ex.Message, Does.StartWith("File to sign does not exist."));
    }

    [Test]
    public void SignFile_TestErrorWithoutThrowing()
    {
        input.SourceFilePath = Path.Combine(GetWorkDir(), "nonexistingfile.txt");
        options.ThrowErrorOnFailure = false;

        var result = Pgp.SignFile(input, connection, options, default);
        Assert.That(result.Success, Is.False);

        input.SourceFilePath = string.Empty;
        result = Pgp.SignFile(input, connection, options, default);
        Assert.That(result.Success, Is.False);
    }

    [Test]
    public void SignFile_TestErrorHandlerWithCustomMessage()
    {
        input.SourceFilePath = Path.Combine(GetWorkDir(), "nonexistingfile.txt");
        options.ThrowErrorOnFailure = false;
        options.ErrorMessageOnFailure = "Something went wrong during signing.";

        var result = Pgp.SignFile(input, connection, options, default);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Error.Message, Does.Contain("Something went wrong during signing."));

        options.ThrowErrorOnFailure = true;
        var ex = Assert.Throws<Exception>(() => Pgp.SignFile(input, connection, options, default));
        Assert.That(ex.Message, Does.Contain("Something went wrong during signing."));
    }

    [Test]
    public void SignFile_TestWithInvalidAndMissingPrivateKey()
    {
        connection.PrivateKey = Path.Combine(GetWorkDir(), "nonexisting.gpg");
        connection.UseFileKey = true;

        var ex = Assert.Throws<Exception>(() => Pgp.SignFile(input, connection, options, default));
        Assert.That(ex.Message, Does.Contain("Private key file not found") | Does.Contain("Could not find file"));

        connection.PrivateKey = "invalid key content";
        connection.UseFileKey = false;

        ex = Assert.Throws<Exception>(() => Pgp.SignFile(input, connection, options, default));
        Assert.That(ex.Message, Does.Contain("Failed to read private key") | Does.Contain("Can't find signing key"));
    }

    [Test]
    public void SignFile_TestWithWrongPassword()
    {
        connection.PrivateKeyPassword = "wrongpassword";

        var ex = Assert.Throws<Exception>(() => Pgp.SignFile(input, connection, options, default));
        Assert.That(ex.Message, Does.Contain("Private key extraction failed"));
        Assert.That(ex.Message, Does.Contain("password might be incorrect"));
    }

    [Test]
    public void SignFile_TestWithEmptyPassword()
    {
        connection.PrivateKeyPassword = string.Empty;

        var ex = Assert.Throws<Exception>(() => Pgp.SignFile(input, connection, options, default));
        Assert.That(ex.Message, Does.Contain("Private key password is required for signing"));

        connection.PrivateKeyPassword = null;
        ex = Assert.Throws<Exception>(() => Pgp.SignFile(input, connection, options, default));
        Assert.That(ex.Message, Does.Contain("Private key password is required for signing"));
    }

    [Test]
    public void SignFile_OutputFileExists()
    {
        _ = Pgp.SignFile(input, connection, options, default);

        input.OutputFileExistsAction = OutputFileExistsAction.Error;
        var ex = Assert.Throws<Exception>(() => Pgp.SignFile(input, connection, options, default));
        Assert.That(ex.Message, Does.Contain("Output file already exists."));
    }

    [Test]
    public void SignFile_TestCancellationToken()
    {
        using var cts = new System.Threading.CancellationTokenSource();
        cts.Cancel();

        Assert.Throws<Exception>(() =>
            Pgp.SignFile(input, connection, options, cts.Token));
    }

    [Test]
    public void SignFile_TestErrorWithWrongPasswordWithoutThrowing()
    {
        connection.PrivateKeyPassword = "wrongpassword";
        options.ThrowErrorOnFailure = false;

        var result = Pgp.SignFile(input, connection, options, default);
        Assert.That(result.Success, Is.False);
    }
}