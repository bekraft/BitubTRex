using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Extensions.Logging;

using Xbim.Common;

using Xbim.Ifc;
using Xbim.Ifc4.Interfaces;

using Bitub.Dto;
using Bitub.Dto.Scene;

using Component = Bitub.Dto.Scene.Component;
using System.Threading.Tasks;

using Bitub.Ifc.Concept;
using Bitub.Dto.Concept;

namespace Bitub.Ifc.Export
{
    /// <summary>
    /// Transfer scene model data exporter. Internally uses an abstract tesselation provider. In case of Xbim tesselation use
    /// <code>
    /// var exporter = new IfcSceneExporter(new XbimTesselationContext(loggerFactory), loggerFactory);
    /// var result = await exporter.Run(myModel);
    /// </code>
    /// </summary>
    public class IfcSceneExporter
    {
        #region Internals
        private readonly ILogger Logger;
        private readonly IIfcTesselationContext TesselatorInstance;
        #endregion

        /// <summary>
        /// Initial experter settings.
        /// </summary>
        public IfcExportSettings Settings { get; set; } = new IfcExportSettings();

        /// <summary>
        /// Default color settings.
        /// </summary>
        public XbimColourMap DefaultProductColorMap { get; set; } = new XbimColourMap(StandardColourMaps.IfcProductTypeMap);

        /// <summary>
        /// Creates a new instance of a scene exporter.
        /// </summary>
        /// <param name="loggerFactory"></param>
        public IfcSceneExporter(IIfcTesselationContext tesselatorInstance, ILoggerFactory loggerFactory = null)
        {
            Logger = loggerFactory?.CreateLogger<IfcSceneExporter>();
            TesselatorInstance = tesselatorInstance;            
        }

        /// <summary>
        /// Runs the model transformation.
        /// </summary>
        /// <param name="model">The IFC model</param>
        /// <returns>A scene</returns>
        public Task<IfcSceneExportResult> RunExport(IModel model, CancelableProgressing monitor)
        {
            return Task.Run(() =>
            {
                try
                {
                    return BuildScene(model, new IfcExportSettings(Settings), monitor);
                }
                catch (Exception e)
                {
                    monitor?.State.MarkBroken();
                    Logger.LogError("{0}: {1} [{2}]", e.GetType().Name, e.Message, e.StackTrace);
                    return new IfcSceneExportResult(e, model, Settings);
                }
            });
        }

        // Runs the scene model export
        private IfcSceneExportResult BuildScene(IModel model, IfcExportSettings settings, CancelableProgressing progressing)
        {
            // Generate new summary
            var result = new IfcSceneExportResult(model, settings);

            // Transfer materials
            var materials =model.ToMaterialBySurfaceStyles().ToDictionary(m => m.Id.Nid);
            result.ExportedModel.Materials.AddRange(materials.Values);

            Logger?.LogInformation("Starting model tesselation of {0}", model.Header.Name);
            // Retrieve enumeration of components having a geomety within given contexts            
            var sceneRepresentations = TesselatorInstance.Tesselate(model, result, progressing);
            var ifcClassifierMap = model.SchemaVersion.ToImplementingClassification<IIfcProduct>();

            Logger?.LogInformation("Starting model export of {0}", model.Header.Name);
            // Run transfer and log parents
            var parents = new HashSet<int>();
            var componentCache = new Dictionary<int, Component>();
            foreach (var sr in sceneRepresentations)
            {
                var product = model.Instances[sr.EntityLabel] as IIfcProduct;
                if (progressing?.State.IsAboutCancelling ?? false)
                {
                    Logger?.LogInformation("Canceled model export of '{0}'", model.Header.FileName);
                    progressing.State.MarkCanceled();
                    break;
                }

                Component c;
                if (!componentCache.TryGetValue(product.EntityLabel, out c))
                {
                    int? optParent;
                    c = product.ToComponent(out optParent, ifcClassifierMap)
                        .ToClassifedComponentWith(product, Settings.FeatureToClassifierFilter)
                        .ToFullyFeaturedWith(product, Settings.FeatureFilterRule);

                    componentCache.Add(product.EntityLabel, c);
                    result.ExportedModel.Components.Add(c);

                    if (optParent.HasValue)
                        parents.Add(optParent.Value);
                }

                c.Representations.AddRange(sr.Representations);
            }

            // Check for remaining components (i.e. missing parents without geometry)
            parents.RemoveWhere(id => componentCache.ContainsKey(id));
            Queue<int> missingInstance = new Queue<int>(parents);
            while (missingInstance.Count > 0)
            {
                if (progressing?.State.IsAboutCancelling ?? false)
                {
                    if (!progressing.State.IsCanceled)
                    {
                        Logger?.LogInformation("Canceled model export of '{0}'", model.Header.FileName);
                        progressing.State.MarkCanceled();
                    }
                    break;
                }

                if (model.Instances[missingInstance.Dequeue()] is IIfcProduct product)
                {
                    Component c;
                    if (!componentCache.TryGetValue(product.EntityLabel, out c))
                    {
                        int? optParent;
                        c = product.ToComponent(out optParent, ifcClassifierMap)
                            .ToClassifedComponentWith(product, Settings.FeatureToClassifierFilter)
                            .ToFullyFeaturedWith(product, Settings.FeatureFilterRule);

                        componentCache.Add(product.EntityLabel, c);                       

                        if (optParent.HasValue && !componentCache.ContainsKey(optParent.Value))
                            // Enqueue missing parents
                            missingInstance.Enqueue(optParent.Value);

                        result.ExportedModel.Components.Add(c);
                    }
                }
            }

            // Add default materials where required
            result.ExportedModel.Materials.AddRange(
                model.ToMaterialByColorMap(                    
                    DefaultProductColorMap,
                    result.ExportedModel.Components
                        .SelectMany(c => c.Representations)
                        .SelectMany(r => r.Bodies)
                        .Select(b => b.Material)
                        .Where(m => 0 > m.Nid)
                        .Distinct()
                )
            );

            return result;
        }
    }
}
