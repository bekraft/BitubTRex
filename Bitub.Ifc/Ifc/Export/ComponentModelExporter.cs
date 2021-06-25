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
using Google.Protobuf.WellKnownTypes;

namespace Bitub.Ifc.Export
{
    /// <summary>
    /// Transfer scene model data exporter. Internally uses an abstract tesselation provider. In case of Xbim tesselation use
    /// <code>
    /// var exporter = new IfcSceneExporter(new XbimTesselationContext(loggerFactory), loggerFactory);
    /// var result = await exporter.Run(myModel);
    /// </code>
    /// </summary>
    public class ComponentModelExporter : IExporter<ComponentScene>
    {
        #region Internals
        private readonly ILoggerFactory loggerFactory;
        private readonly ILogger logger;
        private readonly ITesselationContext<ExportPreferences> tesselatorInstance;
        #endregion

        /// <summary>
        /// Initial experter settings.
        /// </summary>
        public ExportPreferences Preferences { get; set; } = new ExportPreferences();

        /// <summary>
        /// Default color settings.
        /// </summary>
        public XbimColourMap DefaultProductColorMap { get; set; } = new XbimColourMap(StandardColourMaps.IfcProductTypeMap);

        /// <summary>
        /// Creates a new instance of a scene exporter.
        /// </summary>
        /// <param name="loggerFactory"></param>
        public ComponentModelExporter(ITesselationContext<ExportPreferences> tesselatorInstance, ILoggerFactory loggerFactory = null)
        {
            this.loggerFactory = loggerFactory;
            this.logger = loggerFactory?.CreateLogger<ComponentModelExporter>();
            this.tesselatorInstance = tesselatorInstance;            
        }

        /// <summary>
        /// Runs the model transformation.
        /// </summary>
        /// <param name="model">The IFC model</param>
        /// <returns>A scene</returns>
        public Task<ComponentScene> RunExport(IModel model, CancelableProgressing monitor)
        {
            return Task.Run(() =>
            {
                try
                {
                    var applied = new ExportPreferences(Preferences);
                    if (Preferences.BodyExportType == 0)
                        applied.BodyExportType = SceneBodyExportType.MeshBody;

                    return BuildScene(model, applied, monitor);
                }
                catch (Exception e)
                {
                    monitor?.State.MarkBroken();
                    logger.LogError("{0}: {1} [{2}]", e.GetType().Name, e.Message, e.StackTrace);
                    throw e;
                }
            });
        }

        // Runs the scene model export
        private ComponentScene BuildScene(IModel model,ExportPreferences exportSettings, CancelableProgressing progressing)
        {
            var exportContext = new ExportContext<ExportPreferences>(loggerFactory);
            exportContext.InitContextsAndScaleFromModel(model, exportSettings);

            // Transfer materials
            var componentScene = exportContext.CreateEmptySceneModelFromProject(model.Instances.OfType<IIfcProject>().First());
            var materials = model.ToMaterialBySurfaceStyles().ToDictionary(m => m.Id.Nid);
            componentScene.Materials.AddRange(materials.Values);
            
            logger?.LogInformation("Starting model tesselation of {0}", model.Header.Name);
            // Retrieve enumeration of components having a geomety within given contexts            
            var messages = tesselatorInstance.Tesselate(model, exportContext, progressing);
            var ifcClassifierMap = model.SchemaVersion.ToImplementingClassification<IIfcProduct>();

            logger?.LogInformation("Starting model export of {0}", model.Header.Name);

            // Run transfer and log parents
            var parents = new HashSet<int>();
            var componentCache = new Dictionary<int, Component>();
            foreach (var msg in messages)
            {
                if (progressing?.State.IsAboutCancelling ?? false)
                {
                    logger?.LogInformation("Canceled model export of '{0}'", model.Header.FileName);
                    progressing.State.MarkCanceled();
                    break;
                }

                switch (msg.messageType)
                {
                    case TesselationMessageType.Context:
                        componentScene.Contexts.Add(msg.SceneContext.sceneContext);
                        break;
                    case TesselationMessageType.Representation:
                        componentScene.ShapeBodies.Add(msg.ShapeRepresentation.shapeBody);
                        break;
                    case TesselationMessageType.Shape:
                        var product = model.Instances[msg.ProductShape.productLabel] as IIfcProduct;
                        Component c;
                        if (!componentCache.TryGetValue(product.EntityLabel, out c))
                        {
                            int? optParent;
                            c = product.ToComponent(out optParent, ifcClassifierMap)
                                .ToClassifedComponentWith(product, Preferences.FeatureToClassifierFilter)
                                .ToFullyFeaturedWith(product, Preferences.FeatureFilterRule);

                            componentCache.Add(product.EntityLabel, c);
                            componentScene.Components.Add(c);

                            if (optParent.HasValue)
                                parents.Add(optParent.Value);
                        }

                        c.Shapes.AddRange(msg.ProductShape.shapes);
                        break;
                }
            }

            // Check for remaining components (i.e. missing parents without geometry)
            parents.RemoveWhere(id => componentCache.ContainsKey(id));
            var missingInstance = new Queue<int>(parents);
            while (missingInstance.Count > 0)
            {
                if (progressing?.State.IsAboutCancelling ?? false)
                {
                    if (!progressing.State.IsCanceled)
                    {
                        logger?.LogInformation("Canceled model export of '{0}'", model.Header.FileName);
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
                            .ToClassifedComponentWith(product, Preferences.FeatureToClassifierFilter)
                            .ToFullyFeaturedWith(product, Preferences.FeatureFilterRule);

                        componentCache.Add(product.EntityLabel, c);                       

                        if (optParent.HasValue && !componentCache.ContainsKey(optParent.Value))
                            // Enqueue missing parents
                            missingInstance.Enqueue(optParent.Value);

                        componentScene.Components.Add(c);
                    }
                }
            }

            // Add default materials where required
            componentScene.Materials.AddRange(
                model.ToMaterialByColorMap(                    
                    DefaultProductColorMap,
                    componentScene.Components
                        .SelectMany(c => c.Shapes)
                        .Select(s => s.Material)
                        .Where(m => 0 > m.Nid)
                        .Distinct()
                )
            );

            return componentScene;
        }        
    }
}
