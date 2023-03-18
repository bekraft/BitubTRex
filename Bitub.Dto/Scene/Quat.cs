using System;
using System.Xml;
using Bitub.Dto.Spatial;

namespace Bitub.Dto.Scene
{
    public partial class Quat
    {
        /// <summary>
        /// Identity quaternion such that <code>q * q^-1 = Identity</code>.
        /// </summary>
        public static Quat Identity => new Quat() { X = 0, Y = 0, Z = 0, W = 1 };

        /// <summary>
        /// Magnitude of this quaternion.
        /// </summary>
        public double Magnitude => Math.Sqrt(Dot(this));

        /// <summary>
        /// Dot product with other quaternion.
        /// </summary>
        /// <param name="other">Other</param>
        /// <returns>Scalar value</returns>
        public double Dot(Quat other) => X * other.X + Y * other.Y + Z * other.Z + W * other.W;

        /// <summary>
        /// The Abs (squared magnitude).
        /// </summary>
        /// <returns>Absolute scalar value</returns>
        public double Abs() => Dot(this);

        /// <summary>
        /// Scale a quaternion by given scalar value.
        /// </summary>
        /// <param name="s">The scale</param>
        /// <returns>Scaled quaternion</returns>
        public Quat Scale(double s) => new Quat 
        { 
            X = (float) (X * s), 
            Y = (float) (Y * s), 
            Z = (float) (Z * s), 
            W = (float) (W * s) 
        };

        public Quat Diff(Quat other) => new Quat
        {
            X = X - other.X,
            Y = Y - other.Y,
            Z = Z - other.Z,
            W = W - other.W
        };

        public Quat Add(Quat other) => new Quat
        {
            X = X + other.X,
            Y = Y + other.Y,
            Z = Z + other.Z,
            W = W + other.W
        };

        /// <summary>
        /// Normalize quaternion into a new quaternion.
        /// </summary>
        /// <returns>New normalized quaternion.</returns>
        public Quat ToNormalized() => Scale(1.0 / Magnitude);

        /// <summary>
        /// Conjugate this quaternion.
        /// </summary>
        /// <returns>The new conjugate.</returns>
        public Quat Conjugate() => new Quat
        {
            X = -X,
            Y = -Y,
            Z = -Z,
            W = W
        };

        /// <summary>
        /// Inverse of this quaternion.
        /// </summary>
        /// <returns>The new inverse.</returns>
        public Quat Inverse() => Conjugate().Scale(1.0 / Dot(this));

        /// <summary>
        /// Calculate delta quaternion such that <code>delta*this = other</code>.
        /// </summary>
        /// <param name="other">The normalized target quaternion</param>
        /// <returns>The delta (transform diff) quaternion.</returns>
        public Quat Delta(Quat other) => other.Times(Inverse());

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

        public static Quat operator *(Quat q, Quat p) => q.Times(p);

        public static Quat operator *(Quat q, double scale) => q.Scale(scale);

        public static Quat operator -(Quat q, Quat p) => q.Diff(p);

        public static Quat operator +(Quat q, Quat p) => q.Add(p);

    }
}