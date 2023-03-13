using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;

using Microsoft.Extensions.Logging;

using static Bitub.Dto.Xml.XmlSerializationExtensions;

namespace Bitub.Dto.Tests
{
    public abstract class TestBase<T>
    {
        protected ILogger<T> logger;

        private readonly static ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());

        protected TestBase()
        {
            logger = loggerFactory.CreateLogger<T>();
        }

        protected byte[] WriteToXmlStream<E>(E specimen, XmlWriteDelegate<E> writeTo)
        {
            using (var ms = new MemoryStream())
            {
                var writer = XmlWriter.Create(ms);
                writeTo(specimen, writer);
                writer.Flush();
                return ms.ToArray();
            }
        }

        protected IEnumerable<E> ReadFromXmlStream<E>(byte[] buffer, XmlReadDelegate<E> readFrom) where E : new()
        {
            using (var ms = new MemoryStream(buffer))
            {
                var reader = XmlReader.Create(ms);
                reader.MoveToContent();
                return readFrom(reader);
            }
        }

        protected byte[] WriteToXmlStream<E>(E specimen)
        {
            using (var ms = new MemoryStream())
            {
                var serializer = new XmlSerializer(typeof(T));
                serializer.Serialize(ms, specimen);               
                return ms.ToArray();
            }
        }

        protected E ReadFromXmlStream<E>(byte[] buffer) where E : IXmlSerializable
        {
            using (var ms = new MemoryStream(buffer))
            {
                var serializer = new XmlSerializer(typeof(T));
                return (E)serializer.Deserialize(ms);
            }
        }

        protected Stream GetEmbeddedFileStream(string resourceName)
        {
            var name = Assembly.GetExecutingAssembly().GetName().Name;
            return Assembly.GetExecutingAssembly().GetManifestResourceStream($"{name}.Resources.{resourceName}");
        }

        protected string GetUtf8TextFrom(string resourceName)
        {
            using (var fs = GetEmbeddedFileStream(resourceName))
            {
                return GetUtf8TextFrom(fs);
            }
        }

        protected TextReader GetEmbeddedUtf8TextReader(string resourceName)
        {
            return new StreamReader(GetEmbeddedFileStream(resourceName), System.Text.Encoding.UTF8);
        }

        protected string GetUtf8TextFrom(Stream binStream)
        {
            using (var sr = new StreamReader(binStream, System.Text.Encoding.UTF8))
            {
                return sr.ReadToEnd();
            }
        }

        protected string ResolveFilename(string localPath)
        {
            return Path.Combine(ExecutingFullpath, localPath);
        }

        protected string ExecutingFullpath
        {
            get
            {
                string assemblyLocation = Assembly.GetExecutingAssembly().Location;
                return Path.GetDirectoryName(assemblyLocation);
            }
        }

    }
}
