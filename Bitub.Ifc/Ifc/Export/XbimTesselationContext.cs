using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Extensions.Logging;
using System.Text;
using System.Threading.Tasks;

using Bitub.Dto;
using Bitub.Dto.Scene;
using Bitub.Dto.Spatial;
using Bitub.Ifc.Export;

using Bitub.Ifc.Transform;

using Xbim.Common;
using Xbim.Common.Geometry;
using Xbim.ModelGeometry.Scene;

using Xbim.Ifc4.Interfaces;

using Xbim.Common.XbimExtensions;
using Xbim.Common.Metadata;

namespace Bitub.Ifc.Export
{
    /// <summary>
    /// Tesselation context implementing Xbim shape triangulation via Open Cascade.
    /// </summary>
    public sealed class XbimTesselationContext : ITesselationContext<ExportPreferences>
    {
        #region Internals
        private readonly ILogger logger;

        // Aggregates component and remaining shape labels.
        private class ComponentShape
        {
            // Sorted list
            readonly List<int> instanceLabels = new List<int>();
            readonly List<Shape> shapeList = new List<Shape>();

            internal ComponentShape(IEnumerable<XbimShapeInstance> productShapeInstances)
            {
                instanceLabels = productShapeInstances.Select(i => i.InstanceLabel).OrderBy(i => i).ToList();
            }

            internal bool Add(XbimShapeInstance productShapeInstance, Shape productShape)
            {
                var idx = instanceLabels.BinarySearch(productShapeInstance.InstanceLabel);
                if (0 > idx)
                    return false;
                else
                    instanceLabels.RemoveAt(idx);

                shapeList.Add(productShape);
                return true;
            }

            internal IEnumerable<Shape> Shapes
            {
                get => shapeList.ToArray();
            }

            internal bool IsComplete
            {
                get => instanceLabels.Count == 0;
            }
        }
        #endregion

        /// <summary>
        /// A list of EXPRESS IFC types to be excluded while exporting tesselations. Default is empty.
        /// </summary>
        public List<ExpressType> ExcludeExpressType { get; set; } = new List<ExpressType>();

        /// <summary>
        /// A new instance of Xbim tesselation context
        /// </summary>
        /// <param name="loggerFactory">Optional logger factory</param>
        public XbimTesselationContext(ILoggerFactory loggerFactory = null)
        {
            logger = loggerFactory?.CreateLogger<XbimTesselationContext>();
        }

        /// <summary>
        /// Reads the geometry from model if empty.
        /// </summary>
        /// <param name="model">The model</param>
        /// <param name="progressing">The progress emitter</param>
        /// <param name="forceUpdate">Whether to force an update of geometry store anyway</param>
        /// <returns>The given udpated state or a new state</returns>
        public IGeometryStore ReadGeometryStore(IModel model, CancelableProgressing progressing, bool forceUpdate = false)
        {
            // Use Xbim Model Context for geometry creation            
            if (forceUpdate || (model.GeometryStore?.IsEmpty ?? false))
            {
                progressing?.NotifyProgressEstimateUpdate(100);
                ReportProgressDelegate progressDelegate = (percent, userState) =>
                {
                    progressing?.State.UpdateDone(percent, userState.ToString());
                    progressing?.NotifyOnProgressChange();
                };
                
                var context = new Xbim3DModelContext(model, "model", null, logger);                
                context.CreateContext(progressDelegate, false);                
            }
            return model.GeometryStore;
        }

        /// <summary>
        /// Runs tessselation with Xbim scene context
        /// </summary>
        /// <param name="model">The model to be exported</param>
        /// <param name="summary">The scene export summary</param>
        /// <param name="monitor">The progress emitter instance</param>
        /// <returns>An enumerable of tesselated product representations</returns>
        public IEnumerable<TesselationMessage> Tesselate(IModel model, ExportContext<ExportPreferences> ec, CancelableProgressing monitor)
        {
            return Tesselate(model, ec, monitor, XbimGeometryRepresentationType.OpeningsAndAdditionsIncluded);
        }

