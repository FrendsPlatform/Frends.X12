using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;
using EdiFabric.Core.Model.Edi;
using EdiFabric.Core.Model.Edi.ErrorContexts;
using EdiFabric.Framework;
using EdiFabric.Framework.Readers;
using Frends.X12.ConvertToJson.Definitions;

namespace Frends.X12.ConvertToJson;

internal static class Parser
{
    internal static string ConvertX12ToXml(
        [PropertyTab] Input input)
    {
        Edifabric.Activation.Activation.Activate();

        X12ReaderSettings x12ReaderSettings = new();
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(input.Edi));
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

    private static Assembly AssemblyFactory(MessageContext messageContext)
    {
        try
        {
            var returnAssembly = Assembly.Load($"Frends.Edifabric.Templates.X12.{messageContext.Version}");
            return returnAssembly;
        }
        catch (Exception ex)
        {
            throw new ArgumentOutOfRangeException(
                $"Version {messageContext.Version} is not supported. See inner exception for details.", ex);
        }
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
        ArgumentNullException.ThrowIfNull(ediItem);

        var serializer = new XmlSerializer(ediItem.GetType());
        var namespaces = new XmlSerializerNamespaces();

        // Add empty namespace to exclude namespaces from output
        namespaces.Add(string.Empty, string.Empty);
        using var ms = new MemoryStream();
        serializer.Serialize(ms, ediItem, namespaces);
        ms.Position = 0;
        return XDocument.Load(ms, LoadOptions.PreserveWhitespace);
    }
}