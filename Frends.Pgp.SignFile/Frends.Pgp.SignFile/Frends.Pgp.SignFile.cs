using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using Frends.Pgp.SignFile.Definitions;
using Frends.Pgp.SignFile.Helpers;
using Org.BouncyCastle.Bcpg;
using Org.BouncyCastle.Bcpg.OpenPgp;

namespace Frends.Pgp.SignFile;

/// <summary>
/// Task Class for Pgp operations.
/// </summary>
public static class Pgp
{
    /// <summary>
    /// PGP Task for signing files.
    /// [Documentation](https://tasks.frends.com/tasks/frends-tasks/Frends-Pgp-SignFile)
    /// </summary>
    /// <param name="input">Essential parameters.</param>
    /// <param name="options">Optional parameters.</param>
    /// <param name="cancellationToken">A cancellation token provided by Frends Platform.</param>
    /// <returns>object { bool Success, string FilePath, object Error { string Message, Exception AdditionalInfo } }</returns>
    public static Result SignFile(
        [PropertyTab] Input input,
        [PropertyTab] Options options,
        CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrEmpty(input.SourceFilePath) || !File.Exists(input.SourceFilePath))
                throw new ArgumentException("File to sign does not exist.");

            var inputFile = new FileInfo(input.SourceFilePath);

            if (File.Exists(input.OutputFilePath) && input.OutputFileExistsAction == OutputFileExistsAction.Error)
                throw new ArgumentException("Output file already exists.");

            if (options.DetachedSignature)
            {
                CreateDetachedSignature(inputFile, input.OutputFilePath, options, cancellationToken);
            }
            else
            {
                CreateAttachedSignature(inputFile, input.OutputFilePath, options, cancellationToken);
            }

            return new Result(true, input.OutputFilePath);
        }
        catch (Exception ex)
        {
            return ErrorHandler.Handle(ex, options.ThrowErrorOnFailure, options.ErrorMessageOnFailure);
        }
    }

    private static void CreateDetachedSignature(
        FileInfo inputFile,
        string outputPath,
        Options options,
        CancellationToken cancellationToken)
    {
        using Stream outputStream = File.Create(outputPath);
        using var armoredStream = options.UseArmor ? new ArmoredOutputStream(outputStream) : outputStream;

        var signatureGenerator = PgpServices.InitSignatureGenerator(
            options.PrivateKey,
            options.PrivateKeyPassword,
            options.SignatureHashAlgorithm,
            options.UseFileKey);

        using var inputStream = inputFile.OpenRead();
        var buffer = new byte[options.SignatureBufferSize * 1024];
        int bytesRead;

        while ((bytesRead = inputStream.Read(buffer, 0, buffer.Length)) > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();
            signatureGenerator.Update(buffer, 0, bytesRead);
        }

        PgpSignature signature = signatureGenerator.Generate();
        signature.Encode(armoredStream);
    }

    private static void CreateAttachedSignature(
        FileInfo inputFile,
        string outputPath,
        Options options,
        CancellationToken cancellationToken)
    {
        using Stream outputStream = File.Create(outputPath);
        using var armoredStream = options.UseArmor ? new ArmoredOutputStream(outputStream) : outputStream;

        var signatureGenerator = PgpServices.InitSignatureGeneratorWithOnePass(
            armoredStream,
            options.PrivateKey,
            options.PrivateKeyPassword,
            options.SignatureHashAlgorithm,
            options.UseFileKey);

        var literalDataGenerator = new PgpLiteralDataGenerator();
        using var literalOut = literalDataGenerator.Open(
            armoredStream,
            PgpLiteralData.Binary,
            inputFile.Name,
            inputFile.Length,
            DateTime.UtcNow);

        using var inputStream = inputFile.OpenRead();
        var buffer = new byte[options.SignatureBufferSize * 1024];
        int bytesRead;

        while ((bytesRead = inputStream.Read(buffer, 0, buffer.Length)) > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();
            literalOut.Write(buffer, 0, bytesRead);
            signatureGenerator.Update(buffer, 0, bytesRead);
        }

        literalOut.Close();

        PgpSignature signature = signatureGenerator.Generate();
        signature.Encode(armoredStream);
    }
}
