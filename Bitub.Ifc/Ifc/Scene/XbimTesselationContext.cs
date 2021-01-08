using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Extensions.Logging;
using System.Text;
using System.Threading.Tasks;

using Bitub.Ifc.Export;
using Bitub.Dto;
using Bitub.Dto.Scene;
using Bitub.Dto.Spatial;

using Bitub.Ifc.Transform;

using Xbim.Common;
using Xbim.Common.Geometry;
using Xbim.ModelGeometry.Scene;

using Xbim.Ifc4.Interfaces;
using System.IO;
using Xbim.Common.XbimExtensions;
using Xbim.Common.Metadata;

namespace Bitub.Ifc.Export
{
    /// <summary>
    /// Tesselation context implementing Xbim shape triangulation via Open Cascade.
    /// </summary>
    public sealed class XbimTesselationContext : IIfcTesselationContext
    {
        /// <summary>
        /// Internal tesselation package aggregator.
        /// </summary>
        private class TesselationPackage
        {
            internal readonly List<Representation> Representations;
            
            private List<string> _requiredShapesInstances;
            private List<int> _doneShapeGeometryLabels;

            internal TesselationPackage(IEnumerable<XbimShapeInstance> required)
            {
                Representations = new List<Representation>();
                _requiredShapesInstances = required.Select(i => LabelShapeInstance(i)).ToList();                
            }

            internal string LabelShapeInstance(XbimShapeInstance s) => $"{s.ShapeGeometryLabel}/{s.InstanceLabel}";

            internal bool IsShapeGeometryDone(XbimShapeGeometry g) => null != _doneShapeGeometryLabels && _doneShapeGeometryLabels.Contains(g.ShapeLabel);

            internal bool RemoveDone(XbimShapeInstance shape)
            {
                bool removed = _requiredShapesInstances.Remove(LabelShapeInstance(shape));
                if(CountOpenInstances > 0)
                {   // Only if we are not done yet
                    if (null == _doneShapeGeometryLabels)
                        _doneShapeGeometryLabels = new List<int>();
                    _doneShapeGeometryLabels.Add(shape.ShapeGeometryLabel);
                }
                return removed;
            }

            internal int CountOpenInstances => _requiredShapesInstances.Count;

            internal bool IsDone => _requiredShapesInstances.Count == 0;

            internal IfcProductSceneRepresentation ToSceneRepresentation(int productLabel)
            {
                return new IfcProductSceneRepresentation(productLabel, Representations);
            }
        }

        private readonly ILogger Logger;

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
            Logger = loggerFactory?.CreateLogger<XbimTesselationContext>();
        }

        private IDictionary<int, SceneContext> ContextsCreateFromModel(IModel model, IGeometryStoreReader gReader)
        {
            return gReader.ContextIds
                   .Select(label => model.Instances[label])
                   .OfType<IIfcRepresentationContext>()
                   .ToDictionary(c => c.EntityLabel, c => new SceneContext { Name = c.ContextIdentifier });
        }

        private IDictionary<int, SceneContext> ContextsCreateFrom(IModel model, IGeometryStoreReader gReader, string[] contextIdentifiers)
        {
            return gReader.ContextIds
                   .Select(label => model.Instances[label])
                   .OfType<IIfcRepresentationContext>()
                   .Select(c => (c.EntityLabel, contextIdentifiers.FirstOrDefault(id => id == c.ContextIdentifier)))
                   .Where(t => t.Item2 != null)
                   .ToDictionary(t => t.EntityLabel, t => new SceneContext { Name = t.Item2 } );
        }

        private IDictionary<int, SceneContext> SceneContextsOf(IEnumerable<IIfcRepresentationContext> modelContexts, string[] identifiers, bool ignoreCase = false)
        {
            var comparisionMethod = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            return modelContexts
                    .Select(c => (c.EntityLabel, identifiers.FirstOrDefault(id => string.Equals(id, c.ContextIdentifier, comparisionMethod))))
                    .Where(t => t.Item2 != null)
                    .ToDictionary(t => t.EntityLabel, t => new SceneContext { Name = t.Item2 });
        }

