using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;

using Xbim.Common;
using Xbim.Common.Geometry;

using Bitub.Dto;
using Bitub.Dto.Scene;
using Bitub.Ifc.Transform;

using Google.Protobuf.WellKnownTypes;

using Microsoft.Extensions.Logging;

using Xbim.Ifc4.Interfaces;

namespace Bitub.Ifc.Export
{
    public class ExportContext<TSettings> where TSettings: ExportPreferences
    {
        internal readonly ILogger logger;

        public TSettings Current { get; private set; }

        public float Scale { get; private set; } = 1.0f;

        // Ifc context label vs. scene context
        internal readonly ConcurrentDictionary<int, SceneContextTransform> contextCache = new ConcurrentDictionary<int, SceneContextTransform>();
        // Ifc shape label vs. representation qualifier.
        internal readonly ConcurrentDictionary<int, Qualifier> representationQualifierCache = new ConcurrentDictionary<int, Qualifier>();

        internal ExportContext(ILoggerFactory loggerFactory)
        {
            logger = loggerFactory?.CreateLogger<ExportContext<TSettings>>();
        }

        internal void InitContextsAndScaleFromModel(IModel model, TSettings settings)
        {
            if (null == settings)
                throw new ArgumentNullException(nameof(settings));

            Current = settings;
            Scale = Current.UnitsPerMeter / (float)model.ModelFactors.OneMeter;

            Current.SelectedContext = Current.SelectedContext.Select(c => new SceneContext
            {
                Name = c.Name,
                // Given in DEG => use as it is
                FDeflection = model.ModelFactors.DeflectionAngle,
                // Given internally in model units => convert to meter
                FTolerance = model.ModelFactors.LengthToMetresConversionFactor * model.ModelFactors.DeflectionTolerance,
            }).ToArray();
        }

        internal SceneContext CreateContextByIfcRepresentationContext(IIfcRepresentationContext representationContext)
        {
            return new SceneContext
            {
                Name = representationContext.ContextIdentifier.ToString().ToQualifier(),
                // Given in DEG => use as it is
                FDeflection = representationContext.Model.ModelFactors.DeflectionAngle,
                // Given internally in model units => convert to meter
                FTolerance = representationContext.Model.ModelFactors.LengthToMetresConversionFactor * representationContext.Model.ModelFactors.DeflectionTolerance,
            };
        }

        public SceneContext[] ActiveContexts 
        { 
            get => contextCache.Values.Select(v => v.sceneContext).ToArray(); 
        }

        public int[] ActiveContextLabels 
        { 
            get => contextCache.Keys.ToArray(); 
        }

        public bool IsInContext(int contextLabel) => contextCache.ContainsKey(contextLabel);

        internal ComponentScene CreateEmptySceneModelFromProject(IIfcProject p)
        {
            return new ComponentScene()
            {
                Metadata = new MetaData
                {
                    Name = p?.Name,
                    Stamp = Timestamp.FromDateTime(DateTime.Now.ToUniversalTime())
                },                
                Id = p?.GlobalId.ToGlobalUniqueId().ToQualifier(),
                UnitsPerMeter = Current.UnitsPerMeter                
            };
        }

        private IDictionary<int, SceneContext> ContextsCreateFromModel(IModel model, IGeometryStoreReader gReader)
        {
            return gReader.ContextIds
                   .Select(label => model.Instances[label])
                   .OfType<IIfcRepresentationContext>()
                   .ToDictionary(c => c.EntityLabel, c => new SceneContext { Name = c.ContextIdentifier.ToString().ToQualifier() });
        }

        private IDictionary<int, SceneContext> ContextsCreateFrom(IModel model, IGeometryStoreReader gReader, string[] contextIdentifiers)
        {
            return gReader.ContextIds
                   .Select(label => model.Instances[label])
                   .OfType<IIfcRepresentationContext>()
                   .Select(c => (c.EntityLabel, contextIdentifiers.FirstOrDefault(id => string.Equals(id, c.ContextIdentifier, StringComparison.OrdinalIgnoreCase))))
                   .Where(t => t.Item2 != null)
                   .ToDictionary(t => t.EntityLabel, t => new SceneContext { Name = t.Item2.ToQualifier() });
        }

        internal IEnumerable<(int, SceneContext)> FilterActiveUserSceneContexts(IModel model, int[] contextLabels)
        {
            // Retrieve all context with geometry and match those to pregiven in settings
            return contextLabels
                   .Select(label => model.Instances[label])
                   .OfType<IIfcRepresentationContext>()
                   .Select(c => (c.EntityLabel, Current.SelectedContext.FirstOrDefault(sc => sc.Name.IsSuperQualifierOf(c.ContextIdentifier.ToString().ToQualifier(), StringComparison.OrdinalIgnoreCase))))
                   .Where(t => t.Item2 != null);
        }

        // Compute contexts and related transformation
        internal IEnumerable<SceneContextTransform> CreateSceneContextTransforms(IModelFactors factors, XbimContextRegionCollection contextRegions, IDictionary<int, SceneContext> contextTable)
        {
            foreach (var cr in contextRegions)
            {
                SceneContext sc;
                if (contextTable.TryGetValue(cr.ContextLabel, out sc))
                {
                    XbimVector3D offset = XbimVector3D.Zero;
                    XbimVector3D mean = XbimVector3D.Zero;
                    foreach (var r in cr)
                    {
                        mean += r.Centre.ToVector();
                        sc.Regions.Add(r.ToRegion(Scale));
                    }
                    mean *= 1.0 / cr.Count;

                    switch (Current.Positioning)
                    {
                        case ScenePositioningStrategy.UserCorrection:
                            // Center at user's center
                            offset = Current.UserModelCenter.ToXbimVector3DMeter(factors);
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
                            logger?.LogInformation($"No translation correction applied by settings to context '{cr.ContextLabel}'");
                            break;
                        default:
                            throw new NotImplementedException($"Missing implementation for '{Current.Positioning}'");
                    }

                    switch (Current.Transforming)
                    {
                        case SceneTransformationStrategy.Matrix:
                            // If Matrix or Global use rotation matrix representation
                            sc.Wcs = new XbimMatrix3D(offset).ToRotation(Scale);
                            break;
                        case SceneTransformationStrategy.Quaternion:
                            // Otherwise use Quaternion representation
                            sc.Wcs = new XbimMatrix3D(offset).ToQuaternion(Scale);
                            break;
                        default:
                            throw new NotImplementedException($"{Current.Transforming}");
                    }                        

                    // Set correction to negative offset shift (without scale since in model space units)
                    yield return new SceneContextTransform(cr.ContextLabel, sc, new XbimMatrix3D(offset * -1));
                }
                else
                {
                    logger?.LogWarning("Excluding context label '{0}'. Not mentioned by settings.", cr.ContextLabel);
                }
            }
        }

    }
}
