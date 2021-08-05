using System;

namespace Bitub.Dto.Spatial
{
    public static class Extensions
    {
        public static readonly double precision = 1e-8;

        #region Cross-Casting to System.Numerics

        public static System.Numerics.Vector3 ToNetVector3(this XYZ xyz)
        {
            return new System.Numerics.Vector3(xyz.X, xyz.Y, xyz.Z);
        }

        public static XYZ ToXYZ(this System.Numerics.Vector3 xyz)
        {
            return new XYZ(xyz.X, xyz.Y, xyz.Z);
        }

        #endregion

        #region XYZ context

        public static string ToLinedString(this XYZ xyz)
        {
            return string.Format("{0:G} {1:G} {2:G}", xyz.X, xyz.Y, xyz.Z);
        }

        public static bool IsAlmostEqual(this XYZ a, XYZ b, double precision)
        {
            return !(Math.Abs(a.X - b.X) > precision || Math.Abs(a.Y - b.Y) > precision || Math.Abs(a.Z - b.Z) > precision);
        }

        public static float[] ToArray(this XYZ a)
        {
            return new float[] { a.X, a.Y, a.Z };
        }

        public static double ToNorm2(this XYZ a)
        {
            return Math.Sqrt((double)a.X * a.X + (double)a.Y * a.Y + (double)a.Z * a.Z);
        }

        public static XYZ ToNormalized(this XYZ a)
        {
            var norm2 = a.ToNorm2();
            return new XYZ
            {
                X = (float)(a.X / norm2),
                Y = (float)(a.Y / norm2),
                Z = (float)(a.Z / norm2)
            };
        }

        public static XYZ Negate(this XYZ a) => new XYZ
        {
            X = -a.X,
            Y = -a.Y,
            Z = -a.Z
        };

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

        #endregion

        #region ABox context

        public static XYZ CenterOf(this ABox box)
        {
            if (box.IsEmpty())
                throw new NotSupportedException("Empty ABox have no center");
            else
                // Scale extent by 0.5 and add to base
                return box.Min.Add(box.Max.Scale(0.5f));
        }

        public static bool IsEmpty(this ABox box)
        {
            return box.Min.X >= box.Max.X || box.Min.Y >= box.Max.Y || box.Min.Z >= box.Max.Z;
        }

        public static ABox IntersectWith(this ABox a, ABox b)
        {
            if (a.IsEmpty() || b.IsEmpty())
                return ABox.Empty;
            else
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
            if (a.IsEmpty())
                return b;
            else if (b.IsEmpty())
                return a;
            else
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
            var volume = (a.Max.X - a.Min.X) * (a.Max.Y - a.Min.Y) * (a.Max.Z - a.Min.Z);
            return volume > 0 ? volume : 0;
        }

        #endregion

        #region OBox context

        public static ABox ToABox(this OBox box)
        {
            return new ABox(box.Base, box.Base.Add(box.Ex).Add(box.Ey).Add(box.Ez));
        }

        #endregion

        #region General bounding box context

        public static BoundingBox UnionWith(this BoundingBox a, BoundingBox b)
        {
            if (a.OBoxOrABoxCase == BoundingBox.OBoxOrABoxOneofCase.None)
                return b;
            if (b.OBoxOrABoxCase == BoundingBox.OBoxOrABoxOneofCase.None)
                return a;

            var aAbox = (a.OBoxOrABoxCase == BoundingBox.OBoxOrABoxOneofCase.ABox) ? a.ABox : a.OBox.ToABox();
            var bAbox = (b.OBoxOrABoxCase == BoundingBox.OBoxOrABoxOneofCase.ABox) ? b.ABox : b.OBox.ToABox();
            return new BoundingBox { ABox = aAbox.UnionWith(bAbox) };
        }

        #endregion
    }
}
