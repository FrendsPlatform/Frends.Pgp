using System;
using System.Threading;
using Frends.Pgp.DecryptFile.Definitions;
using NUnit.Framework;

namespace Frends.Pgp.DecryptFile.Tests;

[TestFixture]
public class ErrorHandlerTest
{
    private const string CustomErrorMessage = "CustomErrorMessage";

    [Test]
    public void Should_Throw_Error_When_ThrowErrorOnFailure_Is_True()
    {
        var ex = Assert.Throws<Exception>(DecryptAction());
        Assert.That(ex, Is.Not.Null);
    }

    [Test]
    public void Should_Return_Failed_Result_When_ThrowErrorOnFailure_Is_False()
    {
        var options = DefaultOptions();
        options.ThrowErrorOnFailure = false;
        var result = Pgp.DecryptFile(DefaultInput(), options, CancellationToken.None);
        Assert.That(result.Success, Is.False);
    }

    [Test]
    public void Should_Use_Custom_ErrorMessageOnFailure()
    {
        var options = DefaultOptions();
        options.ErrorMessageOnFailure = CustomErrorMessage;
        var ex = Assert.Throws<Exception>(DecryptAction(options));
        Assert.That(ex, Is.Not.Null);
        Assert.That(ex.Message, Contains.Substring(CustomErrorMessage));
    }

    private static Action DecryptAction(Options options = null) =>
        () => Pgp.DecryptFile(DefaultInput(), options ?? DefaultOptions(), CancellationToken.None);

    private static Input DefaultInput() => new()
    {
        SourceFilePath = string.Empty, // Invalid value to cause an exception
    };

    private static Options DefaultOptions() => new()
    {
        ThrowErrorOnFailure = true,
        ErrorMessageOnFailure = string.Empty,
    };
}
