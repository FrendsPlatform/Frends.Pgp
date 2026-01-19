using System;
using System.IO;
using Frends.Pgp.VerifySignature.Definitions;
using NUnit.Framework;

namespace Frends.Pgp.VerifySignature.Tests
{
    public class VerifySignatureTestBase
    {
        // Following keys should not be used on anything except testing as both private key and password are on public GitHub repository.
        protected static readonly string WorkDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../TestData/");
        protected static readonly string PublicKeyFile = "pub.asc";
        protected static readonly string SourceFile = "original_message.txt";
        protected static readonly string DetachedSignatureFile = "message.txt.sig";
        protected static readonly string AttachedSignatureFile = "message.txt.pgp";

        protected Options Options { get; private set; }

        protected Input Input { get; private set; }

        [SetUp]
        public void Setup()
        {
            if (!File.Exists(Path.Combine(WorkDir, SourceFile)))
                throw new FileNotFoundException($"Test file not found: {SourceFile}");

            if (!File.Exists(Path.Combine(WorkDir, PublicKeyFile)))
                throw new FileNotFoundException($"Test file not found: {PublicKeyFile}");

            if (!File.Exists(Path.Combine(WorkDir, DetachedSignatureFile)))
                throw new FileNotFoundException($"Test file not found: {DetachedSignatureFile}.");

            if (!File.Exists(Path.Combine(WorkDir, AttachedSignatureFile)))
                throw new FileNotFoundException($"Test file not found: {AttachedSignatureFile}.");

            Input = new Input
            {
                FilePath = Path.Combine(WorkDir, SourceFile),
                SignatureFilePath = Path.Combine(WorkDir, DetachedSignatureFile),
                IsDetachedSignature = true,
            };

            Options = new Options
            {
                PublicKey = Path.Combine(WorkDir, PublicKeyFile),
                UseFileKey = true,
                ThrowErrorOnFailure = true,
            };
        }
    }
}
