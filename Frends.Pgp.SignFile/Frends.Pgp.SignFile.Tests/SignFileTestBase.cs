using System;
using System.IO;
using Frends.Pgp.SignFile.Definitions;
using NUnit.Framework;

namespace Frends.Pgp.SignFile.Tests
{
    public class SignFileTestBase
    {
        // Following keys should not be used on anything except testing as both private key and password are on public GitHub repository.
        private static readonly string WorkDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../TestData/");
        private static readonly string PrivateKeyFile = "privatekey.gpg";
        private static readonly string PrivateKeyPassword = "veijo666";
        private static readonly string SourceFile = "original_message.txt";
        private static readonly string DetachedSignatureFile = "message.txt.sig";
        private static readonly string AttachedSignatureFile = "message.txt.pgp";

        private Connection connection;
        private Options options;
        private Input input;

        [SetUp]
        public void Setup()
        {
            input = new Input
            {
                SourceFilePath = Path.Combine(WorkDir, SourceFile),
                OutputFilePath = Path.Combine(WorkDir, DetachedSignatureFile),
                OutputFileExistsAction = OutputFileExistsAction.Overwrite,
            };

            connection = new Connection
            {
                PrivateKey = Path.Combine(WorkDir, PrivateKeyFile),
                PrivateKeyPassword = PrivateKeyPassword,
                UseFileKey = true,
            };

            options = new Options
            {
                DetachedSignature = true,
                UseArmor = true,
                SignatureHashAlgorithm = PgpSignatureHashAlgorithm.Sha256,
                ThrowErrorOnFailure = true,
            };
        }

        [TearDown]
        public void DeleteTmpFiles()
        {
            if (File.Exists(Path.Combine(WorkDir, DetachedSignatureFile)))
                File.Delete(Path.Combine(WorkDir, DetachedSignatureFile));

            if (File.Exists(Path.Combine(WorkDir, AttachedSignatureFile)))
                File.Delete(Path.Combine(WorkDir, AttachedSignatureFile));
        }

        protected string GetWorkDir() => WorkDir;

        protected string GetPrivateKeyFile() => PrivateKeyFile;

        protected Input GetInput() => input;

        protected Connection GetConnection() => connection;

        protected Options GetOptions() => options;
    }
}
