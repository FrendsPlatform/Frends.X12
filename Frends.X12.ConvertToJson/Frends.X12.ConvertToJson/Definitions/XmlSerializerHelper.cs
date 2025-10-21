using System;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;

namespace Frends.X12.ConvertToJson.Definitions;

public static class XmlSerializerHelper
{
    public static XmlSerializer CreateWithDataElementNames(Type type)
    {
        var overrides = new XmlAttributeOverrides();

        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var dataElementAttr = prop.GetCustomAttributes()
                .FirstOrDefault(a => a.GetType().Name == "DataElementAttribute");

            if (dataElementAttr == null) continue;

            var codeProp = dataElementAttr.GetType().GetProperty("Id")
                           ?? dataElementAttr.GetType().GetProperty("ElementCode")
                           ?? dataElementAttr.GetType().GetProperty("Code");

            var elementName = codeProp?.GetValue(dataElementAttr)?.ToString();
            if (string.IsNullOrWhiteSpace(elementName)) continue;

            var xmlAttrs = new XmlAttributes();
            xmlAttrs.XmlElements.Add(new XmlElementAttribute(elementName));
            overrides.Add(type, prop.Name, xmlAttrs);
        }

        return new XmlSerializer(type, overrides);
    }
}