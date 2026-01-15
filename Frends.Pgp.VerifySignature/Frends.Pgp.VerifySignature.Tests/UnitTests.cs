using System;
using System.IO;
using System.Threading;
using Frends.Pgp.VerifySignature.Definitions;
using NUnit.Framework;

namespace Frends.Pgp.VerifySignature.Tests;

[TestFixture]
public class UnitTests : VerifySignatureTestBase
{
    [Test]
    public void VerifySignature_ShouldVerifyValidDetachedSignature()
    {
        var result = Pgp.VerifySignature(Input, Options, CancellationToken.None);

        Assert.That(result.Success, Is.True);
        Assert.That(result.IsValid, Is.True);
        Assert.That(result.SignerKeyId, Is.Not.Null);
        Assert.That(result.SignerKeyId, Is.Not.Empty);
    }

    [Test]
    public void VerifySignature_ShouldVerifyValidAttachedSignature()
    {
        Input.FilePath = Path.Combine(WorkDir, AttachedSignatureFile);
        Input.SignatureFilePath = null;
        Options.IsDetachedSignature = false;

        var result = Pgp.VerifySignature(Input, Options, CancellationToken.None);

        Assert.That(result.Success, Is.True);
        Assert.That(result.IsValid, Is.True);
        Assert.That(result.SignerKeyId, Is.Not.Null);
    }

    [Test]
    public void VerifySignature_ShouldFailWithTamperedFile()
    {
        string tamperedFile = Path.Combine(WorkDir, "tampered_message.txt");
        File.WriteAllText(tamperedFile, "This is tampered content that doesn't match the signature");
        try
        {
            Input.FilePath = tamperedFile;

            var result = Pgp.VerifySignature(Input, Options, CancellationToken.None);

            Assert.That(result.Success, Is.True);
            Assert.That(result.IsValid, Is.False, "Signature should be invalid for tampered file");
        }
        finally
        {
            if (File.Exists(tamperedFile))
                File.Delete(tamperedFile);
        }
    }

    [Test]
    public void VerifySignature_ShouldWorkWithRawPublicKeyString()
    {
        string publicKeyContent = File.ReadAllText(Path.Combine(WorkDir, PublicKeyFile));

        Options.PublicKey = publicKeyContent;
        Options.UseFileKey = false;

        var result = Pgp.VerifySignature(Input, Options, CancellationToken.None);

        Assert.That(result.Success, Is.True);
        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void VerifySignature_ShouldReturnSignerKeyId()
    {
        var result = Pgp.VerifySignature(Input, Options, CancellationToken.None);

        Assert.That(result.SignerKeyId, Is.Not.Null);
        Assert.That(result.SignerKeyId, Is.Not.Empty);
        Assert.That(result.SignerKeyId, Does.Match("^[0-9A-F]+$"));
    }

    [Test]
    public void VerifySignature_DetachedAndAttachedShouldBothVerify()
    {
        var detachedResult = Pgp.VerifySignature(Input, Options, CancellationToken.None);
        Assert.That(detachedResult.IsValid, Is.True);

        Input.FilePath = Path.Combine(WorkDir, AttachedSignatureFile);
        Input.SignatureFilePath = null;
        Options.IsDetachedSignature = false;

        var attachedResult = Pgp.VerifySignature(Input, Options, CancellationToken.None);
        Assert.That(attachedResult.IsValid, Is.True);

        Assert.That(detachedResult.SignerKeyId, Is.Not.Null);
        Assert.That(attachedResult.SignerKeyId, Is.Not.Null);
    }

    [Test]
    public void VerifySignature_TestWithInvalidAndMissingPublicKey()
    {
        Options.PublicKey = Path.Combine(WorkDir, "nonexisting.asc");
        Options.UseFileKey = true;

        var ex = Assert.Throws<Exception>(() => Pgp.VerifySignature(Input, Options, CancellationToken.None));
        Assert.That(ex.Message, Does.Contain("Public key file not found"));

        Options.PublicKey = "invalid key content";
        Options.UseFileKey = false;

        ex = Assert.Throws<Exception>(() => Pgp.VerifySignature(Input, Options, CancellationToken.None));
        Assert.That(ex.Message, Does.Contain("not found in keyring"));
    }

    [Test]
    public void VerifySignature_TestWithCorruptedSignature()
    {
        string corruptedSigFile = Path.Combine(WorkDir, "corrupted.sig");
        File.WriteAllText(
            corruptedSigFile,
            "-----BEGIN PGP SIGNATURE-----\n        Version: Test\n\n        iQEzBAABCAAdFiEEzoB5W4N7pN4BAQH/CorruptedSignatureDataHere==\n        =abcd\n        -----END PGP SIGNATURE-----");

        try
        {
            Input.SignatureFilePath = corruptedSigFile;
            Options.ThrowErrorOnFailure = false;

            var result = Pgp.VerifySignature(Input, Options, CancellationToken.None);

            Assert.That(result.Success == false);
        }
        finally
        {
            if (File.Exists(corruptedSigFile))
                File.Delete(corruptedSigFile);
        }
    }

    [Test]
    public void VerifySignature_TestWithInvalidSignatureFile()
    {
        string invalidSigFile = Path.Combine(WorkDir, "invalid.sig");
        File.WriteAllText(invalidSigFile, "This is not a valid PGP signature");

        try
        {
            Input.SignatureFilePath = invalidSigFile;

            var ex = Assert.Throws<Exception>(() => Pgp.VerifySignature(Input, Options, CancellationToken.None));
            Assert.That(ex.Message, Does.Contain("Invalid signature file format") | Does.Contain("No signature found"));
        }
        finally
        {
            if (File.Exists(invalidSigFile))
                File.Delete(invalidSigFile);
        }
    }
}