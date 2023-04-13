using System;
using Bitub.Dto.Scene;
using Bitub.Dto.Spatial;
using NUnit.Framework;

#if NETFRAMEWORK
using MathF = System.Math;
#endif

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

        [Test]
        public void InverseTimesNonInverseIsIdentity()
        {
            var q = M33.MirrorX.ToQuat().ToNormalized();
            Assert.IsTrue(Quat.Identity.IsAlmostEqualTo(q.Inverse() * q));
        }

        [Test]
        public void InverseIsNormalizedConjugate()
        {
            var q = M33.MirrorX.ToQuat();
            Assert.IsTrue(q.Inverse().IsAlmostEqualTo(q.ToNormalized().Conjugate()));
        }

        [Test]
        public void RotateZAndTransformOneX()
        {
            var q = M33.Identity.RotateZ((float)MathF.PI / 2).ToQuat();
            var v = q.Transform(XYZ.OneX);
            Assert.IsTrue(XYZ.OneY.IsAlmostEqualTo(v));
        }
        
        [Test]
        public void RotateXAndTransformOneY()
        {
            var q = M33.Identity.RotateX((float)MathF.PI / 2).ToQuat();
            var v = q.Transform(XYZ.OneY);
            Assert.IsTrue(XYZ.OneZ.IsAlmostEqualTo(v));
        }

        [Test]
        public void RotateYAndTransformOneZ()
        {
            var q = M33.Identity.RotateY((float)MathF.PI / 2).ToQuat();
            var v = q.Transform(XYZ.OneZ);
            Assert.IsTrue(XYZ.OneX.IsAlmostEqualTo(v));
        }

        [Test]
        public void DeltaPostRotateX()
        {
            var q0 = M33.Identity.RotateX((float)MathF.PI / 4).ToQuat();
            var q1 = M33.Identity.RotateX((float)MathF.PI / 2).ToQuat();
            var d = q0.DeltaPost(q1);
            
            Assert.IsTrue(d.IsAlmostEqualTo(q0));
            Assert.IsTrue(q0.Times(d).IsAlmostEqualTo(q1));
        }
        
        [Test]
        public void DeltaPreRotateX()
        {
            var q0 = M33.Identity.RotateX((float)MathF.PI / 4).ToQuat();
            var q1 = M33.Identity.RotateX((float)MathF.PI / 2).ToQuat();
            var d = q0.DeltaPre(q1);
            
            Assert.IsTrue(d.IsAlmostEqualTo(q0));
            Assert.IsTrue(d.Times(q0).IsAlmostEqualTo(q1));
        }
    }
}