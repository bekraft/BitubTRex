using System;

using Bitub.Dto.Spatial;

namespace Bitub.Dto.Scene
{
    public static class GeometryExtensions
    {
        #region Mappint to System.Numerics

        public static System.Numerics.Quaternion ToNetQuaternion(this Quaternion q)
        {
            return new System.Numerics.Quaternion((float)q.X, (float)q.Y, (float)q.Z, (float)q.W);
        }

        public static System.Numerics.Vector3 ToNetVector3(this XYZ p)
        {
            return new System.Numerics.Vector3(p.X, p.Y, p.Z);
        }

        public static System.Numerics.Matrix4x4 ToNetMatrix4x4(this Rotation r)
        {
            return new System.Numerics.Matrix4x4(
                r.Rx.X, r.Rx.Y, r.Rx.Z, 0,
                r.Ry.X, r.Ry.Y, r.Ry.Z, 0,
                r.Rz.X, r.Rz.Y, r.Rz.Z, 0,
                0, 0, 0, 1
            );
        }

        public static System.Numerics.Matrix4x4 ToNetMatrix4x4(this Transform t)
        {
            switch(t.RotationOrQuaternionCase)
            {
                case Transform.RotationOrQuaternionOneofCase.Q:
                    var rQ = System.Numerics.Matrix4x4.CreateFromQuaternion(t.Q.ToNetQuaternion());
                    rQ.Translation = t.T.ToNetVector3();
                    return rQ;
                case Transform.RotationOrQuaternionOneofCase.R:
                    var rR = t.R.ToNetMatrix4x4();
                    rR.Translation = t.T.ToNetVector3();
                    return rR;
                case Transform.RotationOrQuaternionOneofCase.None:
                    return System.Numerics.Matrix4x4.Identity;
                default:
                    throw new NotImplementedException();
            }
            
        }

        #endregion
    }
}