        private IDictionary<int, SceneContext> ContextsCreateFromSettings(IGeometryStoreReader gReader, IfcSceneExportResult s)
        {
            // Retrieve all context with geometry and match those to pregiven in settings
            return gReader.ContextIds
                   .Select(label => s.Model.Instances[label])
                   .OfType<IIfcRepresentationContext>()
                   .Select(c => (c.EntityLabel, s.AppliedSettings.UserRepresentationContext.FirstOrDefault(sc => StringComparer.OrdinalIgnoreCase.Equals(sc.Name, c.ContextIdentifier))))
                   .Where(t => t.Item2 != null)
                   .ToDictionary(t => t.EntityLabel, t => t.Item2);
        }

        // Compute contexts and related transformation
        private void ComputeContextTransforms(IGeometryStoreReader gReader, IfcSceneExportResult s, IDictionary<int, SceneContext> contextTable)
        {
            foreach (var cr in gReader.ContextRegions)
            {
                SceneContext sc;
                if (contextTable.TryGetValue(cr.ContextLabel, out sc))
                {
                    XbimVector3D offset = XbimVector3D.Zero;
                    XbimVector3D mean = XbimVector3D.Zero;
                    foreach (var r in cr)
                    {
                        mean += r.Centre.ToVector();
                        sc.Regions.Add(r.ToRegion(s.Scale));
                    }
                    mean *= 1.0 / cr.Count;

                    switch (s.AppliedSettings.Positioning)
                    {
                        case ScenePositioningStrategy.UserCorrection:
                            // Center at user's center
                            offset = s.AppliedSettings.UserModelCenter.ToXbimVector3DMeter(s.Model.ModelFactors);
                            break;
                        case ScenePositioningStrategy.MostPopulatedRegionCorrection:
                            // Center at most populated
                            offset = cr.MostPopulated().Centre.ToVector();
                            break;
                        case ScenePositioningStrategy.MostExtendedRegionCorrection:
                            // Center at largest
                            offset = cr.Largest().Centre.ToVector();
                            break;
                        case ScenePositioningStrategy.MeanTranslationCorrection:
                            // Use mean correction
                            offset = mean;
                            break;
                        case ScenePositioningStrategy.SignificantPopulationCorrection:
                            var population = cr.Sum(r => r.Population);
                            XbimRegion rs = null;
                            double max = double.NegativeInfinity;
                            foreach (var r in cr)
                            {
                                // Compute weighted extent by relative population
                                double factor = r.Size.Length * r.Population / population;
                                if (max < factor)
                                {
                                    rs = r;
                                    max = factor;
                                }
                            }
                            offset = rs.Centre.ToVector();
                            break;
                        case ScenePositioningStrategy.NoCorrection:
                            // No correction
                            Logger?.LogInformation($"No translation correction applied by settings to context '{cr.ContextLabel}'");
                            break;
                        default:
                            throw new NotImplementedException($"Missing implementation for '{s.AppliedSettings.Positioning}'");
                    }

                    if (s.AppliedSettings.Transforming == SceneTransformationStrategy.Matrix)
                        // If Matrix or Global use rotation matrix representation
                        sc.Wcs = new XbimMatrix3D(offset).ToRotation(s.Scale);
                    else
                        // Otherwise use Quaternion representation
                        sc.Wcs = new XbimMatrix3D(offset).ToQuaternion(s.Scale);

                    // Set correction to negative offset shift (without scale since in model space units)
                    s.SetRepresentationContext(cr.ContextLabel, sc, new XbimMatrix3D(offset * -1));
                }
                else
                {
                    Logger?.LogWarning("Excluding context label '{0}'. Not mentioned by settings.", cr.ContextLabel);
                }
            }
        }

