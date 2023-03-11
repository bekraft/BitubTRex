using Bitub.Dto.Scene;
using NUnit.Framework;

namespace Bitub.Dto.Tests.Scene
{
    public class QuatTests : TestBase<QuatTests>
    {
        public QuatTests() : base() 
        {}

        [Test]
        public void IdentityMatchesIdentity() 
        {
            var m33 = Quat.Identity.ToM33();
            Assert.IsTrue(m33.IsAlmostEqualTo(M33.Identity));
        }
    }
}