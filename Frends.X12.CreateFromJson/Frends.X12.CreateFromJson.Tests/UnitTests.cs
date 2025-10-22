using System;
using System.IO;
using System.Threading;
using Frends.X12.CreateFromJson.Definitions;
using NUnit.Framework;

namespace Frends.X12.CreateFromJson.Tests;

[TestFixture]
public class UnitTests
{
    private const string CustomErrorMessage = "CustomErrorMesasge";
    private static readonly string TestDataDir = Path.Combine(AppContext.BaseDirectory, "TestData");
    private static readonly string SampleJson = File.ReadAllText(Path.Combine(TestDataDir, "sample.json"));

    private static readonly string ExpectedEdi =
        File.ReadAllText(Path.Combine(TestDataDir, "expected.edi")).Replace("\r\n", string.Empty);

    [Test]
    public void Should_Create_EdiString()
    {
        var result = X12.CreateFromJson(DefaultInput(), DefaultOptions(), CancellationToken.None);
        Assert.That(result.Success, Is.True);
        Assert.That(result.Edi, Is.EqualTo(ExpectedEdi));
    }

    [Test]
    public void Should_Throw_Error_When_Json_Is_Not_Serializable()
    {
        var input = DefaultInput();
        input.Json = "Invalid json";
        var ex = Assert.Throws<Exception>(() =>
            X12.CreateFromJson(input, DefaultOptions(), CancellationToken.None));
        Assert.That(ex, Is.Not.Null);
    }

    [Test]
    public void Should_Throw_Error_When_ThrowErrorOnFailure_Is_True()
    {
        var input = DefaultInput();
        input.Json = null;
        var ex = Assert.Throws<Exception>(() =>
            X12.CreateFromJson(input, DefaultOptions(), CancellationToken.None));
        Assert.That(ex, Is.Not.Null);
    }

    [Test]
    public void Should_Return_Failed_Result_When_ThrowErrorOnFailure_Is_False()
    {
        var input = DefaultInput();
        input.Json = null;
        var options = DefaultOptions();
        options.ThrowErrorOnFailure = false;
        var result = X12.CreateFromJson(input, options, CancellationToken.None);
        Assert.That(result.Success, Is.False);
    }

    [Test]
    public void Should_Use_Custom_ErrorMessageOnFailure()
    {
        var input = DefaultInput();
        input.Json = null;
        var options = DefaultOptions();
        options.ErrorMessageOnFailure = CustomErrorMessage;
        var ex = Assert.Throws<Exception>(() =>
            X12.CreateFromJson(input, options, CancellationToken.None));
        Assert.That(ex, Is.Not.Null);
        Assert.That(ex.Message, Contains.Substring(CustomErrorMessage));
    }

    private static Input DefaultInput() => new()
    {
        Json = SampleJson,
    };

    private static Options DefaultOptions() => new()
    {
        ThrowErrorOnFailure = true,
        ErrorMessageOnFailure = string.Empty,
    };
}