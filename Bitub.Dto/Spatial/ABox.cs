using System;

namespace Bitub.Dto.Spatial
{
    public partial class ABox
    {
        /// <summary>
        /// Create a new axis aligned box given by two spanning XYZs in any order.
        /// </summary>
        /// <param name="a">Some point</param>
        /// <param name="b">Some other point</param>
        public ABox(XYZ a, XYZ b)
        {
            if (a.IsAlmostEqualTo(b))
            {
                Min = XYZ.PositiveInfinity;
                Max = XYZ.NegativeInfinity;
            }
            else
            {
                Min = new XYZ(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y), Math.Min(a.Z, b.Z));
                Max = new XYZ(Math.Max(a.X, b.X), Math.Max(a.Y, b.Y), Math.Max(a.Z, b.Z));
            }
        }

        /// <summary>
        /// New empty ABox.
        /// </summary>
        public static ABox Empty
        {
            get => new ABox
            {
                Min = XYZ.PositiveInfinity,
                Max = XYZ.NegativeInfinity
            };
        }

        /// <summary>
        /// New open ABox.
        /// </summary>
        public static ABox Open
        {
            get => new ABox
            {
                Min = XYZ.NegativeInfinity,
                Max = XYZ.PositiveInfinity
            };
        }

        public XYZ Center
        {
            get
            {
                if (IsEmpty)
                    throw new NotSupportedException("Empty ABox have no center");
                else
                    // Scale extent by 0.5 and add to base
                    return (Min + Max) * 0.5f;
            }
        }

        public bool IsEmpty => Min.X >= Max.X || Min.Y >= Max.Y || Min.Z >= Max.Z;
        
        public ABox IntersectWith(ABox b)
        {
            if (IsEmpty || b.IsEmpty)
                return ABox.Empty;
            else
                return new ABox
                {
                    Min = new XYZ
                    {
                        X = Math.Max(Min.X, b.Min.X),
                        Y = Math.Max(Min.Y, b.Min.Y),
                        Z = Math.Max(Min.Z, b.Min.Z)
                    },
                    Max = new XYZ
                    {
                        X = Math.Min(Max.X, b.Max.X),
                        Y = Math.Min(Max.Y, b.Max.Y),
                        Z = Math.Min(Max.Z, b.Max.Z)
                    }
                };
        }

        public ABox UnionWith(ABox b)
        {
            if (IsEmpty)
                return b;
            else if (b.IsEmpty)
                return this;
            else
                return new ABox
                {
                    Min = new XYZ
                    {
                        X = Math.Min(Min.X, b.Min.X),
                        Y = Math.Min(Min.Y, b.Min.Y),
                        Z = Math.Min(Min.Z, b.Min.Z)
                    },
                    Max = new XYZ
                    {
                        X = Math.Max(Max.X, b.Max.X),
                        Y = Math.Max(Max.Y, b.Max.Y),
                        Z = Math.Max(Max.Z, b.Max.Z)
                    }
                };
        }

        public double Volume
        {
            get
            {
                var volume = (Max.X - Min.X) * (Max.Y - Min.Y) * (Max.Z - Min.Z);
                return volume > 0 ? volume : 0;
            }
        }
    }
}
