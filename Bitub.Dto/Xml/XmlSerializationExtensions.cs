using Bitub.Dto.Concept;
using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
 
namespace Bitub.Dto.Xml
{
    public static class XmlSerializationExtensions
    {
        #region´Generalized delegates
        
        public delegate XmlWriter XmlWriteDelegate<E>(E obj, XmlWriter writer);
        public delegate IEnumerable<E> XmlReadDelegate<E>(XmlReader reader);

        #endregion

        #region XML initialization

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

        #endregion

        #region XML extensions for Classifier and Qualifier

        public static IEnumerable<Classifier> ReadClassifierFromXml(this XmlReader reader)
        {
            Classifier mostRecent = new Classifier();
            string hasEntered = null;
            reader.MoveToContent();
            do
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        hasEntered ??= reader.Name;

                        switch (reader.Name)
                        {
                            case nameof(Classifier.Path):
                                mostRecent.Path.AddRange(reader.ReadQualifierFromXml());
                                break;
                        }
                        
                        break;
                    case XmlNodeType.EndElement:
                        if (null == hasEntered)
                            break;

                        if (hasEntered.Equals(reader.Name))
                        {
                            if (mostRecent.Path.Count > 0)
                                yield return mostRecent;
                            // Break
                            yield break;
                        }
                        else
                        {
                            Classifier newClassifier = mostRecent;
                            mostRecent = new Classifier();
                            yield return newClassifier;
                        }

                        break;
                }
            } while (reader.Read());
        }

        public static XmlWriter WriteToXml(this Classifier classifier, XmlWriter writer)
        {
            writer.WriteStartElement(nameof(Classifier.Path));
            foreach (var qualifier in classifier.Path)
                qualifier.WriteToXml(writer);
            writer.WriteEndElement();
            return writer;
        }

        public static IEnumerable<Qualifier> ReadQualifierFromXml(this XmlReader reader)
        {
            string hasEntered = null;
            reader.MoveToContent();
            do
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        hasEntered ??= reader.Name;
                        switch(reader.Name)
                        {
                            case nameof(Qualifier.Anonymous):
                                var id = reader.ReadGlobalUníqueIdFromXml().First();
                                yield return new Qualifier { Anonymous = id };
                                break;
                            case nameof(Qualifier.Named):
                                var name = reader.ReadNameFromXml().First();
                                yield return new Qualifier { Named = name };
                                break;
                        }
                       
                        break;
                    case XmlNodeType.EndElement:
                        if (null == hasEntered)
                            break;

                        if (hasEntered.Equals(reader.Name))
                            yield break;

                        break;
                }
            } while (reader.Read());
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

        public static IEnumerable<Name> ReadNameFromXml(this XmlReader reader)
        {
            string hasEntered = null;
            Name mostRecentName = new Name();
            reader.MoveToContent();
            do
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        hasEntered ??= reader.Name;
                        switch(reader.Name)
                        {
                            case nameof(Name.Frags):
                                if (reader.Read() && XmlNodeType.Text == reader.NodeType)
                                    mostRecentName.Frags.Add(reader.Value);
                                break;
                        }   
                        break;
                    case XmlNodeType.EndElement:
                        if (null == hasEntered)
                            break;
                        
                        var elementName = reader.Name;
                        if (hasEntered.Equals(elementName))
                        {                            
                            if (mostRecentName.Frags.Count > 0)
                                yield return mostRecentName;
                            // Break
                            yield break;
                        } 
                        else if (!nameof(Name.Frags).Equals(elementName))
                        {
                            Name newName = mostRecentName;
                            mostRecentName = new Name();
                            yield return newName;
                        }

                        break;
                }
            } while (reader.Read());            
        }

        public static XmlWriter WriteToXml(this Name name, XmlWriter writer)
        {
            foreach (var frag in name.Frags)
                writer.WriteElementString(nameof(Name.Frags), frag);
            return writer;
        }
              
        public static IEnumerable<GlobalUniqueId> ReadGlobalUníqueIdFromXml(this XmlReader reader)
        {
            string hasEntered = null;
            reader.MoveToContent();
            do
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        hasEntered = hasEntered ?? reader.Name;
                        switch (reader.Name)
                        {
                            case nameof(GlobalUniqueId.Base64):
                                if (reader.Read() && XmlNodeType.CDATA == reader.NodeType)
                                    yield return new GlobalUniqueId { Base64 = reader.Value };
                                break;
                            case nameof(GlobalUniqueId.Guid):
                                if (reader.Read() && XmlNodeType.CDATA == reader.NodeType)
                                    yield return new GlobalUniqueId { Guid = new Guid { Raw = ByteString.FromBase64(reader.Value) } };

                                break;
                        }
                        break;
                    case XmlNodeType.EndElement:
                        if (null == hasEntered)
                            break;
                        
                        if (hasEntered.Equals(reader.Name))
                            yield break;

                        break;
                }
            } while (reader.Read());
        }

        public static XmlWriter WriteToXml(this GlobalUniqueId globalUniqueId, XmlWriter writer)
        {
            switch (globalUniqueId.GuidOrStringCase)
            {
                case GlobalUniqueId.GuidOrStringOneofCase.Base64:
                    writer.WriteStartElement(nameof(GlobalUniqueId.Base64));
                    writer.WriteCData(globalUniqueId.Base64);
                    writer.WriteEndElement();
                    break;
                case GlobalUniqueId.GuidOrStringOneofCase.Guid:
                    writer.WriteStartElement(nameof(GlobalUniqueId.Guid));
                    writer.WriteCData(globalUniqueId.Guid.Raw.ToBase64());
                    writer.WriteEndElement();
                    break;
                default:
                    break;
            }
            return writer;
        }

        #endregion

        #region Canonical filter

        public static IEnumerable<Matcher> ReadMatcherFromXml(this XmlReader reader)
        {
            Matcher recent = null;
            string hasEntered = null;
            do
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        hasEntered ??= reader.Name;

                        switch (reader.Name)
                        {
                            case nameof(Matcher):
                                var strComparisonType = reader.GetAttribute(nameof(StringComparison));
                                var strMatchingType = reader.GetAttribute(nameof(MatchingType));
                                
                                StringComparison stringComparison;
                                if (!Enum.TryParse(strComparisonType, out stringComparison))
                                    throw new XmlException($"Unknown comparison type '{strComparisonType}'");

                                MatchingType matchingType;
                                if (!Enum.TryParse(strMatchingType, out matchingType))
                                    throw new XmlException($"Unknown filter matching type type '{strMatchingType}'");

                                recent = new Matcher(matchingType, stringComparison);
                                break;

                            case nameof(Matcher.Filter):
                                recent?.Filter.AddRange(reader.ReadClassifierFromXml());
                                break;
                        }

                        break;
                    case XmlNodeType.EndElement:
                        if (null == hasEntered)
                            break;

                        var elementName = reader.Name;
                        if (hasEntered.Equals(elementName))
                        {
                            if (null != recent)
                                yield return recent;
                            yield break;
                        }
                        else if (nameof(Matcher).Equals(elementName))
                        {
                            Matcher newFilter = recent;
                            recent = null;
                            yield return newFilter;
                        }
                        
                        break;
                }
            } while (reader.Read());
        }

        public static XmlWriter WriteToXml(this Matcher Matcher, XmlWriter writer)
        {
            writer.WriteStartElement(nameof(Matcher));
            writer.WriteAttributeString(nameof(MatchingType), Matcher.MatchingType.ToString());
            writer.WriteAttributeString(nameof(StringComparison), Matcher.StringComparison.ToString());
            
            if (null != Matcher.Filter)
            {
                writer.WriteStartElement(nameof(Matcher.Filter));
                foreach (var entry in Matcher.Filter)
                    entry.WriteToXml(writer);
                writer.WriteEndElement();
            }

            writer.WriteEndElement();
            return writer;
        }

        #endregion
    }
}
