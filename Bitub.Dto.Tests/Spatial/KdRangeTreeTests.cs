using System;
using System.Collections.Immutable;
using System.Linq;
using Bitub.Dto.Spatial;
using NUnit.Framework;

namespace Bitub.Dto.Tests.Spatial
{
    public class KdRangeTreeTests : TestBase<KdRangeTreeTests>
    {
        private KdRange fixture;

        [SetUp]
        public void SetUp()
        {
            fixture = new KdRange(1e-6f, 1e-4f);
            Assert.IsNull(fixture.Root);
        }

        [Test]
        public void AppendAndPointsWithin()
        {
            var random = new Random(534513064);

            var points = Enumerable
                .Range(0, 30)
                .Select(_ => new XYZ(random.NextDouble(), random.NextDouble(), random.NextDouble()).Scale(10))
                .ToList();
                
            points
                .Select(xyz => fixture.Append(xyz))
                .ForEach(n =>
                {
                    Assert.IsNotNull(n);
                    Assert.IsNotNull(n.Point);
                });

            Assert.IsNotNull(fixture.Root);
            Assert.IsFalse(fixture.ABox.Equals(ABox.Empty));
            Assert.IsFalse(fixture.ABox.Equals(ABox.Open));

            var testAbox = fixture.ABox.Scale(0.5);

            var result = fixture.PointsWithin(testAbox)
                .ToImmutableHashSet();

            var expected = points
                .Where(xyz => testAbox.Contains(xyz))
                .ToImmutableHashSet();
            
            Assert.IsTrue(result.SetEquals(expected));
        }
    }
}