        public IEnumerable<TesselationMessage> Tesselate(IModel model, 
            ExportContext<ExportPreferences> ec, CancelableProgressing monitor, params XbimGeometryRepresentationType[] geometryTypes)
        {
            var geometryStore = ReadGeometryStore(model, monitor);
            Array.Sort(geometryTypes);

            short[] excludeTypeId = ExcludeExpressType.Select(t => model.Metadata.ExpressTypeId(t.ExpressName)).ToArray();
            Array.Sort(excludeTypeId);

            // Start reading the geometry store built before
            using (var gReader = geometryStore.BeginRead())
            {
                int totalCount = gReader.ShapeGeometries.Count();
                int currentCount = 0;
                // Product label vs. Component and candidate shape labels
                var componentCache = new SortedDictionary<int, ComponentShape>();

                // Compute contexts and push them to cache
                var activeContexts = ec.FilterActiveUserSceneContexts(model, gReader.ContextIds.ToArray()).ToDictionary(g => g.Item1, g => g.Item2);
                foreach (var contextTransform in ec.CreateSceneContextTransforms(model.ModelFactors, gReader.ContextRegions, activeContexts))
                {
                    ec.contextCache.TryAdd(contextTransform.contextLabel, contextTransform);
                    yield return new TesselationMessage(contextTransform);
                }

                monitor?.NotifyProgressEstimateUpdate(totalCount);

                foreach (var geometry in gReader.ShapeGeometries)
                {
                    if (monitor?.State.IsAboutCancelling ?? false)
                    {
                        monitor.State.MarkCanceled();
                        break;
                    }

                    currentCount++;
                    
                    monitor?.State.UpdateDone(currentCount, "Running tesselation...");
                    monitor?.NotifyOnProgressChange();

                    if (geometry.ShapeData.Length <= 0)
                        // No geometry
                        continue;

                    var shapes = gReader.ShapeInstancesOfGeometry(geometry.ShapeLabel)
                        // Skip types defined by exclusion and those which aren't defined by geometry types
                        .Where(i => 0 > Array.BinarySearch(excludeTypeId, i.IfcTypeId) && 0 <= Array.BinarySearch(geometryTypes, i.RepresentationType));

                    if (!shapes.Any())
                        // No shape instances
                        continue;

                    using (var ms = new MemoryStream(((IXbimShapeGeometryData)geometry).ShapeData))
                    {
                        using (var br = new BinaryReader(ms))
                        {
                            XbimShapeTriangulation tr = br.ReadShapeTriangulation();
                            
                            // Create representation
                            var shapeBody = new ShapeBody()
                            {
                                Id = new RefId { Nid = geometry.ShapeLabel },
                            };

                            if (ec.Current.BodyExportType.HasFlag(SceneBodyExportType.FaceBody))
                            {
                                shapeBody.Bodies.Add(new Body { FaceBody = CreateFaceBody(ec, tr, new RefId { Nid = shapeBody.Points.Count }) });
                            }
                            if (ec.Current.BodyExportType.HasFlag(SceneBodyExportType.MeshBody))
                            {
                                shapeBody.Bodies.Add(new Body { MeshBody = CreateMeshBody(ec, tr, new RefId { Nid = shapeBody.Points.Count }) });
                            }
                            if (ec.Current.BodyExportType.HasFlag(SceneBodyExportType.WireBody))
                            {
                                // TODO Support wires, find curves by mesh investigation, see tesselation validation support
                                throw new NotImplementedException("Wire body currently not supported/implemented.");
                            }

                            var ptArray = CreatePtArray(ec, tr.Vertices);
                            ptArray.Id = new RefId { Nid = shapeBody.Points.Count };
                            shapeBody.Points.Add(ptArray);
                            yield return new TesselationMessage(new ShapeRepresentation(geometry.IfcShapeLabel, shapeBody));
                            
                            // Create shapes
                            foreach (XbimShapeInstance shape in shapes)
                            {
                                if (monitor?.State.IsAboutCancelling ?? false)
                                {
                                    monitor.State.MarkCanceled();
                                    break;
                                }

                                var product = model.Instances[shape.IfcProductLabel] as IIfcProduct;

                                // Try first to find the referenced component
                                ComponentShape cShape;
                                if (!componentCache.TryGetValue(shape.IfcProductLabel, out cShape))
                                {
                                    // New component ToDo shape tuple built from component and todo ShapeGeometryLabel
                                    cShape = new ComponentShape(gReader.ShapeInstancesOfEntity(product)
                                        .Where(i => 0 > Array.BinarySearch(excludeTypeId, i.IfcTypeId) && 0 <= Array.BinarySearch(geometryTypes, i.RepresentationType)));
                                    componentCache[shape.IfcProductLabel] = cShape;
                                }

                                cShape.Add(shape, CreateShape(ec, shape));

                                // If no shape instances left
                                if (cShape.IsComplete)
                                {   
                                    yield return new TesselationMessage(new ProductShape(shape.IfcProductLabel, cShape.Shapes));
                                    componentCache.Remove(shape.IfcProductLabel);
                                }
                            }
                        }
                    }
                }

                // Return most recent
                if (componentCache.Count > 0)
                {
                    logger?.LogWarning($"Detected {componentCache.Count} unfinished geometry entries. Missing shapes.");
                    foreach (var e in componentCache)
                    {
                        // Announce missing components even if unfinished due to some reason
                        logger?.LogWarning($"IfcProduct '#{e.Key}' misses shape(s).");
                        yield return new TesselationMessage(new ProductShape(e.Key, e.Value.Shapes));
                    }
                }
            }

            monitor?.NotifyOnProgressChange("Done tesselation.");
        }

