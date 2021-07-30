using System;

namespace Bitub.Dto.Scene
{
    public partial class Rotation
    {
        public static Rotation Identity 
        {
            get => new Rotation
            {
                Rx = new Spatial.XYZ { X = 1, Y = 0, Z = 0 },
                Ry = new Spatial.XYZ { X = 0, Y = 1, Z = 0 },
                Rz = new Spatial.XYZ { X = 0, Y = 0, Z = 1 },
            };
        }
    }
}
