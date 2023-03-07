using System;
using System.Collections.Generic;
using Bitub.Dto.Spatial;

namespace Bitub.Dto.Scene
{
    public partial class Rotation
    {
        public static Rotation Identity => new Rotation
        {
            Rx = new XYZ { X = 1, Y = 0, Z = 0 },
            Ry = new XYZ { X = 0, Y = 1, Z = 0 },
            Rz = new XYZ { X = 0, Y = 0, Z = 1 },
        };
        

        public XYZ this[int index] => index switch
        {
            0 => Rx,
            1 => Ry,
            2 => Rz,
            _ => throw new ArgumentException($"{index} out of range"),
        };

        public IEnumerable<XYZ> Row => new[] { Rx, Ry, Rz };

        public static Rotation operator*(Rotation r, float scale)
        {
            r.Rx = r.Rx.Scale(scale);
            r.Ry = r.Ry.Scale(scale);
            r.Rz = r.Rz.Scale(scale);
            return r;
        }

        // Credits to https://www.euclideanspace.com/maths/geometry/rotations/conversions/matrixToQuaternion/index.htm
        public Quaternion ToQuaternion()
        {
            double tr = Rx.X + Ry.Y + Rz.Z;
            double qw = 0;
            double qx = 0;
            double qy = 0; 
            double qz = 0;

            if (tr > 0) { 
                double S = Math.Sqrt(tr + 1.0) * 2; // S=4*qw 
                qw = 0.25 * S;
                qx = (Rz.Y - Ry.Z) / S;
                qy = (Rx.Z - Rz.X) / S; 
                qz = (Ry.X - Rx.Y) / S; 
            } 
            else if ((Rx.X > Ry.Y)&(Rx.X > Rz.Z)) 
            { 
                double S = Math.Sqrt(1.0 + Rx.X - Ry.Y - Rz.Z) * 2; // S=4*qx 
                qw = (Rz.Y - Ry.Z) / S;
                qx = 0.25 * S;
                qy = (Rx.Y + Ry.X) / S; 
                qz = (Rx.Z + Rz.X) / S; 
            } 
            else if (Ry.Y > Rz.Z) 
            { 
                double S = Math.Sqrt(1.0 + Ry.Y - Rx.X - Rz.Z) * 2; // S=4*qy
                qw = (Rx.Z - Rz.X) / S;
                qx = (Rx.Y + Ry.X) / S; 
                qy = 0.25 * S;
                qz = (Ry.Z + Rz.Y) / S; 
            } 
            else 
            { 
                double S = Math.Sqrt(1.0 + Rz.Z - Rx.X - Ry.Y) * 2; // S=4*qz
                qw = (Ry.X - Rx.Y) / S;
                qx = (Rx.Z + Rz.X) / S;
                qy = (Ry.Z + Rz.Y) / S;
                qz = 0.25 * S;
            }

            return new Quaternion() { X = (float)qx, Y = (float)qy, Z = (float)qz, W = (float)qw };
        }
    }
}
