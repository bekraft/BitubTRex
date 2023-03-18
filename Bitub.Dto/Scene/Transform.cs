using System.Xml;
using System.Linq.Expressions;
using Bitub.Dto.Spatial;
using System.Collections;

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


        public M33 RotationM => RotationOrQuaternionCase switch
        {
            RotationOrQuaternionOneofCase.Q => Q.ToM33(),
            RotationOrQuaternionOneofCase.R => R,
            _ => M33.Identity
        };

        public Quat RotationQ => RotationOrQuaternionCase switch
        {
            RotationOrQuaternionOneofCase.Q => Q,
            RotationOrQuaternionOneofCase.R => R.ToQuat(),
            _ => Quat.Identity
        };
    }
}