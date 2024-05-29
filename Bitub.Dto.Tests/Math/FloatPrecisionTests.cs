using Bitub.Dto.Math;
using NUnit.Framework;

namespace Bitub.Dto.Tests.Math
{
    public class FloatExactPrecisionTests : TestBase<FloatExactPrecisionTests>
    {
        protected FloatExactPrecisionPredicate fixture;

        [SetUp]
        public void SetUp() 
        { 
            fixture = FloatExactPrecisionPredicate.Builder.Build();
        }

        [Test]
        public void InitPrecisionPredicate()
        {
            Assert.That(fixture.NormativePrecision, Is.GreaterThan(0.0));
        }
    }
}
