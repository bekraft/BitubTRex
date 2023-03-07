using System.Xml;
using System.Linq.Expressions;
using Bitub.Dto.Spatial;

namespace Bitub.Dto.Scene
{
    public partial class Transform
    {
        public static Transform Identity 
            => new Transform() { T = XYZ.Zero, R = Rotation.Identity };

        public static Transform MirrorX
            => new Transform { T = XYZ.Zero, R = new Rotation { Rx = XYZ.OneX * -1, Ry = XYZ.OneY, Rz = XYZ.OneZ } };

        public static Transform MirrorY
            => new Transform { T = XYZ.Zero, R = new Rotation { Rx = XYZ.OneX, Ry = XYZ.OneY * -1, Rz = XYZ.OneZ } };

        public static Transform MirrorZ
            => new Transform { T = XYZ.Zero, R = new Rotation { Rx = XYZ.OneX, Ry = XYZ.OneY, Rz = XYZ.OneZ * -1 } };
    
    }
}