        // Creates a new representation context
        private Representation GetOrCreateRepresentation(IfcSceneExportResult s, XbimShapeInstance shape, TesselationPackage pkg)
        {            
            var (context, contextWcs) = s.RepresentationContext(shape.RepresentationContext);
            var representation = pkg.Representations.FirstOrDefault(r => r.Context.Equals(context.Name));
            // left expansion & concatenation in xbim !
            var aabb = shape.BoundingBox.ToABox(s.Scale, p => (shape.Transformation * contextWcs).Transform(p));

            if (null == representation)
            {   // Create new representation
                representation = new Representation
                {
                    Context = context.Name,
                    BoundingBox = new BoundingBox { ABox = aabb }
                };
                pkg.Representations.Add(representation);
            }
            else
            {   // Union bounding boxes
                representation.BoundingBox.ABox = representation.BoundingBox.ABox.UnionWith(aabb);
            }
            return representation;
        }

        // Creates a transform
        private Dto.Scene.Transform CreateTransform(IfcSceneExportResult s, XbimShapeInstance shape)
        {
            // Context transformation (relative offset shift => make final transform relative to context shift)
            var contextWcs = s.TransformOf(shape.RepresentationContext) ?? XbimMatrix3D.Identity;
            switch (s.AppliedSettings.Transforming)
            {
                case SceneTransformationStrategy.Matrix:
                    return (shape.Transformation * contextWcs).ToRotation(s.Scale);
                case SceneTransformationStrategy.Quaternion:
                    return (shape.Transformation * contextWcs).ToQuaternion(s.Scale);
                default:
                    throw new NotImplementedException($"Missing implementation for '{s.AppliedSettings.Transforming}'");
            }
        }

        // Append vertices and return shift
        private void AppendVertices(Representation r, IfcSceneExportResult summary, IEnumerable<XbimPoint3D> points)
        {
            PtArray ptArray = new PtArray();
            foreach (var p in points)
                // Append to vertices and apply scale
                p.AppendTo(ptArray.Xyz, summary.Scale);

            r.Points.Add(ptArray);
        }

        /// <summary>
        /// Reads the geometry from model if empty.
        /// </summary>
        /// <param name="model">The model</param>
        /// <param name="progressing">The progress emitter</param>
        /// <param name="forceUpdate">Whether to force an update of geometry store anyway</param>
        /// <returns>The given udpated state or a new state</returns>
        public IEnumerable<IIfcRepresentationContext> ReadGeometryStore(IModel model, CancelableProgressing progressing, bool forceUpdate = false)
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
                
