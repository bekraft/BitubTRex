using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Extensions.Logging;
using System.Text;
using System.Threading.Tasks;

using Bitub.Ifc.Scene;
using Bitub.Transfer;
using Bitub.Transfer.Scene;
using Bitub.Transfer.Spatial;

using Bitub.Ifc.Transform;

using Xbim.Common;
using Xbim.Common.Geometry;
using Xbim.ModelGeometry.Scene;

using Xbim.Ifc4.Interfaces;
using System.IO;
using Xbim.Common.XbimExtensions;
using Xbim.Common.Metadata;

namespace Bitub.Ifc.Scene
{
    /// <summary>
    /// Tesselation context implementing Xbim shape triangulation via Open Cascade.
    /// </summary>
    public sealed class XbimTesselationContext : IIfcTesselationContext
    {
        #region Internals
        private EventHandler<ICancelableProgressState> _progressEventDelegate;
        private EventHandler<ICancelableProgressState> _finishedEventDelegate;
        private EventHandler<ICancelableProgressState> _canceledEventDelegate;
        #endregion

        public event EventHandler<ICancelableProgressState> OnProgressChange
        {
            add {
                lock (this)
                    _progressEventDelegate += value;
            }
            remove {
                lock (this)
                    _progressEventDelegate -= value;
            }
        }

        public event EventHandler<ICancelableProgressState> OnCanceledProgress;
        public event EventHandler<ICancelableProgressState> OnProgressFinished;

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

        private IDictionary<int, SceneContext> ContextsCreateFromSettings(IGeometryStoreReader gReader, IfcSceneExportSummary s)
        {
            // Retrieve all context with geometry and match those to pregiven in settings
            return gReader.ContextIds
                   .Select(label => s.Model.Instances[label])
                   .OfType<IIfcRepresentationContext>()
                   .Select(c => (c.EntityLabel, s.AppliedSettings.UserRepresentationContext.FirstOrDefault(sc => sc.Name == c.ContextIdentifier)))
                   .Where(t => t.Item2 != null)
                   .ToDictionary(t => t.EntityLabel, t => t.Item2);
        }

        // Compute contexts and related transformation
        private void ComputeContextTransforms(IGeometryStoreReader gReader, IfcSceneExportSummary s, IDictionary<int, SceneContext> contextTable)
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
                            offset = s.AppliedSettings.UserModelCenter.ToXbimVector3D();
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

                    // Set correction to negative offset shift
                    s.Context[cr.ContextLabel] = new Tuple<SceneContext, XbimMatrix3D>(sc, new XbimMatrix3D(offset * -1));
                }
                else
                {
                    Logger?.LogWarning($"Excluding context label '{cr.ContextLabel}'. Not mentioned by settings.");
                }
            }
        }

        // Creates a new representation context
        private Representation CreateRepresentation(IfcSceneExportSummary s, XbimShapeInstance firstShape)
        {
            var context = s.ContextOf(firstShape.RepresentationContext);
            var rep = new Representation
            {
                Context = context.Name,
                BoundingBox = new BoundingBox
                {
                    ABox = firstShape.BoundingBox.ToABox(s.Scale, p => firstShape.Transformation.Transform(p))
                }
            };
            return rep;
        }

        // Creates a transform
        private Bitub.Transfer.Scene.Transform CreateTransform(IfcSceneExportSummary s, XbimShapeInstance shape)
        {
            // Context transformation (offset shift)
            var mt = s.TransformOf(shape.RepresentationContext);
            switch (s.AppliedSettings.Transforming)
            {
                case SceneTransformationStrategy.Matrix:
                    return (mt * shape.Transformation).ToRotation(s.Scale);
                case SceneTransformationStrategy.Quaternion:
                    return (mt * shape.Transformation).ToQuaternion(s.Scale);
                default:
                    throw new NotImplementedException($"Missing implementation for '{s.AppliedSettings.Transforming}'");
            }
        }

        // Append vertices and return shift
        private void AppendVertices(Representation r, IfcSceneExportSummary summary, IEnumerable<XbimPoint3D> points)
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
        /// <param name="progressState">Any given state or <c>null</c></param>
        /// <returns>The given udpated state or a new state</returns>
        public CancelableProgressStateToken ReadGeometryStore(IModel model, CancelableProgressStateToken progressState = null)
        {
            if (null == progressState)
                progressState = new CancelableProgressStateToken(true, 100);

            // Use Xbim Model Context for geometry creation            
            if (model.GeometryStore.IsEmpty)
            {
                ReportProgressDelegate progressDelegate = (percent, userState) =>
                {
                    _progressEventDelegate?.Invoke(this, progressState.Update(percent, userState.ToString()));
                    Logger?.LogDebug("... running internal tesselation at ${0}% (${1})", percent, userState ?? "no state info");
                };

                var context = new Xbim3DModelContext(model);
                context.CreateContext(progressDelegate, false);
            }

            return progressState;
        }

        /// <summary>
        /// Runs tessselation with Xbim scene context
        /// </summary>
        /// <param name="model"></param>
        /// <param name="summary"></param>
        /// <param name="progressState"></param>
        /// <returns></returns>
        public IEnumerable<IfcProductSceneRepresentation> Tesselate(IModel model, IfcSceneExportSummary summary, CancelableProgressStateToken progressState = null)
        {
            progressState = ReadGeometryStore(model, new CancelableProgressStateToken(true, 100));

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

                foreach (var geometry in gReader.ShapeGeometries)
                {
                    currentCount++;

                    _progressEventDelegate?.Invoke(this, progressState.Update((int)Math.Round(100.0 * currentCount / totalCount), "Transferring mesh"));

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
                                    Logger?.LogWarning($"Shape #{shape.InstanceLabel} of product #{product.EntityLabel} out of context scope. Skipped.");
                                    continue;
                                }
                                var representation = pkg.Representations.FirstOrDefault(r => r.Context.Equals(ctx.Name));
                                if (null == representation)
                                {
                                    representation = CreateRepresentation(summary, shape);
                                    pkg.Representations.Add(representation);
                                }
                                else
                                {
                                    // Union bounding boxes
                                    representation.BoundingBox.ABox = representation.BoundingBox.ABox.UnionWith(
                                        shape.BoundingBox.ToABox(summary.Scale, p => shape.Transformation.Transform(p))
                                    );
                                }

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
                        Logger?.LogWarning($"Product #{e.Key} misses {e.Value.CountOpenInstances} shape(s).");
                        yield return e.Value.ToSceneRepresentation(e.Key);
                    }
                }
            }
        }

        public void Cancel()
        {
            throw new NotImplementedException();
        }
    }
}
