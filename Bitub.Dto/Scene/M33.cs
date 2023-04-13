using System;
using System.Collections.Generic;
using Bitub.Dto.Spatial;

namespace Bitub.Dto.Scene
{
    public partial class M33
    {
        public static M33 Identity => new M33
        {
            Rx = new XYZ { X = 1, Y = 0, Z = 0 },
            Ry = new XYZ { X = 0, Y = 1, Z = 0 },
            Rz = new XYZ { X = 0, Y = 0, Z = 1 },
        };

        public static M33 MirrorX => new M33
        {
            Rx = XYZ.OneX * -1,
            Ry = XYZ.OneY,
            Rz = XYZ.OneZ
        };
        
        public static M33 MirrorY => new M33
        {
            Rx = XYZ.OneX,
            Ry = XYZ.OneY * -1,
            Rz = XYZ.OneZ
        };
        
        public static M33 MirrorZ => new M33
        {
            Rx = XYZ.OneX,
            Ry = XYZ.OneY,
            Rz = XYZ.OneZ * -1
        };
        
        public XYZ this[int index] => index switch
        {
            0 => Rx,
            1 => Ry,
            2 => Rz,
            _ => throw new ArgumentException($"{index} out of range"),
        };

        public IEnumerable<XYZ> Row => new[] { Rx, Ry, Rz };

        public M33 Transpose() => new M33
        {
            Rx = new XYZ(Rx.X, Ry.X, Rz.X ),
            Ry = new XYZ(Rx.Y, Ry.Y, Rz.Y ),
            Rz = new XYZ(Rx.Z, Ry.Z, Rz.Z )
        };

        public string ToLinedString() => $"{Rx.ToLinedString()} {Ry.ToLinedString()} {Rz.ToLinedString()}";

        public static string FromLineString(string lineString, out M33 newM33) 
        {
            var tail = XYZ.FromLineString(
                XYZ.FromLineString(
                    XYZ.FromLineString(lineString, out XYZ rx), out XYZ ry), out XYZ rz);
            newM33 = new M33 { Rx = rx, Ry = ry, Rz = rz };
            return tail;
        }

        /// <summary>
        /// Rotation by Y-axis.
        /// </summary>
        /// <param name="rollRadians">Radians</param>
        /// <returns>New M33</returns>
        public M33 RotateY(float rollRadians) => new M33
        {   
            Rx = new XYZ(MathF.Cos(rollRadians), 0, MathF.Sin(rollRadians)),
            Ry = XYZ.OneY,
            Rz = new XYZ(-MathF.Sin(rollRadians), 0, MathF.Cos(rollRadians))
        };

        /// <summary>
        /// Rotation by Z-axis
        /// </summary>
        /// <param name="yawRadians">Radians</param>
        /// <returns>New M33</returns>
        public M33 RotateZ(float yawRadians) => new M33
        {
            Rx = new XYZ(MathF.Cos(yawRadians), -MathF.Sin(yawRadians), 0),
            Ry = new XYZ(MathF.Sin(yawRadians), MathF.Cos(yawRadians), 0),
            Rz = XYZ.OneZ
        };

        /// <summary>
        /// Rotation by X-axis
        /// </summary>
        /// <param name="pitchRadians">Radians</param>
        /// <returns>New M33</returns>
        public M33 RotateX(float pitchRadians) => new M33
        {
            Rx = XYZ.OneX,
            Ry = new XYZ(0, MathF.Cos(pitchRadians), -MathF.Sin(pitchRadians)),
            Rz = new XYZ(0, MathF.Sin(pitchRadians), MathF.Cos(pitchRadians))
        };
        
        public bool IsAlmostEqualTo(M33 other, double precision = 10e-6)
        {
            return Rx.IsAlmostEqualTo(other.Rx, precision) && Ry.IsAlmostEqualTo(other.Ry, precision) && Rz.IsAlmostEqualTo(other.Rz, precision);
        }

