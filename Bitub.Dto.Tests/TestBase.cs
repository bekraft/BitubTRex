using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

using Microsoft.Extensions.Logging;

using static Bitub.Dto.Xml.XmlSerializationExtensions;

namespace Bitub.Dto.Tests
{
    public abstract class TestBase<T>
    {
        protected ILogger<T> logger;

        private static ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());

        protected void InternallySetup()
        {
            logger = loggerFactory.CreateLogger<T>();
        }

        protected byte[] WriteToXmlStream<E>(E specimen, XmlWriteDelegate<E> writeTo)
        {
            using (var ms = new MemoryStream())
            {
                var writer = XmlWriter.Create(ms);
                writeTo(specimen, writer);                
                return ms.ToArray();
            }
        }

        protected E ReadFromXmlStream<E>(byte[] buffer, XmlReadDelegate<E> readFrom) where E : new()
        {
            using (var ms = new MemoryStream(buffer))
            {
                var reader = XmlReader.Create(ms);
                reader.MoveToContent();
                var obj = new E();
                readFrom(obj, reader);
                return obj;
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
    }
}
