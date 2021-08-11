using Xbim.Ifc;
using Xbim.Ifc4.UtilityResource;
using Xbim.Ifc4.DateTimeResource;
using Xbim.Ifc4.ActorResource;
using Xbim.Ifc4.Interfaces;
using Xbim.Ifc4.GeometricConstraintResource;
using Xbim.Ifc4.GeometryResource;
using Xbim.Ifc4.Kernel;
using Xbim.Ifc4.MeasureResource;
using Xbim.Ifc4.ProductExtension;
using Xbim.Ifc4.RepresentationResource;
using Xbim.Ifc4.PresentationAppearanceResource;

using System.Linq;

using Xbim.Common;
using System;
using Xbim.Ifc4.TopologyResource;
using System.Collections.Generic;
using Xbim.Common.Geometry;
using Xbim.Ifc4.PropertyResource;
using Bitub.Ifc.Transform;
using Microsoft.Extensions.Logging;
using Bitub.Ifc.Export;

namespace Bitub.Ifc
{
    public static class Ifc4EntityCreatorExtensions
    {
        public static IfcGeometricRepresentationContext NewIfc4GeometricContext(this IModel s, string contextId = null, string contextType = null)
        {
            var context = s.Instances.New<IfcGeometricRepresentationContext>();
            context.ContextIdentifier = contextId;
            context.ContextType = contextType;
            context.Precision = 0.00001;
            context.CoordinateSpaceDimension = 3;

            var axis = s.Instances.New<IfcAxis2Placement3D>();
            axis.Location = s.Instances.New<IfcCartesianPoint>(p => p.SetXYZ(0, 0, 0));

            context.WorldCoordinateSystem = axis;

            return context;
        }

        public static IfcLocalPlacement NewIfc4FullPlacement(this IModel s, IfcCartesianPoint refPoint, IfcDirection refAxis, IfcDirection axis = null)
        {
            var localPlacement = s.Instances.New<IfcLocalPlacement>();
            var placement = s.Instances.New<IfcAxis2Placement3D>();
            placement.Axis = axis ?? s.NewIfcDirection<IfcDirection>(new XbimVector3D(0, 0, 1));
            placement.RefDirection = refAxis;
            placement.Location = refPoint;
            localPlacement.RelativePlacement = placement;
            return localPlacement;
        }

        public static IfcLocalPlacement NewIfc4LocalPlacement(this IModel s, IfcCartesianPoint refPoint)
        {
            var localPlacement = s.Instances.New<IfcLocalPlacement>();
            var placement = s.Instances.New<IfcAxis2Placement3D>();
            placement.Location = refPoint;
            localPlacement.RelativePlacement = placement;
            return localPlacement;
        }

        public static IfcLocalPlacement NewIfc4LocalPlacement(this IModel s, XbimVector3D refPoint, bool scaleUp = false)
        {
            var localPlacement = s.Instances.New<IfcLocalPlacement>();
            var placement = s.Instances.New<IfcAxis2Placement3D>();
            placement.Location = s.NewIfcPoint<IfcCartesianPoint>(refPoint, scaleUp);
            localPlacement.RelativePlacement = placement;
            return localPlacement;
        }

        public static IfcLocalPlacement NewIfc4FullPlacement(this IModel s, XbimVector3D refPoint, XbimVector3D refAxis, bool scaleUp = false)
        {
            var localPlacement = s.Instances.New<IfcLocalPlacement>();
            var placement = s.Instances.New<IfcAxis2Placement3D>();
            placement.Location = s.NewIfcPoint<IfcCartesianPoint>(refPoint, scaleUp);
            placement.RefDirection = s.NewIfcDirection<IfcDirection>(refAxis);
            placement.Axis = s.NewIfcDirection<IfcDirection>(new XbimVector3D(0, 0, 1));
            localPlacement.RelativePlacement = placement;
            return localPlacement;
        }

        public static IfcLocalPlacement NewIfc4FullPlacement(this IModel s, XbimMatrix3D transform, bool scaleUp = false)
        {
            var localPlacement = s.Instances.New<IfcLocalPlacement>();
            var placement = s.Instances.New<IfcAxis2Placement3D>();
            placement.Location = s.NewIfcPoint<IfcCartesianPoint>(transform.Translation, scaleUp);

            if (!transform.GetRotationQuaternion().IsIdentity())
            {
                placement.RefDirection = s.NewIfcDirection<IfcDirection>(transform.Backward, false);
                placement.Axis = s.NewIfcDirection<IfcDirection>(transform.Up, false);
            }
            localPlacement.RelativePlacement = placement;
            return localPlacement;
        }

