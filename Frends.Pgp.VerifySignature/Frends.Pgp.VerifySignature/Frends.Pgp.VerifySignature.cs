using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using Frends.Pgp.VerifySignature.Definitions;
using Frends.Pgp.VerifySignature.Helpers;
using Org.BouncyCastle.Bcpg.OpenPgp;

namespace Frends.Pgp.VerifySignature;

/// <summary>
/// Task Class for Pgp operations.
/// </summary>
public static class Pgp
{
    /// <summary>
    /// PGP Task for verifying signature.
    /// [Documentation](https://tasks.frends.com/tasks/frends-tasks/Frends-Pgp-VerifySignature)
    /// </summary>
    /// <param name="input">Essential parameters.</param>
    /// <param name="options">Optional parameters.</param>
    /// <param name="cancellationToken">A cancellation token provided by Frends Platform.</param>
    /// <returns>object { bool Success, bool IsValid, string SignerKeyId, object Error { string Message, Exception AdditionalInfo } }</returns>
    public static Result VerifySignature(
        [PropertyTab] Input input,
        [PropertyTab] Options options,
        CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrEmpty(input.FilePath) || !File.Exists(input.FilePath))
                throw new ArgumentException("File to verify does not exist.");

            if (input.IsDetachedSignature)
            {
                if (string.IsNullOrEmpty(input.SignatureFilePath))
                    throw new ArgumentException("Signature file path is required for detached signature verification.");

                if (!File.Exists(input.SignatureFilePath))
                    throw new ArgumentException("Signature file does not exist.");
            }

            bool isValid;
            PgpSignature signature;

            if (input.IsDetachedSignature)
            {
                isValid = VerifyDetachedSignature(
                    input.FilePath,
                    input.SignatureFilePath,
                    options,
                    cancellationToken,
                    out signature);
            }
            else
            {
                isValid = VerifyAttachedSignature(
                    input.FilePath,
                    options,
                    cancellationToken,
                    out signature);
            }

            return new Result(
                true,
                isValid,
                signature?.KeyId.ToString("X"));
        }
        catch (Exception ex)
        {
            return ErrorHandler.Handle(ex, options.ThrowErrorOnFailure, options.ErrorMessageOnFailure);
        }
    }

    private static bool VerifyDetachedSignature(
        string filePath,
        string signatureFilePath,
        Options options,
        CancellationToken cancellationToken,
        out PgpSignature signature)
    {
        signature = null;

        using Stream signatureStream = File.OpenRead(signatureFilePath);
        using Stream decoderStream = PgpUtilities.GetDecoderStream(signatureStream);

        PgpObjectFactory pgpFactory = new PgpObjectFactory(decoderStream);
        PgpObject pgpObject = pgpFactory.NextPgpObject();

        if (pgpObject is PgpSignatureList signatureList)
        {
            if (signatureList.Count == 0)
                throw new Exception("No signature found in signature file.");

            signature = signatureList[0];
        }
        else
        {
            throw new Exception("Invalid signature file format.");
        }

        PgpPublicKey publicKey = PgpVerificationServices.GetPublicKey(
            options.PublicKey,
            options.UseFileKey,
            signature.KeyId);

        signature.InitVerify(publicKey);

        using var fileStream = File.OpenRead(filePath);
        var buffer = new byte[options.SignatureBufferSize * 1024];
        int bytesRead;

        while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();
            signature.Update(buffer, 0, bytesRead);
        }

        bool isValid = signature.Verify();
        return isValid;
    }

    private static bool VerifyAttachedSignature(
        string signedFilePath,
        Options options,
        CancellationToken cancellationToken,
        out PgpSignature signature)
    {
        signature = null;

        using Stream signedStream = File.OpenRead(signedFilePath);
        using Stream decoderStream = PgpUtilities.GetDecoderStream(signedStream);

        PgpObjectFactory pgpFactory = new PgpObjectFactory(decoderStream);
        PgpObject pgpObject = pgpFactory.NextPgpObject();

        PgpOnePassSignature onePassSignature = null;

        if (pgpObject is PgpOnePassSignatureList onePassList)
        {
            if (onePassList.Count == 0)
                throw new Exception("No one-pass signature found.");

            onePassSignature = onePassList[0];
            pgpObject = pgpFactory.NextPgpObject();
        }
        else if (pgpObject is PgpCompressedData compressedDataFirst)
        {
            Stream compressedStream = compressedDataFirst.GetDataStream();
            PgpObjectFactory compressedFactory = new PgpObjectFactory(compressedStream);
            pgpObject = compressedFactory.NextPgpObject();

            if (pgpObject is PgpOnePassSignatureList compressedOnePassList)
            {
                if (compressedOnePassList.Count == 0)
                    throw new Exception("No one-pass signature found.");

                onePassSignature = compressedOnePassList[0];
                pgpObject = compressedFactory.NextPgpObject();
            }

            pgpFactory = compressedFactory;
        }

        if (onePassSignature == null)
            throw new Exception("Invalid signed message format - missing one-pass signature.");

        PgpPublicKey publicKey = PgpVerificationServices.GetPublicKey(
            options.PublicKey,
            options.UseFileKey,
            onePassSignature.KeyId);

        onePassSignature.InitVerify(publicKey);

        if (pgpObject is PgpCompressedData compressedData)
        {
            Stream compressedStream = compressedData.GetDataStream();
            PgpObjectFactory compressedFactory = new PgpObjectFactory(compressedStream);
            pgpObject = compressedFactory.NextPgpObject();
        }

        if (pgpObject is PgpLiteralData literalData)
        {
            using Stream literalStream = literalData.GetInputStream();
            var buffer = new byte[options.SignatureBufferSize * 1024];
            int bytesRead;

            while ((bytesRead = literalStream.Read(buffer, 0, buffer.Length)) > 0)
            {
                cancellationToken.ThrowIfCancellationRequested();
                onePassSignature.Update(buffer, 0, bytesRead);
            }

            pgpObject = pgpFactory.NextPgpObject();

            if (pgpObject is PgpSignatureList signatureList)
            {
                if (signatureList.Count == 0)
                    throw new Exception("No signature found in signed message.");

                signature = signatureList[0];
            }
            else
            {
                throw new Exception("Invalid signed message format - missing signature.");
            }

            return onePassSignature.Verify(signature);
        }
        else
        {
            throw new Exception("Invalid signed message format - missing literal data.");
        }
    }
}
