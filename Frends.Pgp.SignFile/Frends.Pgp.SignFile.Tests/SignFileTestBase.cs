using System;
using System.IO;
using Frends.Pgp.SignFile.Definitions;
using NUnit.Framework;

namespace Frends.Pgp.SignFile.Tests
{
    public class SignFileTestBase
    {
        // Following keys should not be used on anything except testing as both private key and password are on public GitHub repository.
        protected static readonly string WorkDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../TestData/");
        protected static readonly string PrivateKeyFile = "privatekey.gpg";
        private static readonly string PrivateKeyPassword = "veijo666";
        private static readonly string SourceFile = "original_message.txt";
        private static readonly string DetachedSignatureFile = "message.txt.sig";
        private static readonly string AttachedSignatureFile = "message.txt.pgp";

        private Options _options;
        private Input _input;

        protected Options Options => _options;

        protected Input Input => _input;

        [SetUp]
        public void Setup()
        {
            DeleteTmpFiles();

            _input = new Input
            {
                SourceFilePath = Path.Combine(WorkDir, SourceFile),
                OutputFilePath = Path.Combine(WorkDir, DetachedSignatureFile),
                OutputFileExistsAction = OutputFileExistsAction.Overwrite,
            };

            _options = new Options
            {
                DetachedSignature = true,
                UseArmor = true,
                SignatureHashAlgorithm = PgpSignatureHashAlgorithm.Sha256,
                ThrowErrorOnFailure = true,
                PrivateKey = Path.Combine(WorkDir, PrivateKeyFile),
                PrivateKeyPassword = PrivateKeyPassword,
                UseFileKey = true,
            };
        }

        [TearDown]
        public void TearDown()
        {
            DeleteTmpFiles();
        }

        private static void DeleteTmpFiles()
        {
            if (File.Exists(Path.Combine(WorkDir, DetachedSignatureFile)))
                File.Delete(Path.Combine(WorkDir, DetachedSignatureFile));

            if (File.Exists(Path.Combine(WorkDir, AttachedSignatureFile)))
                File.Delete(Path.Combine(WorkDir, AttachedSignatureFile));
        }
    }
}