        public static IfcUnitAssignment NewIfc4UnitAssignment(this IModel s, IfcUnitEnum unitType, IfcSIUnitName name, IfcSIPrefix? prefix = null)
        {
            var unitAssignment = s.Instances.New<IfcUnitAssignment>();
            unitAssignment.Units.Add(s.Instances.New<IfcSIUnit>(store =>
            {
                store.UnitType = unitType;
                store.Name = name;
                store.Prefix = prefix;
            }));
            return unitAssignment;
        }

        public static IfcRelContainedInSpatialStructure NewIfc4Contains(this IModel s, IfcSpatialElement spatialElement)
        {
            var relation = spatialElement.ContainsElements.FirstOrDefault();
            if (null == relation)
                relation = s.Instances.New<IfcRelContainedInSpatialStructure>();

            relation.RelatingStructure = spatialElement;
            return relation;
        }

        public static IfcRelAggregates NewIfc4Decomposes(this IModel s, IfcObjectDefinition host)
        {
            var relation = host.IsDecomposedBy.FirstOrDefault();
            if (null == relation)
                relation = s.Instances.New<IfcRelAggregates>();

            relation.RelatingObject = host;
            return relation;
        }

        public static IfcOwnerHistory NewIfc4OwnerHistoryEntry(this IModel s, string version,
            IfcPersonAndOrganization owningUser, IfcApplication owningApplication,
            IfcChangeActionEnum change = IfcChangeActionEnum.ADDED)
        {
            var newEntry = s.Instances.New<IfcOwnerHistory>();

            newEntry.ChangeAction = change;
            var dateTime = (IfcTimeStamp)DateTime.Now;
            newEntry.CreationDate = dateTime;
            newEntry.LastModifiedDate = dateTime;
            newEntry.OwningUser = owningUser;
            newEntry.OwningApplication = owningApplication;
            return newEntry;
        }

        public static T NewIfc4Product<T>(this IModel s, IfcObjectDefinition container = null, string name = null) where T : IInstantiableEntity, IIfcProduct
        {
            var product = s.Instances.New<T>(p => p.Name = name);

            if(container is IfcSpatialElement e)
            {
                if (product is IfcSpatialElement)
                    // Spatial-in-spatial substructure
                    s.NewIfc4Decomposes(e).RelatedObjects.Add(product as IfcProduct);
                else
                    // Product containment
                    s.NewIfc4Contains(e).RelatedElements.Add(product as IfcProduct);
            }
            else
            {
                s.NewIfc4Decomposes(container).RelatedObjects.Add(product as IfcProduct);
            }

            return product;
        }
        
        public static T NewIfc4ConnectedFaceSet<X,T>(this IModel s, 
            X dataHost, 
            Func<X, IEnumerable<XbimVector3D>> pointCoordinates,
            Func<X, IEnumerable<int[]>> indexes,
            bool scaleUp = false) where T : IfcConnectedFaceSet
        {
            T faceSet = s.Instances.New<T>();
            var points = s.NewIfcPoints<IfcCartesianPoint>(pointCoordinates(dataHost), scaleUp);

            foreach (var t in indexes(dataHost))
            {
                // Create face loop by given boundary points
                var polyLoop = s.Instances.New<IfcPolyLoop>();
                polyLoop.Polygon.AddRange(t.Select(k => points[k]));

                // Create bounds
                var bound = s.Instances.New<IfcFaceOuterBound>();
                bound.Bound = polyLoop;
                // Create face
                var face = s.Instances.New<IfcFace>();
                face.Bounds.Add(bound);
                // Add face to outer shell
                faceSet.CfsFaces.Add(face);
            }

            return faceSet;
        }

        public static IfcSurfaceStyle NewIfc4SurfaceStyleRendering(this IModel s, IfcColourRgb surface, IfcColourRgb specular = null, float transparency = 0)
        {
            var surfaceStyle = s.Instances.New<IfcSurfaceStyle>();
            surfaceStyle.Side = IfcSurfaceSide.BOTH;

            var renderingStyle = s.Instances.New<IfcSurfaceStyleRendering>(store =>
            {
                store.SurfaceColour = surface;
                store.DiffuseColour = surface;
                store.ReflectionColour = new IfcNormalisedRatioMeasure(1.0 - transparency);
                store.SpecularColour = specular;
                store.TransmissionColour = new IfcNormalisedRatioMeasure(transparency);

                store.ReflectanceMethod = IfcReflectanceMethodEnum.BLINN;
            });

            surfaceStyle.Styles.Add(renderingStyle);
            return surfaceStyle;
        }

