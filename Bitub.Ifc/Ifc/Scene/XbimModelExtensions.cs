using System;
using System.Linq;

using Xbim.Common.Geometry;
using Xbim.Ifc;

using Bitub.Dto.Scene;
using Bitub.Dto.Spatial;

using Google.Protobuf.Collections;
using Xbim.Ifc4.UtilityResource;

namespace Bitub.Ifc.Scene
{
    public static class XbimModelExtensions
    {
        public static XYZ ToXYZ(this XbimPoint3D p, double scale = 1.0)
        {
            return new XYZ()
            {
                X = (p.X * scale),
                Y = (p.Y * scale),
                Z = (p.Z * scale)
            };
        }

        public static void AppendTo(this XbimPoint3D p, RepeatedField<double> f, double scale = 1.0)
        {
            f.Add((p.X * scale));
            f.Add((p.Y * scale));
            f.Add((p.Z * scale));
        }

        public static void AppendTo(this XbimPoint3D p, RepeatedField<float> f, double scale = 1.0)
        {
            f.Add((float)(p.X * scale));
            f.Add((float)(p.Y * scale));
            f.Add((float)(p.Z * scale));
        }

        public static XYZ ToXYZ(this XbimVector3D v, double scale = 1.0)
        {
            return new XYZ()
            {
                X = (v.X * scale),
                Y = (v.Y * scale),
                Z = (v.Z * scale)
            };
        }

        public static void AppendTo(this XbimVector3D v, RepeatedField<double> f, double scale = 1.0)
        {
            f.Add((v.X * scale));
            f.Add((v.Y * scale));
            f.Add((v.Z * scale));
        }

        public static void AppendTo(this XbimVector3D v, RepeatedField<float> f, double scale = 1.0)
        {
            f.Add((float)(v.X * scale));
            f.Add((float)(v.Y * scale));
            f.Add((float)(v.Z * scale));
        }

        public static Bitub.Dto.Scene.Transform ToRotation(this XbimMatrix3D t, double scale = 1.0)
        {
            return new Bitub.Dto.Scene.Transform
            {                
                R = new Rotation
                {   // XbimMatrix is transposed (left hand chaining)
                    Rx = new XbimVector3D(t.M11, t.M21, t.M31).ToXYZ(),
                    Ry = new XbimVector3D(t.M12, t.M22, t.M32).ToXYZ(),
                    Rz = new XbimVector3D(t.M13, t.M23, t.M33).ToXYZ(),
                },
                T = t.Translation.ToXYZ(scale)
            };
        }

        public static Bitub.Dto.Scene.Transform ToQuaternion(this XbimMatrix3D t, double scale = 1.0)
        {
            var q = t.GetRotationQuaternion();
            return new Bitub.Dto.Scene.Transform
            {
                Q = new Quaternion
                {
                    X = q.X,
                    Y = q.Y,
                    Z = q.Z,
                    W = q.W
                },
                T = t.Translation.ToXYZ(scale)
            };
        }

        public static Color ToColor(this XbimColour c)
        {
            return new Color()
            {
                R = c?.Red ?? 0.75f,
                G = c?.Green ?? 0.75f,
                B = c?.Blue ?? 0.75f,
                A = c?.Alpha ?? 1.0f,
            };
        }

        public static ABox ToABox(this XbimRect3D rect3D, double scale = 1.0, Func<XbimPoint3D, XbimPoint3D> adapter = null)
        {
            return new ABox
            {
                Min = adapter?.Invoke(rect3D.Min).ToXYZ(scale) ?? rect3D.Min.ToXYZ(scale),
                Max = adapter?.Invoke(rect3D.Max).ToXYZ(scale) ?? rect3D.Max.ToXYZ(scale)
            };
        }

        public static BoundingBox ToBoundingBox(this XbimRect3D rect3D, double scale = 1.0, Func<XbimPoint3D, XbimPoint3D> adapter = null)
        {
            return new BoundingBox
            {
                ABox = rect3D.ToABox(scale, adapter)
            };
        }

        public static Region ToRegion(this XbimRegion r, double scale = 1.0, Func<XbimPoint3D, XbimPoint3D> adapter = null)
        {
            return new Region
            {
                BoundingBox = r.ToXbimRect3D().ToBoundingBox(scale, adapter),
                Population = r.Population,
                Label = r.Name
            };
        }
    } 
}
