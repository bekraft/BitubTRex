using Bitub.Dto.Math;
using NUnit.Framework;

namespace Bitub.Dto.Tests.Math
{
    public class FloatPrecisionTests : TestBase<FloatPrecisionTests>
    {
        protected FloatPrecisionPredicate fixture;

        [SetUp]
        public void SetUp() 
        { 
            fixture = FloatPrecisionPredicate.Builder.Build();
        }

        [Test]
        public void InitPrecisionPredicate()
        {
            Assert.That(fixture.NormativePrecision, Is.GreaterThan(0.0));
        }
    }
}