        public static IfcShapeRepresentation NewIfc4RepresentationItem(this IModel s, IfcProduct product, IfcGeometricRepresentationItem geometryItem, IfcStyleAssignmentSelect style)
        {
            if (null != style)
            {
                s.Instances.New<IfcStyledItem>(i =>
                {
                    i.Item = geometryItem;
                    i.Styles.Add(style);
                });
            }

            var shapeRepresentation = s.NewIfc4ShapeRepresentation(product);
            shapeRepresentation.Items.Add(geometryItem);
            return shapeRepresentation;
        }

        public static IfcShapeRepresentation NewIfc4ShapeRepresentation(this IModel s, IfcProduct product, IfcGeometricRepresentationContext context = null)
        {
            var productDefinitionShape = product.Representation;
            if (null == productDefinitionShape)
                productDefinitionShape = s.Instances.New<IfcProductDefinitionShape>();

            product.Representation = productDefinitionShape;

            var shapeRepresentation = s.Instances.New<IfcShapeRepresentation>();
            productDefinitionShape.Representations.Add(shapeRepresentation);

            var project = s.Instances.OfType<IfcProject>().FirstOrDefault();
            shapeRepresentation.ContextOfItems = context ?? project.ModelContext;
            
            return shapeRepresentation;
        }

        public static IfcPropertySet NewIfc4PropertySet(this IModel s, string name, string description = null, IEnumerable<IfcProperty> properties = null)
        {
            var set = s.Instances.New<IfcPropertySet>(x =>
            {
                x.Name = name;
                x.Description = description;
            });

            foreach (var p in properties)
                set.HasProperties.Add(p);

            return set;
        }

        public static IfcRelDefinesByProperties NewIfc4PropertyRelation(this IModel s, IfcPropertySetDefinitionSelect set)
        {
            var rel = s.Instances.New<IfcRelDefinesByProperties>();
            rel.RelatingPropertyDefinition = set;
            return rel;
        }

        /// <summary>
        /// Creates a new local placement based on alignment reference
        /// </summary>
        /// <param name="s">The store</param>
        /// <param name="align">The aligment</param>
        /// <returns>A new local placement</returns>
        public static IfcLocalPlacement NewIfc4ObjectPlacementTo(this IModel s, IfcAlignReferenceAxis align)
        {
            var localPlacement = s.Instances.New<IfcLocalPlacement>();
            return ChangeIfc4ObjectPlacementTo(localPlacement, align, true);
        }

        /// <summary>
        /// Adapts alignment to given IFC4 placement aggregation.
        /// </summary>
        /// <param name="placement">The current placement</param>
        /// <param name="align">The alignment</param>
        /// <returns>A modified placement</returns>
        public static IfcLocalPlacement ChangeIfc4ObjectPlacementTo(this IfcLocalPlacement placement, IfcAlignReferenceAxis align, bool newAxisInstances)
        {
            var s = placement.Model;
            var refAxis = placement.RelativePlacement as IfcAxis2Placement3D;
            if (null == refAxis)
            {
                refAxis = s.Instances.New<IfcAxis2Placement3D>();
                if (null != placement.RelativePlacement)
                    s.Logger.LogWarning($"Leaving potentially orphan instance '{placement.RelativePlacement}'");
            }

            if (!newAxisInstances && refAxis.Axis is IfcDirection d1)
            {
                d1.DirectionRatios.Clear();
                d1.DirectionRatios.AddRange(align.ReferenceAxis.ToIfcReal4());
            }
            else
            {
                refAxis.Axis = s.NewIfcDirection<IfcDirection>(align.ReferenceAxis);
            }

            if (!newAxisInstances && refAxis.RefDirection is IfcDirection d2)
            {
                d2.DirectionRatios.Clear();
                d2.DirectionRatios.AddRange(align.AlignToAxis.ToIfcReal4());
            }
            else
            {
                refAxis.RefDirection = s.NewIfcDirection<IfcDirection>(align.AlignToAxis);
            }

            if (!newAxisInstances && refAxis.Location is IfcCartesianPoint p1)
            {
                p1.Coordinates.Clear();
                p1.Coordinates.AddRange(align.Offset.ToXbimVector3D().ToIfcLengthMeasure4(s.ModelFactors));
            }
            else
            {
                refAxis.Location = s.NewIfcPoint<IfcCartesianPoint>(align.Offset.ToXbimVector3D(), true);
            }

            placement.RelativePlacement = refAxis;
            return placement;
        }
    }
}