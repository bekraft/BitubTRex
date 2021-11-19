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

        public XYZ Normalized
        {
            get => this.ToNormalized();
        }

        public void Normalize()
        {
            var norm2 = Magnitude;
            X = (float)(X / norm2);
            Y = (float)(Y / norm2);
            Z = (float)(Z / norm2);
        }

        public static XYZ operator +(XYZ a, XYZ b) => a.Add(b);
        public static XYZ operator -(XYZ a, XYZ b) => a.Sub(b);
        public static XYZ operator +(XYZ a) => a;
        public static XYZ operator -(XYZ a) => a.Negate();
        public static XYZ operator *(XYZ a, XYZ b) => a.Cross(b);
        public static XYZ operator *(XYZ a, float s) => a.Scale(s);

        public static XYZ PositiveInfinity
        {
            get => new XYZ(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
        }

        public static XYZ NegativeInfinity
        {
            get => new XYZ(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);
        }

        public float GetCoordinate(int index)
        {
            switch (index)
            {
                case 0:
                    return X;
                case 1:
                    return Y;
                case 2:
                    return Z;
                default:
                    throw new IndexOutOfRangeException($"{index} is out of range of [0,2]");
            }
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
    }
}
