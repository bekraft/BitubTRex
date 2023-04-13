using System;
using Bitub.Dto.Scene;
using Bitub.Dto.Spatial;
using NUnit.Framework;

#if NETFRAMEWORK
using MathF = System.Math;
#endif

namespace Bitub.Dto.Tests.Scene
{
    public class M33Tests : TestBase<M33Tests>
    {
        public M33Tests() : base()
        { }
        
        [Test]
        public void IdentityToQuatIsIdentity() 
        {
            var quat = M33.Identity.ToQuat();
            Assert.IsTrue(quat.IsAlmostEqualTo(Quat.Identity));
        }

        [Test]
        public void ToLineStringFromLineString()
        {
            var fixture = $"{M33.Identity.ToLinedString()} Test";
            var tail = M33.FromLineString(fixture, out M33 result);
            Assert.AreEqual(M33.Identity, result);
            Assert.AreEqual("Test", tail);
        }

        [Test]
        public void RotateZAndTransformOneX()
        {
            var m = M33.Identity.RotateZ((float)MathF.PI / 2);
            Assert.IsTrue(new M33 { Rx = XYZ.OneY * -1, Ry = XYZ.OneX, Rz = XYZ.OneZ }.IsAlmostEqualTo(m));
            var r = m * XYZ.OneX;
            Assert.IsTrue(XYZ.OneY.IsAlmostEqualTo(r));
        }
        
        [Test]
        public void RotateYAndTransformOneZ()
        {
            var m = M33.Identity.RotateY((float)MathF.PI / 2);
            Assert.IsTrue(new M33 { Rx = XYZ.OneZ, Ry = XYZ.OneY, Rz = XYZ.OneX * -1 }.IsAlmostEqualTo(m));
            var r = m * XYZ.OneZ;
            Assert.IsTrue(XYZ.OneX.IsAlmostEqualTo(r));
        }

        [Test]
        public void RotateXAndTransformOneY()
        {
            var m = M33.Identity.RotateX((float)MathF.PI / 2);
            Assert.IsTrue(new M33 { Rx = XYZ.OneX, Ry = XYZ.OneZ * -1, Rz = XYZ.OneY }.IsAlmostEqualTo(m));
            var r = m * XYZ.OneY;
            Assert.IsTrue(XYZ.OneZ.IsAlmostEqualTo(r));
        }

    }
}