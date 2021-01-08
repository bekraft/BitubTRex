using System;
using System.Linq;
using System.Collections.Generic;

using Xbim.Common;
using Xbim.Common.Geometry;

using Xbim.Ifc;
using Xbim.Ifc4.Interfaces;

using Bitub.Dto;
using Bitub.Dto.Scene;
using Bitub.Dto.Spatial;
using Bitub.Dto.Concept;

using Bitub.Ifc.Concept;

using Google.Protobuf.Collections;

namespace Bitub.Ifc.Export
{
    public static class XbimModelExtensions
    {
        public static XYZ ToXYZ(this XbimPoint3D p, float scale = 1.0f)
        {
            return new XYZ()
            {
                X = (float)(p.X * scale),
                Y = (float)(p.Y * scale),
                Z = (float)(p.Z * scale)
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

        public static XYZ ToXYZ(this XbimVector3D v, float scale = 1.0f)
        {
            return new XYZ()
            {
                X = (float)(v.X * scale),
                Y = (float)(v.Y * scale),
                Z = (float)(v.Z * scale)
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

        public static Bitub.Dto.Scene.Transform ToRotation(this XbimMatrix3D t, float scale = 1.0f)
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

        public static Bitub.Dto.Scene.Transform ToQuaternion(this XbimMatrix3D t, float scale = 1.0f)
        {
            var q = t.GetRotationQuaternion();
            return new Bitub.Dto.Scene.Transform
            {
                Q = new Quaternion
                {
                    X = (float)q.X,
                    Y = (float)q.Y,
                    Z = (float)q.Z,
                    W = (float)q.W
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

        public static ABox ToABox(this XbimRect3D rect3D, float scale = 1.0f, Func<XbimPoint3D, XbimPoint3D> adapter = null)
        {
            return new ABox
            {
                Min = adapter?.Invoke(rect3D.Min).ToXYZ(scale) ?? rect3D.Min.ToXYZ(scale),
                Max = adapter?.Invoke(rect3D.Max).ToXYZ(scale) ?? rect3D.Max.ToXYZ(scale)
            };
        }

        public static BoundingBox ToBoundingBox(this XbimRect3D rect3D, float scale = 1.0f, Func<XbimPoint3D, XbimPoint3D> adapter = null)
        {
            return new BoundingBox
            {
                ABox = rect3D.ToABox(scale, adapter)
            };
        }

        public static Region ToRegion(this XbimRegion r, float scale = 1.0f, Func<XbimPoint3D, XbimPoint3D> adapter = null)
        {
            return new Region
            {
                BoundingBox = r.ToXbimRect3D().ToBoundingBox(scale, adapter),
                Population = r.Population,
                Label = r.Name
            };
        }

        public static IDictionary<Type, Classifier> ToIfcClassifierMap(this IModel model)
        {
            return model.SchemaVersion.ToImplementingClassification<IIfcProduct>();
        }

        public static Component ToClassifedComponentWith(this Component component, IIfcProduct product, CanonicalFilter featureToClassifierFilter)
        {
            if (null != featureToClassifierFilter)
            {
                foreach (var featureConcept in product.ToFeatures<IIfcSimpleProperty>(featureToClassifierFilter))
                    component.Concepts.Add(featureConcept.ToClassifierOnValueEquals());
            }
            return component;
        }

        public static Component ToFullyFeaturedWith(this Component component, IIfcProduct product, CanonicalFilterRule featureFilterRule)
        {
            component.Features.AddRange(Enumerable
                // Add all, id, name and additional features
                .Concat(product.ToBaseFeatures(), product.ToFeatures<IIfcSimpleProperty>())
                // And filter by rule
                .Where(f => featureFilterRule.IsAcceptedBy(f.Canonical)));
            return component;
        }

        public static Component ToComponent(this IIfcProduct product, out int? optParentLabel, IDictionary<Type, Classifier> ifcClassifierMap)
        {
            var parent = product.Parent<IIfcProduct>().FirstOrDefault();
            var component = new Component
            {
                Id = product.GlobalId.ToGlobalUniqueId(),
                // -1 reserved for roots
                Parent = parent?.GlobalId.ToGlobalUniqueId(),
                Name = product.Name ?? "",
            };

            // Add IFC express types inheritance by default
            component.Concepts.Add(ifcClassifierMap[product.GetType()]);

            component.Children.AddRange(product.Children<IIfcProduct>().Select(p => p.GlobalId.ToGlobalUniqueId()));
            optParentLabel = parent?.EntityLabel;
            return component;
        }

        public static IEnumerable<Material> ToMaterialBySurfaceStyles(this IModel model)
        {
            foreach (var style in model.Instances.OfType<IIfcSurfaceStyle>())
                yield return style.ToMaterial();
        }

        public static IEnumerable<Material> ToMaterialByColorMap(this IModel model, XbimColourMap defaultColorMap, IEnumerable<RefId> negativeTypeNids)
        {
            foreach (var rid in negativeTypeNids)
            {
                var defaultStyle = model.Metadata.GetType((short)Math.Abs(rid.Nid));
                var defaultColor = defaultColorMap[defaultStyle.Name];
                var defaultMaterial = new Material
                {
                    Name = defaultStyle.Name,
                    Id = rid
                };
                defaultMaterial.ColorChannels.Add(new ColorOrNormalised
                {
                    Channel = ColorChannel.Albedo,
                    Color = defaultColor.ToColor(),
                });
                yield return defaultMaterial;
            }
        }

    }
}
