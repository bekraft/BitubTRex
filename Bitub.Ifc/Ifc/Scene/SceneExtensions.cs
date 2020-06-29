using Bitub.Transfer.Spatial;
using Xbim.Common.Geometry;

namespace Bitub.Ifc.Scene
{
    public static class SceneExtensions
    {
        public static XbimVector3D ToXbimVector3D(this XYZ xyz, double scale = 1.0)
        {
            return new XbimVector3D(xyz.X * scale, xyz.Y * scale, xyz.Z * scale);
        }

        public static XbimPoint3D ToXbimPoint3D(this XYZ xyz, double scale = 1.0)
        {
            return new XbimPoint3D(xyz.X * scale, xyz.Y * scale, xyz.Z * scale);
        }
    }
}
