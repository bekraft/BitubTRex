using Xbim.Ifc;
using Xbim.Ifc4.Interfaces;

using Xbim.Common;
using Xbim.Common.Geometry;

using System;
using System.Linq;
using System.Collections.Generic;

using Bitub.Ifc.Transform;

namespace Bitub.Ifc
{
    public static class IfcModelExtensions
    {
        /// <summary>
        /// A new IfcDirection instance given a vector.
        /// </summary>
        /// <typeparam name="T">The IfcPoint type</typeparam>
        /// <param name="s">The model</param>
        /// <param name="v">The vector</param>
        /// <param name="scaleUp">Whether to scale up the vector onto model scale</param>
        /// <returns>A new direction</returns>
        public static T NewIfcDirection<T>(this IModel s, XbimVector3D v, bool scaleUp = false) where T : IIfcDirection, IInstantiableEntity
        {
            var d = s.Instances.New<T>();
            var coordinates = scaleUp ? v.ToDoubleModelScale(s.ModelFactors) : v.ToDouble();
            foreach (var c in coordinates) 
                d.DirectionRatios.Add(c);

            return d;
        }

        /// <summary>
        /// A new IfcPoint instance given a vector.
        /// </summary>
        /// <typeparam name="T">The IfcPoint type</typeparam>
        /// <param name="s">The model</param>
        /// <param name="v">The vector</param>
        /// <param name="scaleUp">Whether to scale up the vector onto model scale</param>
        /// <returns>A new point</returns>
        public static T NewIfcPoint<T>(this IModel s, XbimVector3D v, bool scaleUp = false) where T : IIfcCartesianPoint, IInstantiableEntity
        {
            var p = s.Instances.New<T>();
            var coordinates = scaleUp ? v.ToDoubleModelScale(s.ModelFactors) : v.ToDouble();
            foreach (var c in coordinates) 
                p.Coordinates.Add(c);
            
            return p;
        }

        /// <summary>
        /// A new IfcPoint instance given a vector.
        /// </summary>
        /// <typeparam name="T">The IfcPoint type</typeparam>
        /// <param name="s">The model</param>
        /// <param name="v">The vector</param>
        /// <param name="scaleUp">Whether to scale up the vector onto model scale</param>
        /// <returns>A new point</returns>
        public static T NewIfcPoint<T>(this IModel s, double[] v, bool scaleUp = false) where T : IIfcCartesianPoint, IInstantiableEntity
        {            
            var p = s.Instances.New<T>();
            var coordinates = scaleUp ? v
                .Select(x => x * s.ModelFactors.OneMeter)
                .Select(x => x < s.ModelFactors.Precision ? 0 : x) : v;
            foreach (var c in coordinates)
                p.Coordinates.Add(c);
            return p;
        }

        public static T NewIfcPoint<T>(this IModel s, Action<T> modifier) where T : IIfcCartesianPoint, IInstantiableEntity
        {
            var p = s.Instances.New<T>(modifier);
            return p;
        }

        public static List<T> NewIfcPoints<T>(this IModel s, IEnumerable<double[]> points, bool scaleUp = false) where T : IIfcCartesianPoint, IInstantiableEntity
        {
            return points.Select(c => s.NewIfcPoint<T>(c, scaleUp)).ToList();
        }

        public static List<T> NewIfcPoints<T>(this IModel s, IEnumerable<XbimVector3D> points, bool scaleUp = false) where T : IIfcCartesianPoint, IInstantiableEntity
        {
            return points.Select(c => s.NewIfcPoint<T>(c, scaleUp)).ToList();
        }

        public static T NewIfcColourRgb<T>(this IModel s, float red, float green, float blue) where T : IIfcColourRgb, IInstantiableEntity
        {
            var colour = s.Instances.New<T>();
            colour.Blue = blue;
            colour.Red = red;
            colour.Green = green;
            return colour;
        }

        public static IIfcLocalPlacement NewLocalPlacement(this IModel s, XbimVector3D refPosition, XbimVector3D refAxis, double factor = 1.0)
        {
            switch (s.SchemaVersion)
            {
                case Xbim.Common.Step21.XbimSchemaVersion.Ifc2X3:
                    return s.NewIfc2x3FullPlacement(refPosition * factor, refAxis);
                case Xbim.Common.Step21.XbimSchemaVersion.Ifc4:
                case Xbim.Common.Step21.XbimSchemaVersion.Ifc4x1:
                    return s.NewIfc4FullPlacement(refPosition * factor, refAxis);
            }
            throw new NotImplementedException($"Not implemented schema version ${s.SchemaVersion}");
        }

        public static IIfcLocalPlacement NewLocalPlacement(this IModel s, XbimVector3D refPosition, bool scaleUp = false)
        {
            switch(s.SchemaVersion)
            {
                case Xbim.Common.Step21.XbimSchemaVersion.Ifc2X3:
                    return s.NewIfc2x3LocalPlacement(refPosition, scaleUp);
                case Xbim.Common.Step21.XbimSchemaVersion.Ifc4:
                case Xbim.Common.Step21.XbimSchemaVersion.Ifc4x1:
                    return s.NewIfc4LocalPlacement(refPosition, scaleUp);
            }
            throw new NotImplementedException($"Not implemented schema version ${s.SchemaVersion}");
        }

