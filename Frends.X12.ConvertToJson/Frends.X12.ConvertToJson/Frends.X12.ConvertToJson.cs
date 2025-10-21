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
    /// X12es the input string the specified number of times.
    /// [Documentation](https://tasks.frends.com/tasks/frends-tasks/Frends-X12-ConvertToJson)
    /// </summary>
    /// <param name="input">Essential parameters.</param>
    /// <param name="options">Additional parameters.</param>
    /// <param name="cancellationToken">A cancellation token provided by Frends Platform.</param>
    /// <returns>object { bool Success, string Output, object Error { string Message, dynamic AdditionalInfo } }</returns>
    public static Result ConvertToJson(
        [PropertyTab] Input input,
        [PropertyTab] Options options,
        CancellationToken cancellationToken)
    {
        try
        {
            var xmlResult = ConvertX12ToXml(input);
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
            if (options.ThrowErrorOnFailure)
            {
                if (string.IsNullOrEmpty(options.ErrorMessageOnFailure))
                    throw new Exception(e.Message, e);

                throw new Exception(options.ErrorMessageOnFailure, e);
            }

            var errorMessage = !string.IsNullOrEmpty(options.ErrorMessageOnFailure)
                ? $"{options.ErrorMessageOnFailure}: {e.Message}"
                : e.Message;

            return new Result
            {
                Success = false,
                Json = null,
                Error = new Error
                {
                    Message = errorMessage,
                    AdditionalInfo = new
                    {
                        Exception = e,
                    },
                },
            };
        }
    }

    private static Assembly AssemblyFactory(MessageContext messageContext)
    {
        try
        {
            var returnAssembly = Assembly.Load($"Frends.Edifabric.Templates.X12.{messageContext.Version}");
            return returnAssembly;
        }
        catch (Exception ex)
        {
            throw new ArgumentOutOfRangeException($"Version {messageContext.Version} is not supported. See inner exception for details.", ex);
        }
    }

    private static string ConvertX12ToXml(
        [PropertyTab] Input input)
    {
        Edifabric.Activation.Activation.Activate();

        X12ReaderSettings x12ReaderSettings = new() { };
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(input.InputX12));
        using var ediReader = new X12Reader(stream, AssemblyFactory, x12ReaderSettings);
        var ediItems = ediReader.ReadToEnd().ToList();

        if (ediItems.OfType<ReaderErrorContext>().Any())
        {
            var ex = ediItems.OfType<ReaderErrorContext>().Select(x => x.Exception);
            throw new AggregateException("Error reading edi file", ex);
        }

        var returnValue = ConvertToXml(ediItems);
        return returnValue;
    }

    private static string ConvertToXml(List<IEdiItem> ediItems)
    {
        var xDocument = new XDocument();
        var root = new XElement("X12");
        xDocument.Add(root);

        var ediItemsXml = ediItems.Select(x => Serialize(x));
        root.Add(ediItemsXml.Select(x => x.Elements()));
        return xDocument.ToString();
    }

    private static XDocument Serialize(IEdiItem ediItem)
    {
        if (ediItem == null)
            throw new ArgumentNullException(nameof(ediItem));

        // var serializer = XmlSerializerHelper.CreateWithDataElementNames(ediItem.GetType());
        var serializer = new XmlSerializer(ediItem.GetType());
        XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();

        // Add empty namespace to exclude namespaces from output
        namespaces.Add(string.Empty, string.Empty);
        using var ms = new MemoryStream();
        serializer.Serialize(ms, ediItem, namespaces);
        ms.Position = 0;
        return XDocument.Load(ms, LoadOptions.PreserveWhitespace);
    }
}