using Bitub.Dto.Scene;
using NUnit.Framework;

namespace Bitub.Dto.Tests.Scene
{
    public class M33Tests : TestBase<M33Tests>
    {
        public M33Tests() : base()
        { }
        
        [Test]
        public void IdentityMatchesIdentity() 
        {
            var quat = M33.Identity.ToQuat();
            Assert.IsTrue(quat.IsAlmostEqualTo(Quat.Identity));
        }
    }
}