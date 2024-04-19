using Bitub.Dto.Spatial;
using NUnit.Framework;

namespace Bitub.Dto.Tests.Spatial
{
    public class XYZTests : TestBase<XYZTests>
    {
        public XYZTests() : base() 
        {}

        [Test]
        public void LineStringTests()
        {
            var fixture = $"{XYZ.OneX.ToLinedString()} Test";
            var tail = XYZ.FromLineString(fixture, out XYZ result);
            Assert.That(XYZ.OneX, Is.EqualTo(result));
            Assert.That("Test", Is.EqualTo(tail));
        }
    }
}