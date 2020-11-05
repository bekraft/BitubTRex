using System;
using System.Collections.Generic;
using System.Linq;

using Xbim.Common;
using Xbim.Common.Geometry;
using Xbim.Common.Metadata;
using Xbim.Geometry.Engine.Interop;
using Xbim.Ifc;
using Xbim.Ifc4.Interfaces;
using Xbim.IO;
using Xbim.ModelGeometry.Scene;
using Xbim.ModelGeometry.Scene.Extensions;

using Microsoft.Extensions.Logging;
using System.ComponentModel;
using System.Xml.Linq;

namespace Bitub.Ifc.Transform
{
    public enum IfcModelInjectorMode
    {
        SingletonContainer, CaseSensitiveNameMatchingContainer, NameMatchingContainer, ZValueStoreyMatchingContainer
    }

    public class IfcModelInjectorWorker
    {
        #region Private properties
        private Dictionary<IModel, XbimPlacementTree> Placements { get; set; } = new Dictionary<IModel, XbimPlacementTree>();
        private Dictionary<IModel, XbimInstanceHandleMap> HandleMaps { get; set; } = new Dictionary<IModel, XbimInstanceHandleMap>();
        private XbimGeometryEngine _engine;
        #endregion

        /// <summary>
        /// New Model Injector Package. 
        /// </summary>
        /// <param name="intoModel">The merge-into model instance</param>
        /// <param name="copyTypes">The type enumeration to copy over</param>
        /// <param name="injectModels">The models to query & copy</param>
        /// <param name="singleton">The container (or null by default indicating to find the first occurance)</param>
        public IfcModelInjectorWorker(IfcStore intoModel, IEnumerable<XName> copyTypes,
            IEnumerable<IfcStore> injectModels, IIfcSpatialStructureElement singleton = null)
        {
            Builder = IfcBuilder.WrapStore(intoModel);
            CopyExpressType = new HashSet<XName>(copyTypes.Select(x => (XName)x.LocalName));
            Models = injectModels.ToArray();

            if (null == singleton)
                ContainerCandidate = new IIfcSpatialStructureElement[] { intoModel.Instances.OfType<IIfcSpatialStructureElement>().FirstOrDefault() };
            else
                ContainerCandidate = new IIfcSpatialStructureElement[] { singleton };

            InjectorMode = IfcModelInjectorMode.SingletonContainer;
        }

        public ILogger Logger { get; set; }

        public IfcBuilder Builder { get; private set; }

        public IfcModelInjectorMode InjectorMode { get; private set; }

        public IEnumerable<IIfcSpatialStructureElement> ContainerCandidate { get; private set; }

        protected XbimGeometryEngine Engine
        {
            get { return _engine ?? (_engine = new XbimGeometryEngine()); }
        }

        protected XbimMatrix3D PlacementOf(IIfcProduct p)
        {
            XbimPlacementTree tree;
            if (!Placements.TryGetValue(p.Model, out tree))
            {
                tree = new XbimPlacementTree(p.Model, false);
                Placements.Add(p.Model, tree);
            }
            return XbimPlacementTree.GetTransform(p, tree, Engine);
        }

        protected XbimInstanceHandleMap HandleMapOf(IPersistEntity p)
        {
            XbimInstanceHandleMap map;
            if (!HandleMaps.TryGetValue(p.Model, out map))
            {
                map = new XbimInstanceHandleMap(p.Model, Builder.Store);
                HandleMaps.Add(p.Model, map);
            }
            return map;
        }

        public bool AppendIfcProductType(Type t)
        {
            if (!t.IsSubclassOf(typeof(IIfcProduct)))
                throw new ArgumentException($"Expecting sub-classes of IfcProduct");

            return CopyExpressType.Add(t.XLabel().LocalName);
        }

        protected HashSet<XName> CopyExpressType { get; private set; }
        protected IEnumerable<IfcStore> Models { get; private set; }

        public bool CopyPropertySets { get; set; } = true;

        public IEnumerable<IIfcProduct> InstanceCandidates => Models
            .SelectMany(m => m.Instances)
            .OfType<IIfcProduct>()
            .Where(i => CopyExpressType.Contains(i.ExpressType.Name));

        private object PropertyTransform(ExpressMetaProperty property, object parentObject)
        {
            //only bring over IsDefinedBy and IsTypedBy inverse relationships which will take over all properties and types
            if (property.EntityAttribute.Order < 0 && CopyPropertySets && !(
                property.PropertyInfo.Name == nameof(IIfcProduct.IsDefinedBy) ||
                property.PropertyInfo.Name == nameof(IIfcProduct.IsTypedBy)
                ))
                return null;