        #region Mesh creation

        // Append vertices and return shift
        private PtArray CreatePtArray(ExportContext<ExportPreferences> ec, IEnumerable<XbimPoint3D> points)
        {
            PtArray ptArray = new PtArray();
            foreach (var p in points)
                // Append to vertices and apply scale
                p.AppendTo(ptArray.Xyz, ec.Scale);

            return ptArray;
        }

        // Appends the triangulated face to the given mesh
        private Mesh AppendFaceToMesh(Mesh mesh, XbimFaceTriangulation face)
        {
            if (face.Indices.Count % 3 != 0)
                throw new NotSupportedException("Expecting triangular meshes only");

            switch (face.NormalCount)
            {
                case 0:
                    // No normals at all
                    break;
                case 1:
                    // Single normal
                    face.Normals[0].Normal.AppendTo(mesh.Normal);
                    break;
                default:
                    // No planar face
                    if (face.NormalCount != face.Indices.Count)
                        throw new NotSupportedException($"Incorrect count of normals per face mesh (expecting {face.Indices.Count}, have {face.NormalCount}");

                    face.Normals.Select(n => n.Normal).ForEach(n => n.AppendTo(mesh.Normal));
                    break;
            }

            mesh.Vertex.AddRange(face.Indices.Select(i => (uint)i));
            return mesh;
        }

        // Creates a faceted body having explicit faces
        private FaceBody CreateFaceBody(ExportContext<ExportPreferences> ec, XbimShapeTriangulation tr, RefId pts)
        {
            var faceBody = new FaceBody { Pts = pts };

            foreach (var face in tr.Faces)
            {
                // Translate Xbim face definition                
                var bodyFace = new Face { IsPlanar = face.IsPlanar };
                bodyFace.Mesh = AppendFaceToMesh(new Mesh { Type = FacetType.TriMesh, Orient = Orientation.Ccw }, face);

                if (1 == face.NormalCount)
                    bodyFace.IsPlanar = true;

                faceBody.Faces.Add(bodyFace);
            }
            return faceBody;
        }

        private MeshBody CreateMeshBody(ExportContext<ExportPreferences> ec, XbimShapeTriangulation tr, RefId pts)
        {
            var meshBody = new MeshBody()
            {
                Pts = pts,                 
                IsConvex = false,
                Tess = new Mesh { Orient = Orientation.Ccw, Type = FacetType.TriMesh }
            };

            tr.Faces.ForEach(f => AppendFaceToMesh(meshBody.Tess, f));

            return meshBody;
        }

        private Dto.Scene.Transform CreateTransform(SceneTransformationStrategy transformationStrategy, float scale, XbimMatrix3D matrix3D)
        {
            switch (transformationStrategy)
            {
                case SceneTransformationStrategy.Matrix:
                    return matrix3D.ToRotation(scale);                    
                case SceneTransformationStrategy.Quaternion:
                    return matrix3D.ToQuaternion(scale);                    
                default:
                    throw new NotImplementedException($"Missing implementation for '{transformationStrategy}'");
            }
        }

        private Shape CreateShape(ExportContext<ExportPreferences> ec, XbimShapeInstance shapeInstance)
        {
            // Context transformation (relative offset shift => make final transform relative to context shift)
            SceneContextTransform ctxTransform;
            XbimMatrix3D shapeTransform;
            if (ec.contextCache.TryGetValue(shapeInstance.RepresentationContext, out ctxTransform))
            {
                shapeTransform = shapeInstance.Transformation * ctxTransform.transform;
            }
            else
            {                
                logger.LogWarning($"Processed shape with geometry label '#{shapeInstance.ShapeGeometryLabel}' of unknown context label '#{shapeInstance.RepresentationContext}'");
                return null;
            }

            return new Shape()
            {
                // Encode typeId has material ID with negative magnitude if style label isn't defined
                Material = new RefId { Nid = shapeInstance.StyleLabel > 0 ? shapeInstance.StyleLabel : shapeInstance.IfcTypeId * -1 },
                Context = ctxTransform.sceneContext.Name,
                Transform = CreateTransform(ec.Current.Transforming, ec.Scale, shapeTransform),
                ShapeBody = new RefId { Nid = shapeInstance.ShapeGeometryLabel },
                BoundingBox = new BoundingBox 
                { 
                    ABox = shapeInstance.BoundingBox.ToABox(ec.Scale, p => shapeTransform.Transform(p))
                }                
            };
        }

        #endregion
    }
}