        public static IIfcLocalPlacement NewLocalPlacement(this IModel s, XbimMatrix3D transform, bool scaleUp = false)
        {
            switch (s.SchemaVersion)
            {
                case Xbim.Common.Step21.XbimSchemaVersion.Ifc2X3:
                    return s.NewIfc2x3FullPlacement(transform, scaleUp);
                case Xbim.Common.Step21.XbimSchemaVersion.Ifc4:
                case Xbim.Common.Step21.XbimSchemaVersion.Ifc4x1:
                    return s.NewIfc4FullPlacement(transform, scaleUp);
            }
            throw new NotImplementedException($"Not implemented schema version ${s.SchemaVersion}");
        }

        public static IIfcRelAggregates NewDecomposes(this IModel s, IIfcObjectDefinition host)
        {
            switch(s.SchemaVersion)
            {
                case Xbim.Common.Step21.XbimSchemaVersion.Ifc2X3:
                    return s.NewIfc2x3Decomposes(host as Xbim.Ifc2x3.Kernel.IfcObjectDefinition);
                case Xbim.Common.Step21.XbimSchemaVersion.Ifc4:
                case Xbim.Common.Step21.XbimSchemaVersion.Ifc4x1:
                    return s.NewIfc4Decomposes(host as Xbim.Ifc4.Kernel.IfcObjectDefinition);
            }
            throw new NotImplementedException($"Not implemented schema version ${s.SchemaVersion}");
        }

        public static IIfcRelContainedInSpatialStructure NewContains(this IModel s, IIfcSpatialStructureElement e)
        {
            switch(s.SchemaVersion)
            {
                case Xbim.Common.Step21.XbimSchemaVersion.Ifc2X3:
                    return s.NewIfc2x3Contains(e as Xbim.Ifc2x3.ProductExtension.IfcSpatialStructureElement);
                case Xbim.Common.Step21.XbimSchemaVersion.Ifc4:
                case Xbim.Common.Step21.XbimSchemaVersion.Ifc4x1:
                    return s.NewIfc4Contains(e as Xbim.Ifc4.ProductExtension.IfcSpatialStructureElement);
            }
            throw new NotImplementedException($"Not implemented schema version ${s.SchemaVersion}");
        }

        public static IIfcSIUnit NewIfcSIUnit(this IModel s, IfcUnitEnum unitType, IfcSIUnitName name, IfcSIPrefix? prefix = null)
        {   
            switch(s.SchemaVersion)
            {
                case Xbim.Common.Step21.XbimSchemaVersion.Ifc2X3:
                    return s.NewIfc2x3SIUnit(unitType, name, prefix);
                case Xbim.Common.Step21.XbimSchemaVersion.Ifc4:
                case Xbim.Common.Step21.XbimSchemaVersion.Ifc4x1:
                    return s.Instances.New<Xbim.Ifc4.MeasureResource.IfcSIUnit>(x =>
                    {
                        x.Name = name;
                        x.Prefix = prefix;
                        x.UnitType = unitType;
                    });
                default:
                    throw new NotImplementedException($"Missing implementation for {s.SchemaVersion}");
            }
        }

        public static IIfcPropertySet NewIfcPropertySet(this IModel s, string setName, string description = null)
        {
            switch(s.SchemaVersion)
            {
                case Xbim.Common.Step21.XbimSchemaVersion.Ifc2X3:
                    return s.NewIfc2x3PropertySet(setName, description);
                case Xbim.Common.Step21.XbimSchemaVersion.Ifc4:
                case Xbim.Common.Step21.XbimSchemaVersion.Ifc4x1:
                    return s.NewIfc4PropertySet(setName, description);
                default:
                    throw new NotImplementedException($"Missing implementation for {s.SchemaVersion}");
            }
        }

        public static IIfcRelDefinesByProperties NewIfcRelDefinesByProperties(this IModel s, IIfcPropertySetDefinitionSelect set)
        {
            switch (s.SchemaVersion)
            {
                case Xbim.Common.Step21.XbimSchemaVersion.Ifc2X3:
                    throw new NotSupportedException($"{typeof(IIfcPropertySetDefinitionSelect)} is not supported by IFC2x3");
                case Xbim.Common.Step21.XbimSchemaVersion.Ifc4:
                case Xbim.Common.Step21.XbimSchemaVersion.Ifc4x1:
                    return s.NewIfc4PropertyRelation(set as Xbim.Ifc4.Kernel.IfcPropertySetDefinitionSelect);
                default:
                    throw new NotImplementedException($"Missing implementation for {s.SchemaVersion}");
            }
        }

        public static IIfcRelDefinesByProperties NewIfcRelDefinesByProperties(this IModel s, IIfcPropertySet set)
        {
            if (set.Model.SchemaVersion != s.SchemaVersion)
                throw new ArgumentException($"Misaligned schema versions {s.SchemaVersion} != {set.Model.SchemaVersion}");

            switch(s.SchemaVersion)
            {
                case Xbim.Common.Step21.XbimSchemaVersion.Ifc2X3:
                    return s.NewIfc2x3PropertyRelation(set as Xbim.Ifc2x3.Kernel.IfcPropertySet);
                case Xbim.Common.Step21.XbimSchemaVersion.Ifc4:
                case Xbim.Common.Step21.XbimSchemaVersion.Ifc4x1:
                    return s.NewIfc4PropertyRelation(set as Xbim.Ifc4.Kernel.IfcPropertySet);
                default:
                    throw new NotImplementedException($"Missing implementation for {s.SchemaVersion}");
            }
        }
    }
}