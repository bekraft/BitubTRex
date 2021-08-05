using System;
using System.Collections.Generic;

using Bitub.Dto.Spatial;

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

        public XYZ this[int index]
        {
            get
            {
                switch(index)
                {
                    case 0:
                        return Rx;
                    case 1:
                        return Ry;
                    case 2:
                        return Rz;
                }
                throw new ArgumentException($"{index} out of range");
            }
        }

        public IEnumerable<XYZ> Row
        {
            get => new[] { Rx, Ry, Rz };
        }

        public static Rotation operator*(Rotation r, float scale)
        {
            r.Rx = r.Rx.Scale(scale);
            r.Ry = r.Ry.Scale(scale);
            r.Rz = r.Rz.Scale(scale);
            return r;
        }
    }
}
