using System;
using System.ComponentModel;
using System.Threading;
using Frends.X12.CreateFromJson.Definitions;
using Newtonsoft.Json;

namespace Frends.X12.CreateFromJson;

/// <summary>
/// Task class.
/// </summary>
public static class X12
{
    /// <summary>
    /// Creates X12 Edi string from Json.
    /// [Documentation](https://tasks.frends.com/tasks/frends-tasks/Frends-X12-CreateFromJson)
    /// </summary>
    /// <param name="input">Essential parameters.</param>
    /// <param name="options">Additional parameters.</param>
    /// <param name="cancellationToken">A cancellation token provided by Frends Platform.</param>
    /// <returns>object { bool Success, string Edi, object Error { string Message, Exception AdditionalInfo } }</returns>
    public static Result CreateFromJson(
        [PropertyTab] Input input,
        [PropertyTab] Options options,
        CancellationToken cancellationToken)
    {
        try
        {
            var xml = JsonConvert.DeserializeXmlNode(input.Json) ??
                      throw new FormatException("Could not deserialize input JSON.");
            var result = Parser.CreateX12FromXml(xml.OuterXml, cancellationToken);
            return new Result
            {
                Success = true,
                Edi = result,
                Error = null,
            };
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            return ErrorHandler.Handle(e, options.ThrowErrorOnFailure, options.ErrorMessageOnFailure);
        }
    }
}