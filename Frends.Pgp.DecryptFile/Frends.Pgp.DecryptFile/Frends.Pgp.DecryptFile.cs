using System;
using System.ComponentModel;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using Frends.Pgp.DecryptFile.Definitions;
using Frends.Pgp.DecryptFile.Helpers;
using Org.BouncyCastle.Bcpg.OpenPgp;

namespace Frends.Pgp.DecryptFile;

/// <summary>
/// Task Class for Pgp operations.
/// </summary>
public static class Pgp
{
    /// <summary>
    /// Task for decrypting a file with Pgp
    /// [Documentation](https://tasks.frends.com/tasks/frends-tasks/Frends-Pgp-DecryptFile)
    /// </summary>
    /// <param name="input">Essential parameters.</param>
    /// <param name="options">Additional parameters.</param>
    /// <param name="cancellationToken">A cancellation token provided by Frends Platform.</param>
    /// <returns>object { bool Success, object Error { string Message, Exception AdditionalInfo } }</returns>
    public static Result DecryptFile(
        [PropertyTab] Input input,
        [PropertyTab] Options options,
        CancellationToken cancellationToken)
    {
        var tempFilePath = input.OutputFilePath + Guid.NewGuid() + ".tmp";

        try
        {
            if (string.IsNullOrEmpty(input.SourceFilePath) || !File.Exists(input.SourceFilePath))
                throw new ArgumentException("File to decrypt does not exist.");

            if (File.Exists(input.OutputFilePath))
                throw new ArgumentException("Output file already exists.");

            using var inputStream = File.OpenRead(input.SourceFilePath);
            using var decoderStream = PgpUtilities.GetDecoderStream(inputStream);
            var pgpFactory = new PgpObjectFactory(decoderStream);
            var pgpObject = pgpFactory.NextPgpObject();

            // first object can be marker
            if (pgpObject is not PgpEncryptedDataList) pgpObject = pgpFactory.NextPgpObject();

            var encList = (PgpEncryptedDataList)pgpObject ?? throw new ArgumentException("Source file is not a valid PGP encrypted file.");

            PgpPrivateKey privateKey = null;
            PgpPublicKeyEncryptedData encryptedData = null;

            foreach (var pgpEncryptedData in encList.GetEncryptedDataObjects())
            {
                var data = (PgpPublicKeyEncryptedData)pgpEncryptedData;
                privateKey = PgpService.FindPrivateKey(input, data.KeyId);

                if (privateKey == null) continue;

                encryptedData = data;

                break;
            }

            if (privateKey == null || encryptedData == null) throw new ArgumentException("Matching private key not found.");

            using var clearStream = encryptedData.GetDataStream(privateKey);
            var plainFactory = new PgpObjectFactory(clearStream);

            var message = plainFactory.NextPgpObject();

            // handle compression
            if (message is PgpCompressedData compressedData)
            {
                using var compressedStream = compressedData.GetDataStream();
                plainFactory = new PgpObjectFactory(compressedStream);
                message = plainFactory.NextPgpObject();
            }

            if (message is not PgpLiteralData literalData)
                throw new ArgumentException("Invalid PGP data.");

            using var literalStream = literalData.GetInputStream();
            using var outputStream = File.OpenWrite(tempFilePath);

            var buffer = new byte[input.DecryptBufferSize * 1024];
            int read;

            while ((read = literalStream.Read(buffer, 0, buffer.Length)) > 0)
            {
                cancellationToken.ThrowIfCancellationRequested();
                outputStream.Write(buffer, 0, read);
            }

            outputStream.Close();

            if (encryptedData.IsIntegrityProtected() && !encryptedData.Verify())
                throw new CryptographicException("PGP integrity check failed.");
            File.Move(tempFilePath, input.OutputFilePath);

            return new Result { Success = true };
        }
        catch (Exception ex)
        {
            if (File.Exists(tempFilePath)) File.Delete(tempFilePath);
            var exception = ex;
            if (ex.Message.Contains("Checksum mismatch"))
                exception = new Exception("Private key passphrase is invalid.", ex);

            return ErrorHandler.Handle(exception, options.ThrowErrorOnFailure, options.ErrorMessageOnFailure);
        }
    }
}