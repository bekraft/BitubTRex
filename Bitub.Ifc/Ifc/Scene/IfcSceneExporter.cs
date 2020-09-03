using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Extensions.Logging;

using Xbim.Common;

using Xbim.Ifc;
using Xbim.Ifc4.Interfaces;

using Bitub.Transfer;
using Bitub.Transfer.Scene;

using Component = Bitub.Transfer.Scene.Component;
using System.Threading.Tasks;
using Bitub.Transfer.Classify;

namespace Bitub.Ifc.Scene
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
        public IfcSceneExportSettings Settings { get; set; } = new IfcSceneExportSettings();

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
        public Task<IfcSceneExportSummary> Run(IModel model, CancelableProgressing monitor)
        {
            return Task.Run(() =>
            {
                try
                {
                    return DoSceneModelTransfer(model, new IfcSceneExportSettings(Settings), monitor);
                }
                catch (Exception e)
                {
                    monitor?.State.MarkBroken();
                    Logger.LogError("{0}: {1} [{2}]", e.GetType().Name, e.Message, e.StackTrace);
                    return new IfcSceneExportSummary(model, Settings) { FailureReason = e };
                }
            });
        }

        // Runs the scene model export
        private IfcSceneExportSummary DoSceneModelTransfer(IModel model, IfcSceneExportSettings settings, CancelableProgressing progressing)
        {
            // Generate new summary
            var summary = new IfcSceneExportSummary(model, settings);

            // Transfer materials
            var materials = StylesToMaterial(model).ToDictionary(m => m.Id.Nid);
            summary.Scene.Materials.AddRange(materials.Values);

            Logger?.LogInformation("Starting model tesselation of {0}", model.Header.Name);
            // Retrieve enumeration of components having a geomety within given contexts            
            var sceneRepresentations = TesselatorInstance.Tesselate(model, summary, progressing);

            Logger?.LogInformation("Starting model export of {0}", model.Header.Name);
            // Run transfer and log parents
            var parents = new HashSet<int>();
            foreach (var sr in sceneRepresentations)
            {
                var p = model.Instances[sr.EntityLabel] as IIfcProduct;
                if (progressing?.State.IsAboutCancelling ?? false)
                {
                    Logger?.LogInformation("Canceled model export of '{0}'", model.Header.FileName);
                    progressing.State.MarkCanceled();
                    break;
                }

                Component c;
                if (!summary.ComponentCache.TryGetValue(p.EntityLabel, out c))
                {
                    int? optParent;
                    c = CreateComponent(p, Enumerable.Empty<Classifier>(), out optParent);
                    summary.ComponentCache.Add(p.EntityLabel, c);
                    summary.Scene.Components.Add(c);

                    if (optParent.HasValue)
                        parents.Add(optParent.Value);
                }

                c.Representations.AddRange(sr.Representations);
            }

            // Check for remaining components (i.e. missing parents without geometry)
            parents.RemoveWhere(id => summary.ComponentCache.ContainsKey(id));
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
                    if (!summary.ComponentCache.TryGetValue(product.EntityLabel, out c))
                    {
                        int? optParent;
                        c = CreateComponent(product, Enumerable.Empty<Classifier>(), out optParent);
                        summary.ComponentCache.Add(product.EntityLabel, c);                       

                        if (optParent.HasValue && !summary.ComponentCache.ContainsKey(optParent.Value))
                            // Enqueue missing parents
                            missingInstance.Enqueue(optParent.Value);

                        summary.Scene.Components.Add(c);
                    }
                }
            }

            // Add default materials where required
            summary.Scene.Materials.AddRange(
                DefaultMaterials(
                    model,
                    summary.Scene.Components
                        .SelectMany(c => c.Representations)
                        .SelectMany(r => r.Bodies)
                        .Select(b => b.Material)
                        .Where(m => 0 > m.Nid)
                        .Distinct()
                )
            );

            return summary;
        }


        private IEnumerable<Material> StylesToMaterial(IModel model)
        {
            foreach (var style in model.Instances.OfType<IIfcSurfaceStyle>())
                yield return style.ToMaterial();                       
        }

        private IEnumerable<Material> DefaultMaterials(IModel model, IEnumerable<RefId> refids)
        {
            foreach(var rid in refids)
            {
                var defaultStyle = model.Metadata.GetType((short)Math.Abs(rid.Nid));
                var defaultColor = DefaultProductColorMap[defaultStyle.Name];
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

        // Creates a new component descriptor
        private Component CreateComponent(IIfcProduct product, IEnumerable<Classifier> concepts, out int? optParentLabel)
        {
            var parent = product.Parent<IIfcProduct>().FirstOrDefault();
            var component = new Component
            {
                Id = product.GlobalId.ToGlobalUniqueId(),
                // -1 reserved for roots
                Parent = parent?.GlobalId.ToGlobalUniqueId(),
                Name = product.Name ?? "",
            };

            // Add IFC express type by default
            component.Concepts.Add(CommonExtensions.DefaultXbimEntityQualifier(product).ToClassifier());

            // Add additiona user qualifiers
            foreach (var userProductQualifier in Settings.UserProductQualifier)
                component.Concepts.Add(userProductQualifier(product).ToClassifier());

            component.Concepts.AddRange(concepts);

            component.Children.AddRange(product.Children<IIfcProduct>().Select(p => p.GlobalId.ToGlobalUniqueId()));
            optParentLabel = parent?.EntityLabel;
            return component;
        }
    }
}
