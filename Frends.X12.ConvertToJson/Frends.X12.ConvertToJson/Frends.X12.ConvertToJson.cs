using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using EdiFabric.Core.Model.Edi;
using EdiFabric.Core.Model.Edi.ErrorContexts;
using EdiFabric.Framework;
using EdiFabric.Framework.Readers;
using Frends.X12.ConvertToJson.Definitions;
using Newtonsoft.Json;
using Formatting = Newtonsoft.Json.Formatting;

namespace Frends.X12.ConvertToJson;

/// <summary>
/// Task class.
/// </summary>
public static class X12
{
    /// <summary>
    /// Converts Json into X12 Edi string.
    /// [Documentation](https://tasks.frends.com/tasks/frends-tasks/Frends-X12-ConvertToJson)
    /// </summary>
    /// <param name="input">Essential parameters.</param>
    /// <param name="options">Additional parameters.</param>
    /// <param name="cancellationToken">A cancellation token provided by Frends Platform.</param>
    /// <returns>object { bool Success, string Json, object Error { string Message, Exception AdditionalInfo } }</returns>
    public static Result ConvertToJson(
        [PropertyTab] Input input,
        [PropertyTab] Options options,
        CancellationToken cancellationToken)
    {
        try
        {
            var xmlResult = Parser.ConvertX12ToXml(input);
            XmlDocument doc = new();
            doc.LoadXml(xmlResult);
            cancellationToken.ThrowIfCancellationRequested();
            var json = JsonConvert.SerializeXmlNode(doc, Formatting.Indented);
            return new Result
            {
                Success = true,
                Json = json,
                Error = null,
            };
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            return ErrorHandler.Handle(e, options.ThrowErrorOnFailure, options.ErrorMessageOnFailure);
        }
    }
}