using System;
using System.Linq;
using Bitub.Dto.Scene;

namespace Bitub.Dto.Spatial
{
    public partial class XYZ
    {
        /// <summary>
        /// Zero XYZ.
        /// </summary>
        public static XYZ Zero => new XYZ(0, 0, 0); 

        /// <summary>
        /// New vector (1,0,0).
        /// </summary>
        public static XYZ OneX => new XYZ(1, 0, 0);

        /// <summary>
        /// New vector (0,1,0).
        /// </summary>
        public static XYZ OneY => new XYZ(0, 1, 0);

        /// <summary>
        /// New vector (0,0,1)
        /// </summary>
        public static XYZ OneZ => new XYZ(0, 0, 1);

        /// <summary>
        /// New vevtor (1,1,1)
        /// </summary>
        public static XYZ Ones => new XYZ(1, 1, 1);

        public XYZ(float x, float y, float z)
        {
            x_ = x;
            y_ = y;
            z_ = z;
        }

        public double Magnitude => Math.Sqrt(Dot(this));

        public void Normalize()
        {
            var norm2 = Magnitude;
            X = (float)(X / norm2);
            Y = (float)(Y / norm2);
            Z = (float)(Z / norm2);
        }

        public XYZ ToNormalized()
        {
            var norm2 = Magnitude;
            return new XYZ
            {
                X = (float)(X / norm2),
                Y = (float)(Y / norm2),
                Z = (float)(Z / norm2)
            };
        }

        public static XYZ operator +(XYZ a, XYZ b) => a.Add(b);
        public static XYZ operator -(XYZ a, XYZ b) => a.Sub(b);
        public static XYZ operator +(XYZ a) => a;
        public static XYZ operator -(XYZ a) => a.Negate();
        public static XYZ operator *(XYZ a, XYZ b) => a.Cross(b);
        public static XYZ operator *(XYZ a, float s) => a.Scale(s);
        public static XYZ operator *(XYZ a, double s) => a.Scale(s);

        public static XYZ PositiveInfinity => new XYZ(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);

        public static XYZ NegativeInfinity => new XYZ(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);

        public float GetCoordinate(int index)
        {
            return index switch
            {
                0 => X,
                1 => Y,
                2 => Z,
                _ => throw new IndexOutOfRangeException($"{index} is out of range of [0,2]"),
            };
        }

        public void SetCoordinate(int index, float value)
        {
            switch (index)
            {
                case 0:
                    X = value;
                    break;
                case 1:
                    Y = value;
                    break;
                case 2:
                    Z = value;
                    break;
                default:
                    throw new IndexOutOfRangeException($"{index} is out of range of [0,2]");
            }
        }

        public string ToLinedString() => $"{X:G} {Y:G} {Z:G}";

        public static string FromLineString(string lineString, out XYZ newXYZ) 
        {
            var splitted = lineString
                .Split(new char[]{' '}, 4, StringSplitOptions.None);
            var xyz = splitted
                .Take(3)
                .Select(s => float.Parse(s))
                .ToArray();
            newXYZ = new XYZ { X = xyz[0], Y = xyz[1], Z = xyz[2] };
            return splitted[3]?.Trim();
        }

        /// <summary>
        /// True, if this XYZ is almost equal by each component within given eps inclusively.
        /// </summary>
        /// <param name="other">Other XYZ</param>
        /// <param name="eps">Threshold, meant inclusively, default 10e-6</param>
        /// <returns>True, if almost equal</returns>
        public bool IsAlmostEqualTo(XYZ other, double precision = 1e-6)
        {
            return !(Math.Abs(X - other.X) > precision || Math.Abs(Y - other.Y) > precision || Math.Abs(Z - other.Z) > precision);
        }

        public float[] ToArray()
        {
            return new float[] { X, Y, Z };
        }

        public double Dot(XYZ other)
        {
            return (double)X * other.X + (double)Y * other.Y + (double)Z * other.Z;
        }

        public XYZ Negate() => new XYZ
        {
            X = -X,
            Y = -Y,
            Z = -Z
        };

        public XYZ Cross(XYZ other) => new XYZ
        {
            X = Y * other.Z - other.Y * Z,
            Y = Z * other.X - X * other.Z,
            Z = X * other.Y - Y * other.X
        };

        public XYZ Sub(XYZ other) => new XYZ
        {
            X = X - other.X,
            Y = Y - other.Y,
            Z = Z - other.Z
        };

        public XYZ Add(XYZ other) => new XYZ
        {
            X = X + other.X,
            Y = Y + other.Y,
            Z = Z + other.Z
        };

        public XYZ Inc(XYZ other)
        {
            X += other.X;
            Y += other.Y;
            Z += other.Z;
            return this;
        }

        public XYZ Dec(XYZ other)
        {
            X -= other.X;
            Y -= other.Y;
            Z -= other.Z;
            return this;
        }

        public XYZ Scale(float s) => new XYZ
        {
            X = X * s,
            Y = Y * s,
            Z = Z * s,
        };

        public XYZ Scale(double s) => new XYZ
        {
            X = (float)(X * s),
            Y = (float)(Y * s),
            Z = (float)(Z * s),
        };

        /// <summary>
        /// Transform cartesian to quaternion space.
        /// </summary>
        /// <returns>An Quat</returns>
        public Quat ToQuat() => new Quat { X = X, Y = Y, Z = Z, W = 0 };
    }
}
