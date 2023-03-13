using System;
using Bitub.Dto.Spatial;

namespace Bitub.Dto.Scene
{
    public partial class Quat
    {
        public static Quat Identity => new Quat() { X = 0, Y = 0, Z = 0, W = 1 };

        public double Magnitude => Math.Sqrt(Dot(this));

        public double Dot(Quat other) => X * other.X + Y * other.Y + Z * other.Z + W * other.W;

        public Quat Scale(double s) => new Quat { X = (float) (X * s), Y = (float) (Y * s), Z = (float) (Z * s), W = (float) (W * s) };

        public Quat ToNormalized() => Scale(1.0 / Magnitude);

        // Credits to https://www.euclideanspace.com/maths/geometry/rotations/conversions/quaternionToMatrix/index.htm
        public M33 ToM33()
        {
            var rx = XYZ.OneX;
            var ry = XYZ.OneY;
            var rz = XYZ.OneZ;
            
            double sqw = W * W;
            double sqx = X * X;
            double sqy = Y * Y;
            double sqz = Z * Z;

            // invs (inverse square length) is only required if quaternion is not already normalised
            double invs = 1 / (sqx + sqy + sqz + sqw);
            rx.X = (float)(( sqx - sqy - sqz + sqw) * invs); // since sqw + sqx + sqy + sqz =1/invs * invs
            ry.Y = (float)((-sqx + sqy - sqz + sqw) * invs);
            rz.Z = (float)((-sqx - sqy + sqz + sqw) * invs);
            
            double tmp1 = X * Y;
            double tmp2 = Z * W;
            ry.X = (float)(2.0 * (tmp1 + tmp2) * invs);
            rx.Y = (float)(2.0 * (tmp1 - tmp2) * invs);
            
            tmp1 = X * Z;
            tmp2 = Y * W;
            rz.X = (float)(2.0 * (tmp1 - tmp2) * invs);
            rx.Z = (float)(2.0 * (tmp1 + tmp2) * invs);
            tmp1 = Y * Z;
            tmp2 = X * W;
            rz.Y = (float)(2.0 * (tmp1 + tmp2) * invs);
            ry.Z = (float)(2.0 * (tmp1 - tmp2) * invs);
            return new M33 { Rx = rx, Ry = ry, Rz = rz };
        }

        /// <summary>
        /// True, if this Quat is almost equal by each component within given precision inclusively.
        /// </summary>
        /// <param name="other">Other Quat</param>
        /// <param name="precision">Threshold, meant inclusively, default 10e-6</param>
        /// <returns>True, if almost equal</returns>
        public bool IsAlmostEqualTo(Quat other, double precision = 10e-6) 
        {
            return !(Math.Abs(X - other.X) > precision || Math.Abs(Y - other.Y) > precision || Math.Abs(Z - other.Z) > precision || Math.Abs(W - other.W) > precision);
        }

        public Quat Times(Quat other) 
        {
            return new Quat
            {
                X = W * other.X + X * other.W + Y * other.Z - Z * other.Y,
                Y = W * other.Y + Y * other.W + Z * other.X - X * other.Z,
                Z = W * other.Z + Z * other.W + X * other.Y - Y * other.X,
                W = W * other.W - X * other.X - Y * other.Y - Z * other.Z
            };
        }
    }
}