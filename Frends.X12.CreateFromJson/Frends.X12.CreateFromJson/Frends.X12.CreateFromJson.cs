using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using System.Xml.Serialization;
using EdiFabric.Core.Model.Edi;
using EdiFabric.Core.Model.Edi.X12;
using EdiFabric.Framework.Writers;
using Frends.X12.CreateFromJson.Definitions;
using Newtonsoft.Json;

namespace Frends.X12.CreateFromJson;

/// <summary>
/// Task class.
/// </summary>
public static class X12
{
    /// <summary>
    /// X12es the input string the specified number of times.
    /// [Documentation](https://tasks.frends.com/tasks/frends-tasks/Frends-X12-CreateFromJson)
    /// </summary>
    /// <param name="input">Essential parameters.</param>
    /// <param name="connection">Connection parameters.</param>
    /// <param name="options">Additional parameters.</param>
    /// <param name="cancellationToken">A cancellation token provided by Frends Platform.</param>
    /// <returns>object { bool Success, string Output, object Error { string Message, dynamic AdditionalInfo } }</returns>
    // TODO: Remove Connection parameter if the task does not make connections
    public static Result CreateFromJson(
        [PropertyTab] Input input,
        [PropertyTab] Options options,
        CancellationToken cancellationToken)
    {
        try
        {
            var xml = JsonConvert.DeserializeXmlNode(input.Json);
            if (xml == null) throw new FormatException("Cound not deserialize input JSON.");
            var result = CreateX12FromXml(xml.OuterXml, cancellationToken);
            return new Result
            {
                Success = true,
                Output = result,
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
                Output = null,
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

    private static string CreateX12FromXml(
        string xml, CancellationToken cancellationToken)
    {
        Edifabric.Activation.Activation.Activate();

        var ediXml = XElement.Parse(xml);
        using var stream = new MemoryStream();

        // Why we cannot use shorthand using here and must use a using block? Here:
        // X12Writer is Disposable and we *must* call dispose to make sure that
        // it gets properly cleaned up AND to make sure that it flushes all writes
        // to the output MemoryStream. So we need to call Dispose before trying to
        // read data from the stream.
        // PS. Writer also has a Flush method, but it is marked as deprecated.

        using (var writer = new X12Writer(stream))
        {
            Type documentType = DetermineDocumentType(ediXml);
            WriteIsaIfAny(ediXml, writer);
            WriteGsIfAny(ediXml, writer);
            WriteEdiMessages(ediXml, documentType, writer, cancellationToken);
        }

        return ReadStreamToEnd(stream);
    }

    private static string ReadStreamToEnd(MemoryStream stream)
    {
        stream.Position = 0;
        using var reader = new StreamReader(stream, Encoding.Default);
        return reader.ReadToEnd();
    }

    private static Type FindX12MessageType(string x12Version, string typeName, Assembly assembly)
    {
        var returnType = assembly.ExportedTypes.FirstOrDefault(x => x.Name == typeName);
        if (returnType == null)
        {
            throw new ArgumentException(
                $"X12 message type {typeName} was not found in " +
                $"X12 version {x12Version}");
        }

        return returnType;
    }

    private static Assembly LoadX12Version(string x12Version)
    {
        var assemblyName = $"Frends.Edifabric.Templates.X12.{x12Version}";
        try
        {
            return Assembly.Load(assemblyName);
        }
        catch (Exception ex)
        {
            throw new ArgumentOutOfRangeException(
                $"Version {x12Version} is not supported. " +
                $"See inner exception for details.", ex);
        }
    }

    private static Type DetermineDocumentType(XElement x12Xml)
    {
        var gsXEl = x12Xml.DescendantsAndSelf("GS").FirstOrDefault();
        if (gsXEl == null)
            throw new FormatException("Could not find X12 functional group element (GS).");
        var gsHeader = DeserializePart<GS>(gsXEl);
        var x12Version = gsHeader.VersionAndRelease_8;
        var assembly = LoadX12Version(gsHeader.VersionAndRelease_8);
        var tsXEl = x12Xml.DescendantsAndSelf().FirstOrDefault(o => o.Name.LocalName.StartsWith("TS"));
        if (tsXEl == null)
            throw new FormatException("Could not find X12 transaction set element (TS...).");
        var typeName = tsXEl.Name.LocalName;
        var documentType = FindX12MessageType(x12Version, typeName, assembly);
        return documentType;
    }

    private static IEdiItem? DeserializeXElement(Type type, XElement xElement)
    {
        var serializer = new XmlSerializer(type);
        using var reader = xElement.CreateReader();
        var deserialized = (IEdiItem?)serializer.Deserialize(reader);
        return deserialized;
    }

    private static void WriteIsaIfAny(XElement ediXml, X12Writer writer)
    {
        var isaXEl = ediXml.DescendantsAndSelf("ISA").FirstOrDefault();
        if (isaXEl != null)
        {
            var isa = DeserializePart<ISA>(isaXEl);
            writer.Write(isa);
        }
    }

    private static void WriteGsIfAny(XElement ediXml, X12Writer writer)
    {
        var gsXEl = ediXml.DescendantsAndSelf("GS").FirstOrDefault();
        if (gsXEl != null)
        {
            var gs = DeserializePart<GS>(gsXEl);
            writer.Write(gs);
        }
    }

    private static void WriteEdiMessages(
        XElement ediXml,
        Type documentType,
        X12Writer writer,
        CancellationToken cancellationToken)
    {
        var ediMessages = ediXml.DescendantsAndSelf(documentType.Name.ToString());
        foreach (var xElement in ediMessages)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var ediMessage = (EdiMessage?)DeserializeXElement(documentType, xElement);
            if (ediMessage != null) writer.Write(ediMessage);
        }
    }

    private static T DeserializePart<T>(XElement xElement)
    {
        var serializer = new XmlSerializer(typeof(T));
        using var reader = xElement.CreateReader();
        var part = (T?)serializer.Deserialize(reader);
        if (part != null) return part;
        throw new FormatException($"Could not deserialize element {xElement.Name.LocalName}");
    }
}