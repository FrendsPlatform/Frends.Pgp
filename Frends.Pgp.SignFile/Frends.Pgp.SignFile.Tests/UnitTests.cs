using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Frends.Pgp.SignFile.Definitions;
using NUnit.Framework;

namespace Frends.Pgp.SignFile.Tests;

[TestFixture]
public class UnitTests : SignFileTestBase
{
    [Test]
    public void SignFile_ShouldCreateDetachedSignature()
    {
        var result = Pgp.SignFile(Input, Options, CancellationToken.None);

        Assert.That(result.Success, Is.True);
        Assert.That(File.Exists(result.FilePath), Is.True);

        var signatureContent = File.ReadAllText(result.FilePath);
        Assert.That(signatureContent, Does.StartWith("-----BEGIN PGP SIGNATURE-----"));
        Assert.That(signatureContent, Does.EndWith($"-----END PGP SIGNATURE-----{Environment.NewLine}"));
    }

    [Test]
    public void SignFile_ShouldCreateAttachedSignature()
    {
        Input.OutputFilePath = Path.Combine(WorkDir, "message.txt.pgp");
        Options.DetachedSignature = false;

        var result = Pgp.SignFile(Input, Options, CancellationToken.None);

        Assert.That(result.Success, Is.True);
        Assert.That(File.Exists(result.FilePath), Is.True);

        var signedContent = File.ReadAllText(result.FilePath);
        Assert.That(signedContent, Does.StartWith("-----BEGIN PGP MESSAGE-----"));
        Assert.That(signedContent, Does.EndWith($"-----END PGP MESSAGE-----{Environment.NewLine}"));
    }

    [Test]
    public void SignFile_ShouldCreateBinarySignature()
    {
        Options.UseArmor = false;

        var result = Pgp.SignFile(Input, Options, CancellationToken.None);
        var bytes = File.ReadAllBytes(result.FilePath);
        var text = Encoding.UTF8.GetString(bytes);

        Assert.That(text, Does.Not.StartWith("-----BEGIN PGP SIGNATURE-----"));

        bool hasNonAscii = bytes.Any(b => b < 32 || b > 126);
        Assert.That(hasNonAscii, Is.True, "Output should contain binary bytes when UseArmor=false");

        Assert.That(text, Does.Not.Contain("-----END PGP SIGNATURE-----"));
    }

    [Test]
    public void SignFile_ShouldSignWithAllHashAlgorithms(
        [Values(
        PgpSignatureHashAlgorithm.Md2,
        PgpSignatureHashAlgorithm.Md5,
        PgpSignatureHashAlgorithm.RipeMd160,
        PgpSignatureHashAlgorithm.Sha1,
        PgpSignatureHashAlgorithm.Sha224,
        PgpSignatureHashAlgorithm.Sha256,
        PgpSignatureHashAlgorithm.Sha384,
        PgpSignatureHashAlgorithm.Sha512)]
        PgpSignatureHashAlgorithm hashAlgorithm)
    {
        Options.SignatureHashAlgorithm = hashAlgorithm;

        var result = Pgp.SignFile(Input, Options, CancellationToken.None);
        var signatureContent = File.ReadAllText(result.FilePath);

        Assert.That(result.Success, Is.True);
        Assert.That(signatureContent, Does.StartWith("-----BEGIN PGP SIGNATURE-----"));
        Assert.That(signatureContent, Does.Match(@"^-----BEGIN PGP SIGNATURE-----[\r\n]+Version: BouncyCastle"));
        Assert.That(signatureContent, Does.EndWith($"-----END PGP SIGNATURE-----{Environment.NewLine}"));
    }

