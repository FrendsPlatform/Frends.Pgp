using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading;
using Frends.Pgp.EncryptFile.Definitions;
using Frends.Pgp.EncryptFile.Helpers;
using Org.BouncyCastle.Bcpg;
using Org.BouncyCastle.Bcpg.OpenPgp;

namespace Frends.Pgp.EncryptFile;

/// <summary>
/// Task Class for Pgp operations.
/// </summary>
public static class Pgp
{
    static Pgp()
    {
        var currentAssembly = Assembly.GetExecutingAssembly();
        var currentContext = AssemblyLoadContext.GetLoadContext(currentAssembly);
        if (currentContext != null)
            currentContext.Unloading += OnPluginUnloadingRequested;
    }

    /// <summary>
    /// Encrypts a file using public or private key. Additionally can be configured to use key rings.
    /// [Documentation](https://tasks.frends.com/tasks/frends-tasks/Frends-Pgp-EncryptFile)
    /// </summary>
    /// <param name="input">Essential parameters.</param>
    /// <param name="options">Additional parameters.</param>
    /// <param name="cancellationToken">A cancellation token provided by Frends Platform.</param>
    /// <returns>object { bool Success, string FilePath, Error Error { string Message, Exception AdditionalInfo} }</returns>
    public static Result EncryptFile(
        [PropertyTab] Input input,
        [PropertyTab] Options options,
        CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrEmpty(input.SourceFilePath) || !File.Exists(input.SourceFilePath))
                throw new ArgumentException("File to encrypt does not exists.");

            var inputFile = new FileInfo(input.SourceFilePath);

            // destination file
            using (Stream outputStream = File.OpenWrite(input.OutputFilePath))

            // ascii output?
            using (var armoredStream = input.UseArmor ? new ArmoredOutputStream(outputStream) : outputStream)
            using (var encryptedOut = PgpServices.GetEncryptionStream(armoredStream, input))
            using (var compressedOut = PgpServices.GetCompressionStream(encryptedOut, input))
            {
                // signature init - if necessary
                var signatureGenerator = input.SignWithPrivateKey ? PgpServices.InitPgpSignatureGenerator(compressedOut, input) : null;

                // writing to configured output
                var literalDataGenerator = new PgpLiteralDataGenerator();
                using (var literalOut = literalDataGenerator.Open(compressedOut, PgpLiteralData.Binary, inputFile.Name, inputFile.Length, DateTime.Now))
                using (var inputStream = inputFile.OpenRead())
                {
                    var buf = new byte[input.EncryptBufferSize * 1024];
                    int len;

                    while ((len = inputStream.Read(buf, 0, buf.Length)) > 0)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        literalOut.Write(buf, 0, len);
                        if (input.SignWithPrivateKey)
                            signatureGenerator.Update(buf, 0, len);
                    }

                    if (input.SignWithPrivateKey)
                        signatureGenerator.Generate().Encode(compressedOut);
                }
            }

            return new Result(true, input.OutputFilePath);
        }
        catch (Exception ex)
        {
            return ErrorHandler.Handle(ex, options.ThrowErrorOnFailure, options.ErrorMessageOnFailure);
        }
    }

    private static void OnPluginUnloadingRequested(AssemblyLoadContext obj)
    {
        // Dispose resources
        // Unwire event
        obj.Unloading -= OnPluginUnloadingRequested;
    }
}