            return property.PropertyInfo.GetValue(parentObject, null);
        }

        private IIfcSpatialStructureElement FindContainer(IIfcProduct injected)
        {
            switch (InjectorMode)
            {
                case IfcModelInjectorMode.SingletonContainer:
                    return ContainerCandidate.FirstOrDefault();
                case IfcModelInjectorMode.NameMatchingContainer:
                case IfcModelInjectorMode.CaseSensitiveNameMatchingContainer:
                case IfcModelInjectorMode.ZValueStoreyMatchingContainer:
                    var msg = $"Missing implementation for {InjectorMode}";
                    Logger?.LogError(msg);
                    throw new NotImplementedException(msg);
            }
            throw new ArgumentException($"Unhandled injector mode {InjectorMode}");
        }

        public BackgroundWorker RunAsync(RunWorkerCompletedEventHandler completionHander = null, ProgressChangedEventHandler progressChanged = null)
        {
            var backgroundWorker = new BackgroundWorker();
            backgroundWorker.DoWork += delegate (object sender, DoWorkEventArgs args)
            {
                var bw = sender as BackgroundWorker;
                var miw = args.Argument as IfcModelInjectorWorker;
                miw.RunModelMerge(bw.ReportProgress);
                args.Result = miw;
            };
            backgroundWorker.ProgressChanged += progressChanged;
            backgroundWorker.RunWorkerCompleted += delegate (object sender, RunWorkerCompletedEventArgs args)
            {
                var miw = args.Result as IfcModelInjectorWorker;
                Logger?.LogInformation($"Model ingest '{miw.Builder.Store.FileName}' done.");
            };
            backgroundWorker.RunWorkerCompleted += completionHander;
            backgroundWorker.RunWorkerAsync(this);
            return backgroundWorker;
        }

        /// <summary>
        /// Runs the model ingest & merge procedure.
        /// </summary>
        /// <param name="progressAction">The action to take on progress</param>
        /// <param name="cancellationPending">The indicator of a pending cancel</param>
        public void RunModelMerge(ReportProgressDelegate progressAction = null, Func<bool> cancellationPending = null)
        {
            var candidateInstances = Models.SelectMany(m => m.Instances).OfType<IIfcProduct>();
            long totalCount = Models.Select(m => m.Instances.Count).Sum();
            long currentCount = 0;
            int recentPercentage = 0;

            Builder.Wrap(s =>
            {
                Dictionary<IIfcProduct, XbimMatrix3D> t2Inv = new Dictionary<IIfcProduct, XbimMatrix3D>();
                progressAction?.Invoke(0, $"Start injection into ${s.FileName}");

                foreach (var i in candidateInstances)
                {
                    ++currentCount;
                    if (cancellationPending?.Invoke() ?? false)
                    {
                        Logger?.LogInformation($"Canceling model ingest of {s.FileName}");
                        // Don't apply changes
                        return false;
                    }

                    if (CopyExpressType.Contains(i.ToXName().LocalName))
                    {
                        var newProduct = s.InsertCopy(i, HandleMapOf(i), PropertyTransform, true, false);

                        if (i.ObjectPlacement is IIfcGridPlacement gp)
                            // Since grids are products, they were to be embedded into the model too
                            Logger?.LogWarning($"Can't transfer grid placement #{gp.EntityLabel}. Replacing #{newProduct.EntityLabel} of {i.ExpressType} with local placement.");

                        // Get original aggregated transformation
                        var t1 = PlacementOf(i);
                        // Get container transformation
                        var container = FindContainer(i);
                        XbimMatrix3D t2;
                        if (t2Inv.TryGetValue(container, out t2))
                        {
                            // Compute inv of container local placement
                            t2 = PlacementOf(container);
                            t2.Invert();
                            t2Inv.Add(container, t2);
                        }

                        // New placement based on given global transformation
                        var c2 = t2 * t1;

                        var placement = s.NewLocalPlacement(c2, true);

                        // Relate to product and container
                        newProduct.ObjectPlacement = placement;
                        s.NewContains(container).RelatedElements.Add(newProduct);
                    }

                    int percentage = (int)Math.Ceiling(100.0 * currentCount / totalCount);
                    if (recentPercentage < percentage)
                        progressAction?.Invoke(recentPercentage = percentage, null);
                }

                progressAction?.Invoke(100, $"Finalized injection into ${s.FileName}");
                // Apply changes
                return true;
            });
        }
    }
}
