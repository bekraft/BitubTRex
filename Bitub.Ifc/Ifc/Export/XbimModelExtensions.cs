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

using Google.Protobuf.Collections;

namespace Bitub.Ifc.Export
{
    public static class XbimModelExtensions
    {
        #region Point context 

        public static XYZ ToXYZ(this XbimPoint3D p, XbimVector3D scale)
        {
            return new XYZ()
            {
                X = (float)(p.X * scale.X),
                Y = (float)(p.Y * scale.Y),
                Z = (float)(p.Z * scale.Z)
            };
        }

        public static void AppendTo(this XbimPoint3D p, RepeatedField<double> f, XbimVector3D scale)
        {
            f.Add((p.X * scale.X));
            f.Add((p.Y * scale.Y));
            f.Add((p.Z * scale.Z));
        }

        public static void AppendTo(this XbimPoint3D p, RepeatedField<float> f, XbimVector3D scale)
        {
            f.Add((float)(p.X * scale.X));
            f.Add((float)(p.Y * scale.Y));
            f.Add((float)(p.Z * scale.Z));
        }

        #endregion

        #region XbimVector3D context

        public static XYZ ToXYZ(this XbimVector3D v)
        {
            return new XYZ()
            {
                X = (float)v.X,
                Y = (float)v.Y,
                Z = (float)v.Z
            };
        }

        public static XYZ ToXYZ(this XbimVector3D v, XbimVector3D scale)
        {
            return new XYZ()
            {
                X = (float)(v.X * scale.X),
                Y = (float)(v.Y * scale.Y),
                Z = (float)(v.Z * scale.Z)
            };
        }

        public static void AppendTo(this XbimVector3D v, RepeatedField<double> f)
        {
            f.Add(v.X);
            f.Add(v.Y);
            f.Add(v.Z);
        }

        public static void AppendTo(this XbimVector3D v, RepeatedField<double> f, XbimVector3D scale)
        {
            f.Add((v.X * scale.X));
            f.Add((v.Y * scale.Y));
            f.Add((v.Z * scale.Z));
        }

        public static void AppendTo(this XbimVector3D v, RepeatedField<float> f)
        {
            f.Add((float)v.X);
            f.Add((float)v.Y);
            f.Add((float)v.Z);
        }


        public static void AppendTo(this XbimVector3D v, RepeatedField<float> f, XbimVector3D scale)
        {
            f.Add((float)(v.X * scale.X));
            f.Add((float)(v.Y * scale.Y));
            f.Add((float)(v.Z * scale.Z));
        }

        #endregion

        #region XbimMatrix3D context

        public static Dto.Scene.Transform ToRotation(this XbimMatrix3D t, XbimVector3D scale)
        {
            return new Dto.Scene.Transform
            {                
                R = new Rotation
                {   // XbimMatrix is transposed (left hand chaining)
                    Rx = new XbimVector3D(t.M11, t.M21, t.M31).Normalized().ToXYZ(),
                    Ry = new XbimVector3D(t.M12, t.M22, t.M32).Normalized().ToXYZ(),
                    Rz = new XbimVector3D(t.M13, t.M23, t.M33).Normalized().ToXYZ(),
                },
                T = t.Translation.ToXYZ(scale)
            };
        }

