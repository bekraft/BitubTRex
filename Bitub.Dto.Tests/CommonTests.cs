using NUnit.Framework;

using System;
using System.Linq;

using Bitub.Dto.Xml;

namespace Bitub.Dto.Tests
{
    public class CommonTests : BaseTests<CommonTests>
    {
        [SetUp]
        public void StartUp()
        {
            InternallySetup();
        }

        [Test]
        public void QualifierLogic()
        {
            var nq1 = new string[] { "A", "Test1" }.ToQualifier();
            var nq2 = new string[] { "A" }.ToQualifier();
            var nq3 = new string[] { "A", "Test1" }.ToQualifier();
            var nq4 = new string[] { "A", "Test2" }.ToQualifier();

            var aq1 = System.Guid.NewGuid().ToQualifier();

            Assert.IsTrue(nq2.IsSuperQualifierOf(nq1));
            Assert.IsFalse(nq1.IsSuperQualifierOf(nq2));

            Assert.AreEqual(nq1, nq3);

            var q1 = nq1.ToSubQualifierOf(nq2);
            Assert.IsTrue(nq1.IsCompliantTo(q1));
            Assert.AreEqual(1, q1.Named.Frags.Count);
            Assert.AreEqual("Test1", q1.Named.Frags[0]);

            var q2 = nq1.ToCommonRoot(nq4);
            Assert.IsTrue(nq1.IsCompliantTo(q2));
            Assert.AreEqual(1, q2.Named.Frags.Count);
            Assert.AreEqual("A", q2.Named.Frags[0]);
        }

        [Test]
        public void ClassifierLogic()
        {
            var nc1 = new string[] { "A", "Test1" }.ToQualifier().ToClassifier();
            var nq2 = new string[] { "A" }.ToQualifier();
            var nq3 = new string[] { "Test1" }.ToQualifier();

            var q1 = nc1.ToSubQualifiers(nq2).ToArray();
            Assert.AreEqual(1, q1.Length);
            Assert.AreEqual(nq3, q1[0]);
        }

        [Test]
        public void NameAndPathMatching()
        {
            var nc1 = new string[] { "A" }.ToQualifier();
            var nc2 = new string[] { "A", "Test" }.ToQualifier();
            var nc3 = new string[] { "A", "Test", "Of" }.ToQualifier();
            var c1 = new Classifier();
            c1.Path.AddRange(new[] { nc1, nc2, nc3 });

            var r1 = c1.FilterSubNameMatching(nc2).ToArray();
            Assert.AreEqual(2, r1.Length);
            Assert.AreEqual(nc2, r1[0]);
            Assert.AreEqual(nc3, r1[1]);

            var r2 = c1.FilterSubPathMatching(nc2).ToArray();
            Assert.AreEqual(2, r2.Length);
            Assert.AreEqual(nc2, r2[0]);
            Assert.AreEqual(nc3, r2[1]);

            var r3 = c1.FilterSuperPathMatching(nc2).ToArray();
            Assert.AreEqual(2, r3.Length);
            Assert.AreEqual(nc1, r3[0]);
            Assert.AreEqual(nc2, r3[1]);
        }

        [Test]
        public void QualifierXmlRoundtrip()
        {
            var named = new string[] { "A", "Test1" }.ToQualifier();
            var anonymous = System.Guid.NewGuid().ToQualifier();

            var xmlNamed = WriteToXmlStream(named, (o, writer) => writer.WriteOuterXml(o, XmlSerializationExtensions.WriteToXml));
            Assert.IsTrue(xmlNamed.Length > 0);

            Qualifier readNamed = ReadFromXmlStream<Qualifier>(xmlNamed, XmlSerializationExtensions.ReadFromXml);
            Assert.AreEqual(named, readNamed);
        }

        [Test]
        public void ClassifierXmlRoundtrip()
        {
            throw new NotImplementedException();
        }
    }
}
