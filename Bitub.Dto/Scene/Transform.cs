using System;
using Bitub.Dto.Spatial;

namespace Bitub.Dto.Scene
{
    public partial class Transform
    {
        public static Transform Identity 
            => new Transform() { T = XYZ.Zero, R = M33.Identity };

        public static Transform MirrorX
            => new Transform { T = XYZ.Zero, R = new M33 { Rx = XYZ.OneX * -1, Ry = XYZ.OneY, Rz = XYZ.OneZ } };

        public static Transform MirrorY
            => new Transform { T = XYZ.Zero, R = new M33 { Rx = XYZ.OneX, Ry = XYZ.OneY * -1, Rz = XYZ.OneZ } };

        public static Transform MirrorZ
            => new Transform { T = XYZ.Zero, R = new M33 { Rx = XYZ.OneX, Ry = XYZ.OneY, Rz = XYZ.OneZ * -1 } };

        public static Transform RighthandZup 
            => Identity;

        public static Transform RighthandYup
            => new Transform { T = XYZ.Zero, R = new M33 { Rx = XYZ.OneX, Ry = XYZ.OneZ, Rz = XYZ.OneY * -1 } };
        
        public static Transform LefthandZup 
            => new Transform { T = XYZ.Zero, R = new M33 { Rx = XYZ.OneX, Ry = XYZ.OneY * -1, Rz = XYZ.OneZ } };
        
        public static Transform LefthandYup
            => new Transform { T = XYZ.Zero, R = new M33 { Rx = XYZ.OneX, Ry = XYZ.OneZ, Rz = XYZ.OneY } };


        /// <summary>
        /// Returns rotation as rotation matrix.
        /// </summary>
        public M33 RotationM => RotationOrQuaternionCase switch
        {
            RotationOrQuaternionOneofCase.Q => Q.ToM33(),
            RotationOrQuaternionOneofCase.R => R,
            _ => M33.Identity
        };

        /// <summary>
        /// Returns rotation as quaternion.
        /// </summary>
        public Quat RotationQ => RotationOrQuaternionCase switch
        {
            RotationOrQuaternionOneofCase.Q => Q,
            RotationOrQuaternionOneofCase.R => R.ToQuat(),
            _ => Quat.Identity
        };

        /// <summary>
        /// Concat given transform. For best precision, ensure that given transform type matches
        /// this transform type (see <see cref="RotationOrQuaternionCase"/>).
        /// </summary>
        /// <param name="t">Given transform</param>
        /// <returns>New transformed transform</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown only if unexpected case occurs</exception>
        public Transform Apply(Transform t) => t.RotationOrQuaternionCase switch
        {
            RotationOrQuaternionOneofCase.R => ApplyM33(t),
            RotationOrQuaternionOneofCase.None => this,
            RotationOrQuaternionOneofCase.Q => ApplyQ(t),
            _ => throw new ArgumentOutOfRangeException()
        };

        public XYZ Apply(XYZ xyz) => RotationOrQuaternionCase switch
        {
            RotationOrQuaternionOneofCase.None => xyz,
            RotationOrQuaternionOneofCase.R => R.Times(xyz).Inc(T),
            RotationOrQuaternionOneofCase.Q => Q.Transform(xyz).Inc(T),
            _ => throw new ArgumentOutOfRangeException()
        };
        
        private Transform ApplyM33(Transform t)
        {
            var r = RotationM;
            return new Transform { R = r.Times(t.R), T = r.Times(t.T).Inc(T) };
        }

        private Transform ApplyQ(Transform t)
        {
            var q = RotationQ;
            return new Transform { Q = q * t.Q, T = T + t.T };
        }

        public Transform Offset(XYZ offset) => new Transform { R = R, Q = Q, T = T.Add(offset) };
    }
}