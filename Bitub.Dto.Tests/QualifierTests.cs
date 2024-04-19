using NUnit.Framework;

using System.Linq;
using Bitub.Dto.Xml;

namespace Bitub.Dto.Tests
{
    public class QualifierTests : TestBase<QualifierTests>
    {       
        public QualifierTests() : base()
        {}

        [Test]
        public void SubSuperQualifierTests()
        {
            var nq1 = new string[] { "A", "Test1" }.ToQualifier();
            var nq2 = new string[] { "A" }.ToQualifier();
            var nq3 = new string[] { "A", "Test1" }.ToQualifier();
            var nq4 = new string[] { "A", "Test2" }.ToQualifier();

            var aq1 = System.Guid.NewGuid().ToQualifier();

            Assert.That(nq2.IsSuperQualifierOf(nq1), Is.True);
            Assert.That(nq1.IsSuperQualifierOf(nq2), Is.False);

            Assert.That(nq1, Is.EqualTo(nq3));

            var q1 = nq1.ToSubQualifierOf(nq2);
            Assert.That(nq1.IsCompliantTo(q1), Is.True);
            Assert.That(1, Is.EqualTo(q1.Named.Frags.Count));
            Assert.That("Test1", Is.EqualTo(q1.Named.Frags[0]));

            var q2 = nq1.ToCommonRoot(nq4);
            Assert.That(nq1.IsCompliantTo(q2), Is.True);
            Assert.That(1, Is.EqualTo(q2.Named.Frags.Count));
            Assert.That("A", Is.EqualTo(q2.Named.Frags[0]));
        }

        [Test]
        public void NameAndPathMatchingTests()
        {
            var nc1 = new string[] { "A" }.ToQualifier();
            var nc2 = new string[] { "A", "Test" }.ToQualifier();
            var nc3 = new string[] { "A", "Test", "Of" }.ToQualifier();
            var c1 = new Classifier();
            c1.Path.AddRange(new[] { nc1, nc2, nc3 });

            var r1 = c1.FilterSubNameMatching(nc2).ToArray();
            Assert.That(2, Is.EqualTo(r1.Length));
            Assert.That(nc2, Is.EqualTo(r1[0]));
            Assert.That(nc3, Is.EqualTo(r1[1]));

            var r2 = c1.FilterSubPathMatching(nc2).ToArray();
            Assert.That(2, Is.EqualTo(r2.Length));
            Assert.That(nc2, Is.EqualTo(r2[0]));
            Assert.That(nc3, Is.EqualTo(r2[1]));

            var r3 = c1.FilterSuperPathMatching(nc2).ToArray();
            Assert.That(2, Is.EqualTo(r3.Length));
            Assert.That(nc1, Is.EqualTo(r3[0]));
            Assert.That(nc2, Is.EqualTo(r3[1]));
        }

        [Test]
        public void RoundtripXmlNamedTests()
        {
            var named = new string[] { "A", "Test1" }.ToQualifier();
            Assert.That(named.ToLabel(), Is.EqualTo("A.Test1"));

            var xmlNamed = WriteToXmlStream(named, (o, writer) => writer.WriteOuterXml(o, XmlSerializationExtensions.WriteToXml));
            Assert.That(xmlNamed.Length > 0, Is.True);                  

            var readNamed = ReadFromXmlStream<Qualifier>(xmlNamed, XmlSerializationExtensions.ReadQualifierFromXml);
            Assert.That(named, Is.EqualTo(readNamed.First()));
        }


        [Test]
        public void RoundtripXmlAnonymousTests()
        {
            var anonymous = System.Guid.NewGuid().ToQualifier();

            var xmlAnonymous = WriteToXmlStream(anonymous, (o, writer) => writer.WriteOuterXml(o, XmlSerializationExtensions.WriteToXml));
            Assert.That(xmlAnonymous.Length > 0, Is.True);            

            var readAnonymous = ReadFromXmlStream<Qualifier>(xmlAnonymous, XmlSerializationExtensions.ReadQualifierFromXml);
            Assert.That(anonymous, Is.EqualTo(readAnonymous.First()));
        }
    }
}