                var context = new Xbim3DModelContext(model, "model", null, Logger);                
                context.CreateContext(progressDelegate, false);
                return context.Contexts;
            }
            else
            {
                return model.GeometryStore.BeginRead().ContextIds
                        .Select(label => model.Instances[label])
                        .Cast<IIfcRepresentationContext>();
            }
        }

        /// <summary>
        /// Runs tessselation with Xbim scene context
        /// </summary>
        /// <param name="model">The model to be exported</param>
        /// <param name="summary">The scene export summary</param>
        /// <param name="monitor">The progress emitter instance</param>
        /// <returns>An enumerable of tesselated product representations</returns>
        public IEnumerable<IfcProductSceneRepresentation> Tesselate(IModel model, IfcSceneExportResult summary, CancelableProgressing monitor)
        {
            ReadGeometryStore(model, monitor);

            short[] excludeTypeId = ExcludeExpressType.Select(t => model.Metadata.ExpressTypeId(t.ExpressName)).ToArray();
            Array.Sort(excludeTypeId);

            // Start reading the geometry store built before
            using (var gReader = model.GeometryStore.BeginRead())
            {
                int totalCount = gReader.ShapeGeometries.Count();
                int currentCount = 0;
                // Product label vs. Component and candidate shape labels
                var packageCache = new SortedDictionary<int, TesselationPackage>();
                // Compute contexts
                ComputeContextTransforms(gReader, summary, ContextsCreateFromSettings(gReader, summary));

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
                        .Where(i => 0 > Array.BinarySearch(excludeTypeId, i.IfcTypeId) 
                        && i.RepresentationType == XbimGeometryRepresentationType.OpeningsAndAdditionsIncluded);

                    if (!shapes.Any())
                        // No shape instances
                        continue;

                    using (var ms = new MemoryStream(((IXbimShapeGeometryData)geometry).ShapeData))
                    {
                        using (var br = new BinaryReader(ms))
                        {
                            XbimShapeTriangulation tr = br.ReadShapeTriangulation();                            
                            foreach (XbimShapeInstance shape in shapes)
                            {
                                if (monitor?.State.IsAboutCancelling ?? false)
                                {
                                    monitor.State.MarkCanceled();
                                    break;
                                }

                                var product = model.Instances[shape.IfcProductLabel] as IIfcProduct;

                                // Try first to find the referenced component
                                TesselationPackage pkg;
                                if (!packageCache.TryGetValue(shape.IfcProductLabel, out pkg))
                                {
                                    // New component ToDo shape tuple built from component and todo ShapeGeometryLabel
                                    pkg = new TesselationPackage(gReader.ShapeInstancesOfEntity(product));
                                    packageCache[shape.IfcProductLabel] = pkg;
                                }

                                var ctx = summary.ContextOf(shape.RepresentationContext);
                                if (null == ctx)
                                {
                                    Logger?.LogWarning($"Shape of representation #{shape.RepresentationContext} of product #{product.EntityLabel} out of context scope. Skipped.");
                                    continue;
                                }

                                // Check for representation
                                var representation = GetOrCreateRepresentation(summary, shape, pkg);

                                if (!pkg.IsShapeGeometryDone(geometry))
                                    AppendVertices(representation, summary, tr.Vertices);

                                // TODO Use "bias" definition to adjust biased local offsets
                                var body = new FaceBody
                                {
                                    Material = new RefId { Nid = shape.StyleLabel > 0 ? shape.StyleLabel : shape.IfcTypeId * -1 },
                                    Transform = CreateTransform(summary, shape),
                                    PtSet = (uint)representation.Points.Count - 1,
                                };

                                foreach (var face in tr.Faces)
                                {
                                    if (face.Indices.Count % 3 != 0)
                                        throw new NotSupportedException("Expecting triangular meshes only");

                                    // Translate Xbim face definition
                                    var bodyFace = new Face
                                    {
                                        IsPlanar = face.IsPlanar,                                        
                                        Mesh = new Mesh
                                        {
                                            Type = FacetType.TriMesh,
                                            Orient = Orientation.Ccw
                                        }
                                    };

                                    switch (face.NormalCount)
                                    {
                                        case 0:
                                            // No normals at all
                                            break;
                                        case 1:
                                            // Single normal
                                            bodyFace.IsPlanar = true;
                                            face.Normals[0].Normal.AppendTo(bodyFace.Mesh.Normal);
                                            break;
                                        default:
                                            // No planar face
                                            if (face.NormalCount != face.Indices.Count)
                                                throw new NotSupportedException($"Incorrect count of normals per face mesh (expecting {face.Indices.Count}, have {face.NormalCount}");

                                            foreach (var n in face.Normals.Select(n => n.Normal))
                                                n.AppendTo(bodyFace.Mesh.Normal);

                                            break;
                                    }

                                    bodyFace.Mesh.Vertex.AddRange(face.Indices.Select(i => (uint)i));
                                    body.Faces.Add(bodyFace);
                                }

                                // Add body to known component
                                representation.Bodies.Add(body);
                                // Remove geometry label from todo list
                                pkg.RemoveDone(shape);

                                // If no shape instances left
                                if (pkg.IsDone)
                                {   
                                    yield return pkg.ToSceneRepresentation(shape.IfcProductLabel);
                                    packageCache.Remove(shape.IfcProductLabel);
                                }
                            }
                        }
                    }
                }

                // Return most recent
                if (packageCache.Count > 0)
                {
                    Logger?.LogWarning($"Detected {packageCache.Count} unfinished geometry entries. Missing shapes.");
                    foreach (var e in packageCache)
                    {
                        // Announce missing components even if unfinished due to some reason
                        Logger?.LogWarning($"IfcProduct #{e.Key} misses {e.Value.CountOpenInstances} shape(s).");
                        yield return e.Value.ToSceneRepresentation(e.Key);
                    }
                }
            }

            monitor?.NotifyOnProgressChange("Done tesselation.");
        }
    }
}
