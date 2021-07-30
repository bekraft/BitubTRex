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
            if (a.IsAlmostEqual(b, Extensions.precision))
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
    }
}
