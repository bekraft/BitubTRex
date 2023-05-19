using System;

using Bitub.Dto.Spatial;

namespace Bitub.Dto.Scene
{
    public static class SystemNumericsExtensions
    {
        #region Mapping from System.Numerics

        public static XYZ ToXYZ(this System.Numerics.Vector3 xyz)
        {
            return new XYZ(xyz.X, xyz.Y, xyz.Z);
        }

        public static Quat ToQuat(this System.Numerics.Quaternion quaternion)
        {
            return new Quat { X = quaternion.X, Y = quaternion.Y, Z = quaternion.Z, W = quaternion.W };
        }

        public static Transform ToTransform(this System.Numerics.Matrix4x4 m44)
        {
            return new Transform
            {
                T = new XYZ 
                { 
                    X = m44.M41, Y = m44.M42, Z = m44.M43 
                },
                R = new M33 
                {
                    Rx = new XYZ { X = m44.M11, Y = m44.M12, Z = m44.M13 },
                    Ry = new XYZ { X = m44.M21, Y = m44.M22, Z = m44.M23 },
                    Rz = new XYZ { X = m44.M31, Y = m44.M32, Z = m44.M33 }
                }
            };
        }

        #endregion

        #region Mapping to System.Numerics

        public static System.Numerics.Quaternion ToNetQuaternion(this Quat q)
        {
            return new System.Numerics.Quaternion((float)q.X, (float)q.Y, (float)q.Z, (float)q.W);
        }

        public static System.Numerics.Matrix4x4 ToNetMatrix44(this M33 r)
        {
            return new System.Numerics.Matrix4x4(
                r.Rx.X, r.Rx.Y, r.Rx.Z, 0,
                r.Ry.X, r.Ry.Y, r.Ry.Z, 0,
                r.Rz.X, r.Rz.Y, r.Rz.Z, 0,
                0, 0, 0, 1
            );
        }

        public static System.Numerics.Vector3 ToNetVector3(this XYZ xyz)
        {
            return new System.Numerics.Vector3(xyz.X, xyz.Y, xyz.Z);
        }

        public static System.Numerics.Matrix4x4 ToNetMatrix44(this Transform t)
        {
            switch(t.RotationOrQuaternionCase)
            {
                case Transform.RotationOrQuaternionOneofCase.Q:
                    var rQ = System.Numerics.Matrix4x4.CreateFromQuaternion(t.Q.ToNetQuaternion());
                    rQ.Translation = t.T.ToNetVector3();
                    return rQ;
                case Transform.RotationOrQuaternionOneofCase.R:
                    var rR = t.R.ToNetMatrix44();
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

