using NUnit.Framework;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Bitub.Dto.Concept;
using System.IO;
using Google.Protobuf.Collections;
using Bitub.Dto.Xml;

namespace Bitub.Dto.Tests.Concept
{
    public class CanonicalFilterTests : TestBase<CanonicalFilterTests>
    {
        protected List<Classifier> filterClassifiers;

        [SetUp]
        public void Setup()
        {
            InternallySetup();
            filterClassifiers = new List<Classifier>();
            var c1 = new Classifier();
            c1.Path.Add(new string[] {"A", "B"}.ToQualifier());
            c1.Path.Add(System.Guid.NewGuid().ToQualifier());
            filterClassifiers.Add(c1);
        }

        [Test]
        public void RoundtripXmlTests()
        {
            var filter = new CanonicalFilter(FilterMatchingType.SubOrEquiv, StringComparison.OrdinalIgnoreCase)
            {
                Filter = filterClassifiers
            };

            var xml = WriteToXmlStream(filter, (o, writer) => writer.WriteOuterXml(o, XmlSerializationExtensions.WriteToXml));
            Assert.IsTrue(xml.Length > 0);
            var xmlString = Encoding.UTF8.GetString(xml);
        }

        [Test]
        public void FilterIsPassingTests()
        {
            
        }
    }
}
