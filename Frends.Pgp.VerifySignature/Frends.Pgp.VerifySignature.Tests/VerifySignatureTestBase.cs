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

        private Options _options;
        private Input _input;

        protected Options Options => _options;

        protected Input Input => _input;

        [SetUp]
        public void Setup()
        {
            Console.WriteLine("=== DEBUG INFO ===");
            Console.WriteLine($"WorkDir: {Path.GetFullPath(WorkDir)}");
            Console.WriteLine($"SourceFile exists: {File.Exists(Path.Combine(WorkDir, SourceFile))}");
            Console.WriteLine($"SourceFile size: {new FileInfo(Path.Combine(WorkDir, SourceFile)).Length} bytes");
            Console.WriteLine($"DetachedSig exists: {File.Exists(Path.Combine(WorkDir, DetachedSignatureFile))}");
            Console.WriteLine($"DetachedSig size: {new FileInfo(Path.Combine(WorkDir, DetachedSignatureFile)).Length} bytes");
            Console.WriteLine($"PublicKey exists: {File.Exists(Path.Combine(WorkDir, PublicKeyFile))}");
            Console.WriteLine($"PublicKey size: {new FileInfo(Path.Combine(WorkDir, PublicKeyFile)).Length} bytes");

            // Check file content hashes
            var sourceHash = GetFileHash(Path.Combine(WorkDir, SourceFile));
            var sigHash = GetFileHash(Path.Combine(WorkDir, DetachedSignatureFile));
            var keyHash = GetFileHash(Path.Combine(WorkDir, PublicKeyFile));

            Console.WriteLine($"SourceFile MD5: {sourceHash}");
            Console.WriteLine($"DetachedSig MD5: {sigHash}");
            Console.WriteLine($"PublicKey MD5: {keyHash}");
            Console.WriteLine("==================");


            if (!File.Exists(Path.Combine(WorkDir, SourceFile)))
                throw new FileNotFoundException($"Test file not found: {SourceFile}");

            if (!File.Exists(Path.Combine(WorkDir, PublicKeyFile)))
                throw new FileNotFoundException($"Test file not found: {PublicKeyFile}");

            if (!File.Exists(Path.Combine(WorkDir, DetachedSignatureFile)))
                throw new FileNotFoundException($"Test file not found: {DetachedSignatureFile}.");

            if (!File.Exists(Path.Combine(WorkDir, AttachedSignatureFile)))
                throw new FileNotFoundException($"Test file not found: {AttachedSignatureFile}.");

            _input = new Input
            {
                FilePath = Path.Combine(WorkDir, SourceFile),
                SignatureFilePath = Path.Combine(WorkDir, DetachedSignatureFile),
            };

            _options = new Options
            {
                IsDetachedSignature = true,
                PublicKey = Path.Combine(WorkDir, PublicKeyFile),
                UseFileKey = true,
                ThrowErrorOnFailure = true,
            };
        }

        private string GetFileHash(string filePath)
        {
            using var md5 = System.Security.Cryptography.MD5.Create();
            using var stream = File.OpenRead(filePath);
            var hash = md5.ComputeHash(stream);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
    }
}
