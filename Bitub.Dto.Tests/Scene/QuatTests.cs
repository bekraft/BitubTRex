using System;
using Bitub.Dto.Scene;
using Bitub.Dto.Spatial;
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
            Assert.That(m33.IsAlmostEqualTo(M33.Identity), Is.True);
        }

        [Test]
        public void InverseTimesNonInverseIsIdentity()
        {
            var q = M33.MirrorX.ToQuat().ToNormalized();
            Assert.That(Quat.Identity.IsAlmostEqualTo(q.Inverse() * q), Is.True);
        }

        [Test]
        public void InverseIsNormalizedConjugate()
        {
            var q = M33.MirrorX.ToQuat();
            Assert.That(q.Inverse().IsAlmostEqualTo(q.ToNormalized().Conjugate()), Is.True);
        }

        [Test]
        public void RotateZAndTransformOneX()
        {
            var q = M33.Identity.RotateZ((float)MathF.PI / 2).ToQuat();
            var v = q.Transform(XYZ.OneX);
            Assert.That(XYZ.OneY.IsAlmostEqualTo(v), Is.True);
        }
        
        [Test]
        public void RotateXAndTransformOneY()
        {
            var q = M33.Identity.RotateX((float)MathF.PI / 2).ToQuat();
            var v = q.Transform(XYZ.OneY);
            Assert.That(XYZ.OneZ.IsAlmostEqualTo(v), Is.True);
        }

        [Test]
        public void RotateYAndTransformOneZ()
        {
            var q = M33.Identity.RotateY((float)MathF.PI / 2).ToQuat();
            var v = q.Transform(XYZ.OneZ);
            Assert.That(XYZ.OneX.IsAlmostEqualTo(v), Is.True);
        }

        [Test]
        public void DeltaPostRotateX()
        {
            var q0 = M33.Identity.RotateX((float)MathF.PI / 4).ToQuat();
            var q1 = M33.Identity.RotateX((float)MathF.PI / 2).ToQuat();
            var d = q0.DeltaPost(q1);
            
            Assert.That(d.IsAlmostEqualTo(q0), Is.True);
            Assert.That(q0.Times(d).IsAlmostEqualTo(q1), Is.True);
        }
        
        [Test]
        public void DeltaPreRotateX()
        {
            var q0 = M33.Identity.RotateX((float)MathF.PI / 4).ToQuat();
            var q1 = M33.Identity.RotateX((float)MathF.PI / 2).ToQuat();
            var d = q0.DeltaPre(q1);
            
            Assert.That(d.IsAlmostEqualTo(q0), Is.True);
            Assert.That(d.Times(q0).IsAlmostEqualTo(q1), Is.True);
        }
    }
}