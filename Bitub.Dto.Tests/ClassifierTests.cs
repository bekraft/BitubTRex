using NUnit.Framework;

using System;
using System.Linq;

using Bitub.Dto.Xml;

namespace Bitub.Dto.Tests
{
    public class ClassifierTests : TestBase<ClassifierTests>
    {
        [SetUp]
        public void StartUp()
        {
            InternallySetup();
        }

        [Test]
        public void RoundtripXmlTests()
        {
            var classifier = new Classifier();
            Enumerable.Range(1, 10).ForEach(_ => classifier.Path.Add(System.Guid.NewGuid().ToQualifier()));

            var xml = WriteToXmlStream(classifier, (o, writer) => writer.WriteOuterXml(o, XmlSerializationExtensions.WriteToXml));
            Assert.IsTrue(xml.Length > 0);

            var read = ReadFromXmlStream<Classifier>(xml, XmlSerializationExtensions.ReadClassifierFromXml);
            Assert.AreEqual(classifier, read.First());
        }

        [Test]
        public void SubSuperClassifierTests()
        {
            var nc1 = new string[] { "A", "Test1" }.ToQualifier().ToClassifier();
            var nq2 = new string[] { "A" }.ToQualifier();
            var nq3 = new string[] { "Test1" }.ToQualifier();

            var q1 = nc1.ToSubQualifiers(nq2).ToArray();
            Assert.AreEqual(1, q1.Length);
            Assert.AreEqual(nq3, q1[0]);
        }
    }
}
