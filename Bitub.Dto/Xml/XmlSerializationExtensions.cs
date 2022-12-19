using System;
using System.IO;
using System.Collections.Generic;

using System.Xml;
using System.Xml.Serialization;

namespace Bitub.Dto.Xml
{
    public static class XmlSerializationExtensions
    {
        #region Delegates
        
        public delegate XmlWriter XmlWriteDelegate<E>(E obj, XmlWriter writer);
        public delegate XmlReader XmlReadDelegate<E>(E obj, XmlReader reader);
        
        #endregion

        /// <summary>
        /// A new "headless" UTF-8 serializer omitting namespaces and XML declarations.
        /// </summary>
        /// <typeparam name="T">A root serialization type</typeparam>
        /// <returns></returns>
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

        public static XmlWriter WriteOuterXml<T>(this XmlWriter writer, T obj, XmlWriteDelegate<T> writeTo)
        {
            writer.WriteStartElement(typeof(T).Name);
            writer = writeTo(obj, writer);
            writer.WriteEndElement();
            return writer;
        }

        #region XML extensions for Classifier and Qualifier

        public static XmlReader ReadFromXml(this Classifier classifier, XmlReader reader)
        {
            classifier.Path.Clear();
            bool hasEnteredClassifier = false;
            bool hasEnteredPath = false;
            reader.MoveToContent();
            do
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        if (hasEnteredPath)
                        {
                            var qualifier = new Qualifier();
                            reader = qualifier.ReadFromXml(reader);
                            classifier.Path.Add(qualifier);
                        }
                        else if (typeof(Classifier).Name.Equals(reader.Name))
                        {
                            hasEnteredClassifier = true;
                        }
                        else if (nameof(Classifier.Path).Equals(reader.Name) && hasEnteredClassifier)
                        {
                            hasEnteredPath = true;
                        }
                        else
                            throw new XmlException($"Unexpected element '{reader.Name}'");

                        break;
                    case XmlNodeType.EndElement:
                        if (!hasEnteredClassifier)
                            throw new XmlException($"Structural assertion exception. Not entered {typeof(Qualifier).Name}.");

                        if (typeof(Classifier).Name.Equals(reader.Name))
                        {
                            reader.ReadEndElement();
                            return reader;
                        }

                        break;
                }
            } while (reader.Read());
            throw new XmlException("Unexpected end of XML reader");
        }

        public static XmlWriter WriteToXml(this Classifier classifier, XmlWriter writer)
        {
            foreach (var qualifier in classifier.Path)
                qualifier.WriteToXml(writer);
            return writer;
        }

        public static XmlReader ReadFromXml(this Qualifier qualifier, XmlReader reader)
        {
            qualifier.ClearGuidOrName();
            bool hasEntered = false;
            reader.MoveToContent();
            do
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        if (typeof(Qualifier).Name.Equals(reader.Name))
                        {
                            hasEntered = true;
                        }
                        else if (nameof(Qualifier.Anonymous).Equals(reader.Name))
                        {
                            var globalUniqueId = new GlobalUniqueId();
                            reader = globalUniqueId.ReadFromXml(reader);
                            qualifier.Anonymous = globalUniqueId;
                        }
                        else if (nameof(Qualifier.Named).Equals(reader.Name))
                        {
                            var name = new Name();
                            reader = name.ReadFromXml(reader);
                            qualifier.Named = name;
                        }
                        else
                            throw new XmlException($"Unexpected element '{reader.Name}'");

                        break;
                    case XmlNodeType.EndElement:
                        if (!hasEntered)
                            throw new XmlException($"Structural assertion exception. Not entered {typeof(Qualifier).Name}.");

                        if (typeof(Qualifier).Name.Equals(reader.Name))
                        {
                            reader.ReadEndElement();
                            return reader;
                        }

                        break;
                }
            } while (reader.Read());
            throw new XmlException("Unexpected end of XML reader");
        }

        public static XmlWriter WriteToXml(this Qualifier qualifier, XmlWriter writer)
        {
            switch (qualifier.GuidOrNameCase)
            {
                case Qualifier.GuidOrNameOneofCase.Anonymous:
                    writer.WriteStartElement(nameof(Qualifier.Anonymous));
                    qualifier.Anonymous.WriteToXml(writer);
                    writer.WriteEndElement();
                    break;
                case Qualifier.GuidOrNameOneofCase.Named:
                    writer.WriteStartElement(nameof(Qualifier.Named));
                    qualifier.Named.WriteToXml(writer);
                    writer.WriteEndElement();
                    break;
            }
            return writer;
        }

        public static XmlReader ReadFromXml(this Name name, XmlReader reader)
        {
            string hasEntered = null;
            name.Frags.Clear();
            reader.MoveToContent();
            do
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        hasEntered = null == hasEntered ? reader.Name : hasEntered;
                        switch(reader.Name)
                        {
                            case nameof(Name.Frags):
                                name.Frags.Add(reader.ReadElementContentAsString());
                                break;
                        }   
                        break;
                    case XmlNodeType.EndElement:
                        if (null == hasEntered)
                            throw new XmlException($"Structural assertion exception. Not entered any node.");
                        else
                            if (hasEntered.Equals(reader.Name))
                            {
                                reader.ReadEndElement();
                                return reader;
                            }

                        break;
                }
            } while (reader.Read());
            throw new XmlException("Unexpected end of XML reader");
        }

        public static XmlWriter WriteToXml(this Name name, XmlWriter writer)
        {
            foreach (var frag in name.Frags)
                writer.WriteElementString(nameof(Name.Frags), frag);
            return writer;
        }

        public static XmlReader ReadFromXml(this GlobalUniqueId globalUniqueId, XmlReader reader)
        {
            bool hasEntered = false;
            reader.MoveToContent();
            do
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        if (typeof(GlobalUniqueId).Name.Equals(reader.Name))
                        {
                            hasEntered = true;
                        }
                        else if (nameof(GlobalUniqueId.Base64).Equals(reader.Name))
                        {
                            globalUniqueId.Base64 = reader.Value;
                        }
                        else if (nameof(GlobalUniqueId.Guid).Equals(reader.Name))
                        {
                            System.Guid guid;
                            if (!System.Guid.TryParse(reader.Value, out guid))
                                throw new XmlException($"Cannot parse GUID from '{reader.Value}'");

                            globalUniqueId.Guid = guid.ToDtoGuid();
                        }
                        else
                            throw new XmlException($"Unexpected element '{reader.Name}'");

                        break;
                    case XmlNodeType.EndElement:
                        if (!hasEntered)
                            throw new XmlException($"Structural assertion exception. Not entered {typeof(GlobalUniqueId).Name}.");

                        if (typeof(GlobalUniqueId).Name.Equals(reader.Name))
                        {
                            reader.ReadEndElement();
                            return reader;
                        }

                        break;
                }
            } while (reader.Read());
            throw new XmlException("Unexpected end of XML reader");
        }

        public static XmlWriter WriteToXml(this GlobalUniqueId globalUniqueId, XmlWriter writer)
        {
            switch (globalUniqueId.GuidOrStringCase)
            {
                case GlobalUniqueId.GuidOrStringOneofCase.Base64:
                    writer.WriteElementString(nameof(GlobalUniqueId.Base64), globalUniqueId.Base64);
                    break;
                case GlobalUniqueId.GuidOrStringOneofCase.Guid:
                    writer.WriteElementString(nameof(GlobalUniqueId.Guid), globalUniqueId.Guid.ToString());
                    break;
                default:
                    break;
            }
            return writer;
        }

        #endregion
    }
}