        public static Dto.Scene.Transform ToQuaternion(this XbimMatrix3D t, XbimVector3D scale)
        {
            var q = t.GetRotationQuaternion();
            return new Dto.Scene.Transform
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

        #endregion

        public static ABox ToABox(this XbimRect3D rect3D, XbimVector3D scale, Func<XbimPoint3D, XbimPoint3D> adapter = null)
        {
            return new ABox
            {
                Min = adapter?.Invoke(rect3D.Min).ToXYZ(scale) ?? rect3D.Min.ToXYZ(scale),
                Max = adapter?.Invoke(rect3D.Max).ToXYZ(scale) ?? rect3D.Max.ToXYZ(scale)
            };
        }

        public static BoundingBox ToBoundingBox(this XbimRect3D rect3D, XbimVector3D scale, Func<XbimPoint3D, XbimPoint3D> adapter = null)
        {
            return new BoundingBox
            {
                ABox = rect3D.ToABox(scale, adapter)
            };
        }

        public static Region ToRegion(this XbimRegion r, XbimVector3D scale, Func<XbimPoint3D, XbimPoint3D> adapter = null)
        {
            return new Region
            {
                BoundingBox = r.ToXbimRect3D().ToBoundingBox(scale, adapter),
                Population = r.Population,
                Label = r.Name
            };
        }

        public static RefId ToRefId(this IIfcRoot entity, SceneComponentIdentificationStrategy strategy)
        {
            switch (strategy)
            {
                case SceneComponentIdentificationStrategy.UseGloballyUniqueID:
                    return new RefId { Sid = entity.GlobalId.ToGlobalUniqueId().ToQualifier() };
                case SceneComponentIdentificationStrategy.UseIfcInstanceLabel:
                    return new RefId { Nid = entity.EntityLabel };
                default:
                    throw new NotImplementedException();
            }
        }

        public static Component ToComponent(this IIfcProduct product, 
            out int? optParentLabel, IDictionary<Type, Classifier> ifcClassifierMap, SceneComponentIdentificationStrategy strategy)
        {
            var parent = product.Parent<IIfcProduct>().FirstOrDefault();
            var component = new Component
            {
                Id = product.ToRefId(strategy),
                Parent = parent?.ToRefId(strategy),
                Name = product.Name ?? "",
            };

            // Add IFC express types inheritance by default
            component.Concepts.Add(ifcClassifierMap[product.GetType()]);

            optParentLabel = parent?.EntityLabel;
            return component;
        }

        #region Colouring and materials

        /// <summary>
        /// Mapping to a color.
        /// </summary>
        /// <param name="c">The color instance</param>
        /// <returns>The component color instance</returns>
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

        public static IEnumerable<Material> ToMaterialBySurfaceStyles(this IModel model)
        {
            foreach (var style in model.Instances.OfType<IIfcSurfaceStyle>())
                yield return style.ToMaterial();
        }

        /// <summary>
        /// Converts a default colouring entry of an IFC type into a material reference using given type ID and typeID-2-RefId mapper.
        /// </summary>
        /// <param name="defaultColorMap">The default color map</param>
        /// <param name="model">The model and its metadata of concern</param>
        /// <param name="typeID">The type ID</param>
        /// <param name="generator">The RefID generator</param>
        /// <returns>A material using found colour specification as Albedo channel.</returns>
        public static Material ToMaterialByIfcTypeID(this XbimColourMap defaultColorMap, IModel model, int typeID, Func<int, RefId> generator)
        {
            var defaultStyle = model.Metadata.GetType((short)typeID);
            var defaultColor = defaultColorMap[defaultStyle.Name];
            var defaultMaterial = new Material
            {
                Name = defaultStyle.Name,
                Id = generator?.Invoke(typeID) ?? new RefId { Nid = typeID }
            };
            defaultMaterial.ColorChannels.Add(new ColorOrNormalised
            {
                Channel = ColorChannel.Albedo,
                Color = defaultColor.ToColor(),
            });
            return defaultMaterial;
        }

        /// <summary>
        /// In principal using <see cref="ToMaterialByIfcTypeID(XbimColourMap, IModel, int, Func{int, RefId})"/>. Maps multiple types in a sequence.
        /// </summary>
        /// <param name="defaultColorMap">The default color map</param>
        /// <param name="model">The model and its metadata of concern</param>
        /// <param name="typeIDs">The type ID sequence</param>
        /// <param name="generator">The RefID generator</param>
        /// <returns>A material using found colour specification as Albedo channel.</returns>
        public static IEnumerable<Material> ToMaterialByIfcTypeIDs(this XbimColourMap defaultColorMap, IModel model, IEnumerable<int> typeIDs, Func<int, RefId> generator)
        {
            return typeIDs.Select(typeID => ToMaterialByIfcTypeID(defaultColorMap, model, typeID, generator));
        }

        #endregion
    }
}
