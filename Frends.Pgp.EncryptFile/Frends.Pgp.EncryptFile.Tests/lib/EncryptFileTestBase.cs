using System;
using System.IO;
using Frends.Pgp.EncryptFile.Definitions;
using NUnit.Framework;

namespace Frends.Pgp.EncryptFile.Tests;

public class EncryptFileTestBase
{
    // following keys should not be used on anything except testing as both private key and password are on public GitHub repository.
    private static readonly string WorkDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../TestData/");
    private static readonly string PublicKeyFile = "pub.asc";
    private static readonly string SourceFile = "original_message.txt";
    private static readonly string EncryptedFile = "encrypted_message.pgp";

    private readonly string privateKey = "privatekey.gpg";
    private readonly string privateKeyPassword = "veijo666";

    private Options options;

    private Input input;

    [SetUp]
    public void Setup()
    {
        input = new Input
        {
            SourceFilePath = Path.Combine(WorkDir, SourceFile),
            OutputFilePath = Path.Combine(WorkDir, EncryptedFile),
            PublicKeyFile = Path.Combine(WorkDir, PublicKeyFile),
        };

        options = new Options
        {
            ThrowErrorOnFailure = true,
            UseIntegrityCheck = true,
            UseArmor = true,
            UseCompression = true,
            EncryptBufferSize = 64,
        };
    }

    [TearDown]
    public void DeleteTmpFile()
    {
        if (File.Exists(Path.Combine(WorkDir, EncryptedFile)))
            File.Delete(Path.Combine(WorkDir, EncryptedFile));
    }

    protected string GetWorkDir() => WorkDir;

    protected string GetPrivateKey() => privateKey;

    protected string GetPrivateKeyPassword() => privateKeyPassword;

    protected Input GetInput() => input;

    protected Options GetOptions() => options;
}
