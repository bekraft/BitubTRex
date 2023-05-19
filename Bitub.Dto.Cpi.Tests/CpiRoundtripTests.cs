using NUnit.Framework;

using System.IO;
using System.Collections.Generic;
using System.Xml.Serialization;

using Bitub.Dto.Xml;

namespace Bitub.Dto.Cpi.Tests
{
    public class CpiRoundtripTests
    {
        [Test]
        public void RoundtripContentsSection()
        {
            var fixture = new ContentSection()
            {
                Section = new List<ContentReference>() 
                { 
                    new ContentReference { SectionType = ContentType.objectDataSection },
                    new ContentReference { SectionType = ContentType.objectSection },
                }
            };

            Assert.IsTrue(fixture.IsValid);
            var serializer = XmlSerializationExtensions.CreateHeadlessUtf8Serializer<ContentSection>();
            var xml = serializer(fixture);

            var deserializer = XmlSerializationExtensions.CreateUtf8Deserializer<ContentSection>();
            var read = deserializer(xml);
            Assert.IsNotNull(read);
            Assert.AreEqual(2, read.Section.Count);
            Assert.IsTrue(read.IsValid);
        }
    }
}
