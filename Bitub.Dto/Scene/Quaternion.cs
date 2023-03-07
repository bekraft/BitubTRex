using Bitub.Dto.Spatial;

namespace Bitub.Dto.Scene
{
    public partial class Quaternion
    {
        public static Quaternion Identity => new Quaternion() { X = 0, Y = 0, Z = 0, W = 1 };

        // Credits to https://www.euclideanspace.com/maths/geometry/rotations/conversions/quaternionToMatrix/index.htm
        public Rotation ToRotation()
        {
            var rx = XYZ.OneX;
            var ry = XYZ.OneY;
            var rz = XYZ.OneZ;
            
            double sqw = W * W;
            double sqx = X * X;
            double sqy = Y * Y;
            double sqz = Z * Z;

            // invs (inverse square length) is only required if quaternion is not already normalised
            double invs = 1 / (sqx + sqy + sqz + sqw);
            rx.X = (float)(( sqx - sqy - sqz + sqw) * invs); // since sqw + sqx + sqy + sqz =1/invs * invs
            ry.Y = (float)((-sqx + sqy - sqz + sqw) * invs);
            rz.Z = (float)((-sqx - sqy + sqz + sqw) * invs);
            
            double tmp1 = X * Y;
            double tmp2 = Z * W;
            ry.X = (float)(2.0 * (tmp1 + tmp2) * invs);
            rx.Y = (float)(2.0 * (tmp1 - tmp2) * invs);
            
            tmp1 = X * Z;
            tmp2 = Y * W;
            rz.X = (float)(2.0 * (tmp1 - tmp2) * invs);
            rx.Z = (float)(2.0 * (tmp1 + tmp2) * invs);
            tmp1 = Y * Z;
            tmp2 = X * W;
            rz.Y = (float)(2.0 * (tmp1 + tmp2) * invs);
            ry.Z = (float)(2.0 * (tmp1 - tmp2) * invs);
            return new Rotation() { Rx = rx, Ry = ry, Rz = rz };
        }
    }
}