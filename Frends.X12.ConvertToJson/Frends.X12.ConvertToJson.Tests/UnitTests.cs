using System;
using System.IO;
using System.Threading;
using Frends.X12.ConvertToJson.Definitions;
using NUnit.Framework;

namespace Frends.X12.ConvertToJson.Tests;

[TestFixture]
public class UnitTests
{
    private const string CustomErrorMessage = "CustomErrorMessage";
    private static readonly string TestDataDir = Path.Combine(AppContext.BaseDirectory, "TestData");
    private static readonly string SampleEdi = File.ReadAllText(Path.Combine(TestDataDir, "sample.edi"));

    private static readonly string ExpectedJson =
        File.ReadAllText(Path.Combine(TestDataDir, "expected.json"));

    [Test]
    public void Should_Create_JsonString()
    {
        var result = X12.ConvertToJson(DefaultInput(), DefaultOptions(), CancellationToken.None);
        Assert.That(result.Success, Is.True);
        Assert.That(result.Json, Is.EqualTo(ExpectedJson));
    }

    [Test]
    public void Should_Throw_Error_When_Edi_Is_Invalid()
    {
        var input = DefaultInput();
        input.Edi = "Invalid input";
        var ex = Assert.Throws<Exception>(() =>
            X12.ConvertToJson(input, DefaultOptions(), CancellationToken.None));
        Assert.That(ex, Is.Not.Null);
    }

    [Test]
    public void Should_Throw_Error_When_ThrowErrorOnFailure_Is_True()
    {
        var input = DefaultInput();
        input.Edi = null;
        var ex = Assert.Throws<Exception>(() =>
            X12.ConvertToJson(input, DefaultOptions(), CancellationToken.None));
        Assert.That(ex, Is.Not.Null);
    }

    [Test]
    public void Should_Return_Failed_Result_When_ThrowErrorOnFailure_Is_False()
    {
        var input = DefaultInput();
        input.Edi = null;
        var options = DefaultOptions();
        options.ThrowErrorOnFailure = false;
        var result = X12.ConvertToJson(input, options, CancellationToken.None);
        Assert.That(result.Success, Is.False);
    }

    [Test]
    public void Should_Use_Custom_ErrorMessageOnFailure()
    {
        var input = DefaultInput();
        input.Edi = null;
        var options = DefaultOptions();
        options.ErrorMessageOnFailure = CustomErrorMessage;
        var ex = Assert.Throws<Exception>(() =>
            X12.ConvertToJson(input, options, CancellationToken.None));
        Assert.That(ex, Is.Not.Null);
        Assert.That(ex.Message, Contains.Substring(CustomErrorMessage));
    }

    private static Input DefaultInput() => new()
    {
        Edi = SampleEdi,
    };

    private static Options DefaultOptions() => new()
    {
        ThrowErrorOnFailure = true,
        ErrorMessageOnFailure = string.Empty,
    };
}