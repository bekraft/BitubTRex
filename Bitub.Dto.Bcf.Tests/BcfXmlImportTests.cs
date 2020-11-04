using NUnit.Framework;

using System.Linq;

using Bitub.Dto.BcfXml;

namespace Bitub.Dto.Bcf.Tests
{
    public class BcfXmlTests : BaseTests<BcfXmlTests>
    {
        public BcfXmlTests() : base()
        { }

        [Test]
        public void ReadExample1()
        {
            var fixture = GetEmbeddedFileStream("Bcf21.Example1.bcfzip");
            Assert.IsNotNull(fixture);
            var example1 = BcfFile.ReadFrom(fixture);
            Assert.IsNotNull(example1);
            
            var issues = example1.Issues.ToArray();

            Assert.IsNotNull(issues);
            Assert.AreEqual(16, issues.Length);

            Assert.IsTrue(issues.All(i => null != i.Markup.Topic && 0 < i.Viewpoints.Length));
        }

        [Test]
        public void ReadExample2()
        {
            var fixture = GetEmbeddedFileStream("Bcf21.Example2.bcfzip");
            Assert.IsNotNull(fixture);
            var example2 = BcfFile.ReadFrom(fixture);
            Assert.IsNotNull(example2);

            var issues = example2.Issues.ToArray();

            Assert.IsNotNull(issues);
            Assert.AreEqual(2, issues.Length);

            Assert.IsTrue(issues.All(i => null != i.Markup.Topic));
            Assert.AreEqual(1, issues.Count(i => i.Viewpoints.Length == 0));

            var extensions = example2.Extensions;
            Assert.AreEqual(6, extensions.Count);
            Assert.AreEqual(3, extensions.First(e => e.Key == "TopicType").Count());
        }
    }
}
