using System;

namespace Bitub.Dto.Spatial
{
    public partial class XYZ
    {
        public static XYZ Zero 
        { 
            get => new XYZ(0, 0, 0); 
        }

        public XYZ(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public double Magnitude 
        {
            get => this.ToNorm2();
        }

        public static XYZ operator +(XYZ a, XYZ b) => a.Add(b);
        public static XYZ operator -(XYZ a, XYZ b) => a.Sub(b);
        public static XYZ operator +(XYZ a) => a;
        public static XYZ operator -(XYZ a) => a.Negate();
        public static XYZ operator *(XYZ a, XYZ b) => a.Cross(b);
    }
}
