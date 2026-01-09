using System;
using System.IO;
using System.Linq;
using System.Text;
using Frends.Pgp.SignFile.Definitions;
using NUnit.Framework;

namespace Frends.Pgp.SignFile.Tests;

[TestFixture]
public class UnitTests : SignFileTestBase
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
    public void SignFile_ShouldCreateDetachedSignature()
    {
        var result = Pgp.SignFile(input, connection, options, default);

        Assert.That(result.Success, Is.True);
        Assert.That(File.Exists(result.FilePath), Is.True);

        var signatureContent = File.ReadAllText(result.FilePath);
        Assert.That(signatureContent, Does.StartWith("-----BEGIN PGP SIGNATURE-----"));
        Assert.That(signatureContent, Does.EndWith($"-----END PGP SIGNATURE-----{Environment.NewLine}"));
    }

    [Test]
    public void SignFile_ShouldCreateAttachedSignature()
    {
        input.OutputFilePath = Path.Combine(GetWorkDir(), "message.txt.pgp");
        options.DetachedSignature = false;

        var result = Pgp.SignFile(input, connection, options, default);

        Assert.That(result.Success, Is.True);
        Assert.That(File.Exists(result.FilePath), Is.True);

        var signedContent = File.ReadAllText(result.FilePath);
        Assert.That(signedContent, Does.StartWith("-----BEGIN PGP MESSAGE-----"));
        Assert.That(signedContent, Does.EndWith($"-----END PGP MESSAGE-----{Environment.NewLine}"));
    }

    [Test]
    public void SignFile_ShouldCreateBinarySignature()
    {
        options.UseArmor = false;

        var result = Pgp.SignFile(input, connection, options, default);
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
        options.SignatureHashAlgorithm = hashAlgorithm;

        var result = Pgp.SignFile(input, connection, options, default);
        var signatureContent = File.ReadAllText(result.FilePath);

        Assert.That(result.Success, Is.True);
        Assert.That(signatureContent, Does.StartWith("-----BEGIN PGP SIGNATURE-----"));
        Assert.That(signatureContent, Does.Match(@"^-----BEGIN PGP SIGNATURE-----[\r\n]+Version: BouncyCastle"));
        Assert.That(signatureContent, Does.EndWith($"-----END PGP SIGNATURE-----{Environment.NewLine}"));
    }

    [Test]
    public void SignFile_ShouldOverwriteWhenOutputFileExistsAndActionIsOverwrite()
    {
        File.WriteAllText(input.OutputFilePath, "existing content");
        input.OutputFileExistsAction = OutputFileExistsAction.Overwrite;

        var result = Pgp.SignFile(input, connection, options, default);

        Assert.That(result.Success, Is.True);
        var signatureContent = File.ReadAllText(result.FilePath);
        Assert.That(signatureContent, Does.StartWith("-----BEGIN PGP SIGNATURE-----"));
        Assert.That(signatureContent, Does.Not.Contain("existing content"));
    }

    [Test]
    public void SignFile_ShouldWorkWithRawKeyString()
    {
        string privateKeyContent = File.ReadAllText(Path.Combine(GetWorkDir(), GetPrivateKeyFile()));

        connection.PrivateKey = privateKeyContent;
        connection.UseFileKey = false;

        var result = Pgp.SignFile(input, connection, options, default);

        Assert.That(result.Success, Is.True);
        var signatureContent = File.ReadAllText(result.FilePath);
        Assert.That(signatureContent, Does.StartWith("-----BEGIN PGP SIGNATURE-----"));
    }

    [Test]
    public void SignFile_DetachedAndAttachedSignaturesShouldBeDifferent()
    {
        var detachedResult = Pgp.SignFile(input, connection, options, default);
        var detachedContent = File.ReadAllText(detachedResult.FilePath);

        input.OutputFilePath = Path.Combine(GetWorkDir(), "message.txt.pgp");
        options.DetachedSignature = false;
        var attachedResult = Pgp.SignFile(input, connection, options, default);
        var attachedContent = File.ReadAllText(attachedResult.FilePath);

        Assert.That(detachedContent, Does.StartWith("-----BEGIN PGP SIGNATURE-----"));

        Assert.That(attachedContent, Does.StartWith("-----BEGIN PGP MESSAGE-----"));

        Assert.That(attachedContent.Length, Is.GreaterThan(detachedContent.Length));
    }
}
