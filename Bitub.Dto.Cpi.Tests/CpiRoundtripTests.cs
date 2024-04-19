using NUnit.Framework;

using System.Collections.Generic;

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

            Assert.That(fixture.IsValid, Is.True);
            var serializer = XmlSerializationExtensions.CreateHeadlessUtf8Serializer<ContentSection>();
            var xml = serializer(fixture);

            var deserializer = XmlSerializationExtensions.CreateUtf8Deserializer<ContentSection>();
            var read = deserializer(xml);
            Assert.That(read, Is.Not.Null);
            Assert.That(2, Is.EqualTo(read.Section.Count));
            Assert.That(read.IsValid, Is.True);
        }
    }
}