        public M33 Times(M33 m) => new M33
        {
            Rx = new XYZ(
                Rx.X * m.Rx.X + Rx.Y * m.Ry.X + Rx.Z * m.Rz.X,
                Rx.X * m.Rx.Y + Rx.Y * m.Ry.Y + Rx.Z * m.Rz.Y,
                Rx.X * m.Rx.Z + Rx.Y * m.Ry.Z + Rx.Z * m.Rz.Z
            ),
            Ry = new XYZ(
                Ry.X * m.Rx.X + Ry.Y * m.Ry.X + Ry.Z * m.Rz.X,
                Ry.X * m.Rx.Y + Ry.Y * m.Ry.Y + Ry.Z * m.Rz.Y,
                Ry.X * m.Rx.Z + Ry.Y * m.Ry.Z + Ry.Z * m.Rz.Z
            ),
            Rz = new XYZ(
                Rz.X * m.Rx.X + Rz.Y * m.Ry.X + Rz.Z * m.Rz.X,
                Rz.X * m.Rx.Y + Rz.Y * m.Ry.Y + Rz.Z * m.Rz.Y,
                Rz.X * m.Rx.Z + Rz.Y * m.Ry.Z + Rz.Z * m.Rz.Z
            )
        };

        public XYZ Times(XYZ xyz) => new XYZ
        (
            Rx.X * xyz.X + Rx.Y * xyz.Y + Rx.Z * xyz.Z,
            Ry.X * xyz.X + Ry.Y * xyz.Y + Ry.Z * xyz.Z,
            Rz.X * xyz.X + Rz.Y * xyz.Y + Rz.Z * xyz.Z
        );

        public XYZ Transform(XYZ xyz) => Times(xyz);

        public static M33 operator*(M33 r, float scale)
        {
            r.Rx = r.Rx.Scale(scale);
            r.Ry = r.Ry.Scale(scale);
            r.Rz = r.Rz.Scale(scale);
            return r;
        }

        public static M33 operator *(M33 a, M33 b) => a.Times(b);
        
        public static XYZ operator *(M33 a, XYZ b) => a.Times(b);

        // Credits to https://www.euclideanspace.com/maths/geometry/M33s/conversions/matrixToQuaternion/index.htm
        public Quat ToQuat()
        {
            double tr = Rx.X + Ry.Y + Rz.Z;
            double qw = 0;
            double qx = 0;
            double qy = 0; 
            double qz = 0;

            if (tr > 0) { 
                var s = Math.Sqrt(tr + 1.0) * 2; // S=4*qw 
                qw = 0.25 * s;
                qx = (Rz.Y - Ry.Z) / s;
                qy = (Rx.Z - Rz.X) / s; 
                qz = (Ry.X - Rx.Y) / s; 
            } 
            else if ((Rx.X > Ry.Y)&(Rx.X > Rz.Z)) 
            { 
                var s = Math.Sqrt(1.0 + Rx.X - Ry.Y - Rz.Z) * 2; // S=4*qx 
                qw = (Rz.Y - Ry.Z) / s;
                qx = 0.25 * s;
                qy = (Rx.Y + Ry.X) / s; 
                qz = (Rx.Z + Rz.X) / s; 
            } 
            else if (Ry.Y > Rz.Z) 
            { 
                var s = Math.Sqrt(1.0 + Ry.Y - Rx.X - Rz.Z) * 2; // S=4*qy
                qw = (Rx.Z - Rz.X) / s;
                qx = (Rx.Y + Ry.X) / s; 
                qy = 0.25 * s;
                qz = (Ry.Z + Rz.Y) / s; 
            } 
            else 
            { 
                var s = Math.Sqrt(1.0 + Rz.Z - Rx.X - Ry.Y) * 2; // S=4*qz
                qw = (Ry.X - Rx.Y) / s;
                qx = (Rx.Z + Rz.X) / s;
                qy = (Ry.Z + Rz.Y) / s;
                qz = 0.25 * s;
            }

            return new Quat { X = (float)qx, Y = (float)qy, Z = (float)qz, W = (float)qw };
        }
    }
}
