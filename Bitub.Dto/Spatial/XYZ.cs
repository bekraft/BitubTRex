using System;

namespace Bitub.Dto.Spatial
{
    public partial class XYZ
    {
        public XYZ(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public double Magnitude 
        { 
            get => Math.Sqrt((double)X * X + (double)Y * Y + (double)Z * Z); 
        }
    }
}
