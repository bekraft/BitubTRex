using NUnit.Framework;

using System.Linq;

using Bitub.Dto.BcfXml;

namespace Bitub.Dto.Bcf.Tests
{
    [TestFixture]
    public class BcfXmlTests : TestBase<BcfXmlTests>
    {
        public BcfXmlTests() : base()
        { }

        [Test]
        public void ReadExample1()
        {
            var fixture = GetEmbeddedFileStream("Bcf21.Example1.bcfzip");
            Assert.That(fixture, Is.Not.Null);
            var example1 = BcfFile.ReadFrom(fixture);
            Assert.That(example1, Is.Not.Null);
            
            var issues = example1.Issues.ToArray();

            Assert.That(issues, Is.Not.Null);
            Assert.That(16, Is.EqualTo(issues.Length));

            Assert.That(issues.All(i => null != i.Markup.Topic && 0 < i.Viewpoints.Length), Is.True);
        }

        [Test]
        public void ReadExample2()
        {
            var fixture = GetEmbeddedFileStream("Bcf21.Example2.bcfzip");
            Assert.That(fixture, Is.Not.Null);
            var example2 = BcfFile.ReadFrom(fixture);
            Assert.That(example2, Is.Not.Null);

            var issues = example2.Issues.ToArray();

            Assert.That(issues, Is.Not.Null);
            Assert.That(2, Is.EqualTo(issues.Length));

            Assert.That(issues.All(i => null != i.Markup.Topic), Is.True);
            Assert.That(1, Is.EqualTo(issues.Count(i => i.Viewpoints.Length == 0)));

            var extensions = example2.Extensions;
            Assert.That(6, Is.EqualTo(extensions.Count));
            Assert.That(3, Is.EqualTo(extensions.First(e => e.Key == "TopicType").Count()));
        }
    }
}
