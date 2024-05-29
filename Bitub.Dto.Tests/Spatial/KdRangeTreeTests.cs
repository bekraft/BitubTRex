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
            Assert.That(fixture.Root, Is.Null);
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
                    Assert.That(n, Is.Not.Null);
                    Assert.That(n.Point, Is.Not.Null);
                });

            Assert.That(fixture.Root, Is.Not.Null);
            Assert.That(fixture.ABox.Equals(ABox.Empty), Is.False);
            Assert.That(fixture.ABox.Equals(ABox.Open), Is.False);

            var testAbox = fixture.ABox.Scale(0.5);

            var result = fixture.PointsWithin(testAbox)
                .ToImmutableHashSet();

            var expected = points
                .Where(xyz => testAbox.Contains(xyz))
                .ToImmutableHashSet();
            
            Assert.That(result.SetEquals(expected), Is.True);
        }
    }
}