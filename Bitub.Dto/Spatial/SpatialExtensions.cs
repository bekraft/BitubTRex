using System;

namespace Bitub.Dto.Spatial
{
    public static class SpatialExtensions
    {
        #region Cross-Casting to System.Numerics

        public static System.Numerics.Vector3 ToNetVector3(this XYZ xyz)
        {
            return new System.Numerics.Vector3((float)xyz.X, (float)xyz.Y, (float)xyz.Z);
        }

        #endregion

        public static bool IsAlmostEqual(this XYZ a, XYZ b, double precision)
        {
            return !(Math.Abs(a.X - b.X) > precision || Math.Abs(a.Y - b.Y) > precision || Math.Abs(a.Z - b.Z) > precision);
        }

        public static float[] ToArray(this XYZ a)
        {
            return new float[] { a.X, a.Y, a.Z };
        }

        public static float ToNorm2(this XYZ a)
        {
            return (float)Math.Sqrt((double)a.X * a.X + (double)a.Y * a.Y + (double)a.Z * a.Z);
        }

        public static XYZ ToNormalized(this XYZ a)
        {
            var norm2 = a.ToNorm2();
            return new XYZ
            {
                X = a.X / norm2,
                Y = a.Y / norm2,
                Z = a.Z / norm2
            };
        }

        public static XYZ Cross(this XYZ a, XYZ b) => new XYZ
        {
            X = a.Y * b.Z - b.Y * a.Z,
            Y = a.Z * b.X - a.X * b.Z,
            Z = a.X * b.Y - a.Y * b.X
        };

        public static XYZ Sub(this XYZ a, XYZ b) => new XYZ
        {
            X = a.X - b.X,
            Y = a.Y - b.Y,
            Z = a.Z - b.Z
        };

        public static XYZ Add(this XYZ a, XYZ b) => new XYZ
        {
            X = a.X + b.X,
            Y = a.Y + b.Y,
            Z = a.Z + b.Z
        };

        public static XYZ Scale(this XYZ a, float s) => new XYZ
        {
            X = (a.X * s),
            Y = (a.Y * s),
            Z = (a.Z * s),
        };

        public static XYZ CenterOf(this ABox box)
        {
            // Scale extent by 0.5 and add to base
            return box.Min.Add(box.Max.Scale(0.5f));
        }

        public static bool IsEmpty(this ABox box)
        {
            return box.Min.X >= box.Max.X || box.Min.Y >= box.Max.Y || box.Min.Z >= box.Max.Z;
        }

        public static ABox IntersectWith(this ABox a, ABox b)
        {
            return new ABox
            {
                Min = new XYZ
                {
                    X = Math.Max(a.Min.X, b.Min.X),
                    Y = Math.Max(a.Min.Y, b.Min.Y),
                    Z = Math.Max(a.Min.Z, b.Min.Z)
                },
                Max = new XYZ
                {
                    X = Math.Min(a.Max.X, b.Max.X),
                    Y = Math.Min(a.Max.Y, b.Max.Y),
                    Z = Math.Min(a.Max.Z, b.Max.Z)
                }
            };
        }

        public static ABox UnionWith(this ABox a, ABox b)
        {
            return new ABox
            {
                Min = new XYZ
                {
                    X = Math.Min(a.Min.X, b.Min.X),
                    Y = Math.Min(a.Min.Y, b.Min.Y),
                    Z = Math.Min(a.Min.Z, b.Min.Z)
                },
                Max = new XYZ
                {
                    X = Math.Max(a.Max.X, b.Max.X),
                    Y = Math.Max(a.Max.Y, b.Max.Y),
                    Z = Math.Max(a.Max.Z, b.Max.Z)
                }
            };
        }

        public static double Volume(this ABox a)
        {
            return (a.Max.X - a.Min.X) * (a.Max.Y - a.Min.Y) * (a.Max.Z - a.Min.Z);
        }
    }
}
