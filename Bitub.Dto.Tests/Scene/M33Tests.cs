using System;
using Bitub.Dto.Scene;
using Bitub.Dto.Spatial;
using NUnit.Framework;

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
            Assert.That(quat.IsAlmostEqualTo(Quat.Identity), Is.True);
        }

        [Test]
        public void ToLineStringFromLineString()
        {
            var fixture = $"{M33.Identity.ToLinedString()} Test";
            var tail = M33.FromLineString(fixture, out M33 result);
            Assert.That(M33.Identity, Is.EqualTo(result));
            Assert.That("Test", Is.EqualTo(tail));
        }

        [Test]
        public void RotateZAndTransformOneX()
        {
            var m = M33.Identity.RotateZ((float)MathF.PI / 2);
            Assert.That(new M33 { Rx = XYZ.OneY * -1, Ry = XYZ.OneX, Rz = XYZ.OneZ }.IsAlmostEqualTo(m), Is.True);
            var r = m * XYZ.OneX;
            Assert.That(XYZ.OneY.IsAlmostEqualTo(r), Is.True);
        }
        
        [Test]
        public void RotateYAndTransformOneZ()
        {
            var m = M33.Identity.RotateY((float)MathF.PI / 2);
            Assert.That(new M33 { Rx = XYZ.OneZ, Ry = XYZ.OneY, Rz = XYZ.OneX * -1 }.IsAlmostEqualTo(m), Is.True);
            var r = m * XYZ.OneZ;
            Assert.That(XYZ.OneX.IsAlmostEqualTo(r), Is.True);
        }

        [Test]
        public void RotateXAndTransformOneY()
        {
            var m = M33.Identity.RotateX((float)MathF.PI / 2);
            Assert.That(new M33 { Rx = XYZ.OneX, Ry = XYZ.OneZ * -1, Rz = XYZ.OneY }.IsAlmostEqualTo(m), Is.True);
            var r = m * XYZ.OneY;
            Assert.That(XYZ.OneZ.IsAlmostEqualTo(r), Is.True);
        }

    }
}