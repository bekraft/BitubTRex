using System;
using System.IO;
using System.Collections.Generic;

using System.Xml;
using System.Xml.Serialization;

namespace Bitub.Dto.Xml
{
    public static class XmlSerializationExtensions
    {
        public static Func<T, string> CreateHeadlessUtf8Serializer<T>()
        {
            var serializer = new XmlSerializer(typeof(T));
            var ns = new XmlSerializerNamespaces();
            ns.Add(string.Empty, string.Empty);

            return (data) =>
            {
                using (var ms = new MemoryStream())
                {
                    serializer.Serialize(CreateHeadlessXml(ms), data, ns);
                    return System.Text.Encoding.UTF8.GetString(ms.ToArray());
                }
            };
        }

        public static Func<string, T> CreateUtf8Deserializer<T>()
        {
            var serializer = new XmlSerializer(typeof(T));
            return (data) =>
            {
                using (var ms = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(data)))
                {
                    if (serializer.Deserialize(ms) is T obj)
                    {
                        return obj;
                    }
                    else
                    {
                        return default(T);
                    }
                }
            };
        }

        public static XmlWriter CreateHeadlessXml(this Stream outStream)
        {
            return XmlWriter.Create(outStream, new XmlWriterSettings
            {
                Indent = false,
                OmitXmlDeclaration = true,
                Encoding = System.Text.Encoding.UTF8,
                NewLineChars = ""
            });
        }

        public static XmlWriter CreateHeadlessXml(this StringWriter writer)
        {
            return XmlWriter.Create(writer, new XmlWriterSettings
            {
                Indent = false,
                OmitXmlDeclaration = true,
                Encoding = System.Text.Encoding.UTF8,
                NewLineChars = ""
            });
        }

    }
}