    [Test]
    public void SignFile_ShouldOverwriteWhenOutputFileExistsAndActionIsOverwrite()
    {
        File.WriteAllText(Input.OutputFilePath, "existing content");
        Input.OutputFileExistsAction = OutputFileExistsAction.Overwrite;

        var result = Pgp.SignFile(Input, Options, CancellationToken.None);

        Assert.That(result.Success, Is.True);
        var signatureContent = File.ReadAllText(result.FilePath);
        Assert.That(signatureContent, Does.StartWith("-----BEGIN PGP SIGNATURE-----"));
        Assert.That(signatureContent, Does.Not.Contain("existing content"));
    }

    [Test]
    public void SignFile_ShouldWorkWithRawKeyString()
    {
        string privateKeyContent = File.ReadAllText(Path.Combine(WorkDir, PrivateKeyFile));

        Options.PrivateKey = privateKeyContent;
        Options.UseFileKey = false;

        var result = Pgp.SignFile(Input, Options, CancellationToken.None);

        Assert.That(result.Success, Is.True);
        var signatureContent = File.ReadAllText(result.FilePath);
        Assert.That(signatureContent, Does.StartWith("-----BEGIN PGP SIGNATURE-----"));
    }

    [Test]
    public void SignFile_DetachedAndAttachedSignaturesShouldBeDifferent()
    {
        var detachedResult = Pgp.SignFile(Input, Options, CancellationToken.None);
        var detachedContent = File.ReadAllText(detachedResult.FilePath);

        Input.OutputFilePath = Path.Combine(WorkDir, "message.txt.pgp");
        Options.DetachedSignature = false;
        var attachedResult = Pgp.SignFile(Input, Options, CancellationToken.None);
        var attachedContent = File.ReadAllText(attachedResult.FilePath);

        Assert.That(detachedContent, Does.StartWith("-----BEGIN PGP SIGNATURE-----"));

        Assert.That(attachedContent, Does.StartWith("-----BEGIN PGP MESSAGE-----"));

        Assert.That(attachedContent.Length, Is.GreaterThan(detachedContent.Length));
    }

    [Test]
    public void SignFile_TestWithoutInputFile()
    {
        Input.SourceFilePath = Path.Combine(WorkDir, "nonexistingfile.txt");
        var ex = Assert.Throws<Exception>(() => Pgp.SignFile(Input, Options, CancellationToken.None));
        Assert.That(ex.Message, Does.StartWith("File to sign does not exist."));

        Input.SourceFilePath = string.Empty;
        ex = Assert.Throws<Exception>(() => Pgp.SignFile(Input, Options, CancellationToken.None));
        Assert.That(ex.Message, Does.StartWith("File to sign does not exist."));
    }

    [Test]
    public void SignFile_TestWithInvalidAndMissingPrivateKey()
    {
        Options.PrivateKey = Path.Combine(WorkDir, "nonexisting.gpg");
        Options.UseFileKey = true;

        var ex = Assert.Throws<Exception>(() => Pgp.SignFile(Input, Options, CancellationToken.None));
        Assert.That(ex.Message, Does.Contain("Private key file not found") | Does.Contain("Could not find file"));

        Options.PrivateKey = "invalid key content";
        Options.UseFileKey = false;

        ex = Assert.Throws<Exception>(() => Pgp.SignFile(Input, Options, CancellationToken.None));
        Assert.That(ex.Message, Does.Contain("Failed to read private key") | Does.Contain("Can't find signing key"));
    }

    [Test]
    public void SignFile_TestWithWrongPassword()
    {
        Options.PrivateKeyPassword = "wrongpassword";

        var ex = Assert.Throws<Exception>(() => Pgp.SignFile(Input, Options, CancellationToken.None));
        Assert.That(ex.Message, Does.Contain("Private key extraction failed"));
    }

    [Test]
    public void SignFile_OutputFileExists()
    {
        _ = Pgp.SignFile(Input, Options, CancellationToken.None);

        Input.OutputFileExistsAction = OutputFileExistsAction.Error;
        var ex = Assert.Throws<Exception>(() => Pgp.SignFile(Input, Options, CancellationToken.None));
        Assert.That(ex.Message, Does.Contain("Output file already exists."));
    }
}
