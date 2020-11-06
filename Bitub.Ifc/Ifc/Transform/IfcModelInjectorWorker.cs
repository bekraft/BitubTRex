using System;
using System.Collections.Generic;
using System.Linq;

using Xbim.Common;
using Xbim.Common.Geometry;
using Xbim.Common.Metadata;
using Xbim.Geometry.Engine.Interop;
using Xbim.Ifc;
using Xbim.Ifc4.Interfaces;
using Xbim.ModelGeometry.Scene;

using Microsoft.Extensions.Logging;

using System.Xml.Linq;

using Bitub.Dto;

namespace Bitub.Ifc.Transform
{
    public class IfcModelInjectorWorker
    {
        /*
        #region Private properties
        private Dictionary<IModel, XbimPlacementTree> placements = new Dictionary<IModel, XbimPlacementTree>();
        private Dictionary<IModel, XbimInstanceHandleMap> handles = new Dictionary<IModel, XbimInstanceHandleMap>();
        private XbimGeometryEngine engine;
        #endregion


        protected HashSet<Qualifier> CopyExpressType { get; private set; }
        protected IEnumerable<IfcStore> Models { get; private set; }

        public bool CopyPropertySets { get; set; } = true;

        public IfcModelInjectorWorker(IfcStore intoModel, IEnumerable<XName> copyTypes,
            IEnumerable<IfcStore> injectModels, IIfcSpatialStructureElement singleton = null)
        {
            Builder = IfcBuilder.WrapStore(intoModel);
            CopyExpressType = new HashSet<Qualifier>(copyTypes.Select(x => (XName)x.LocalName));
            Models = injectModels.ToArray();

            if (null == singleton)
                ContainerCandidate = new IIfcSpatialStructureElement[] { intoModel.Instances.OfType<IIfcSpatialStructureElement>().FirstOrDefault() };
            else
                ContainerCandidate = new IIfcSpatialStructureElement[] { singleton };
        }

        public ILogger Logger { get; set; }

        public IfcBuilder Builder { get; private set; }

        public IEnumerable<IIfcSpatialStructureElement> ContainerCandidate { get; private set; }

        protected XbimGeometryEngine Engine
        {
            get { return engine ?? (engine = new XbimGeometryEngine()); }
        }

        protected XbimMatrix3D PlacementOf(IIfcProduct p)
        {
            XbimPlacementTree tree;
            if (!placements.TryGetValue(p.Model, out tree))
            {
                tree = new XbimPlacementTree(p.Model, false);
                placements.Add(p.Model, tree);
            }
            return XbimPlacementTree.GetTransform(p, tree, Engine);
        }

        protected XbimInstanceHandleMap HandleMapOf(IPersistEntity p)
        {
            XbimInstanceHandleMap map;
            if (!handles.TryGetValue(p.Model, out map))
            {
                map = new XbimInstanceHandleMap(p.Model, Builder.Store);
                handles.Add(p.Model, map);
            }
            return map;
        }

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

                    if (CopyExpressType.Contains(i.ToQualifiedTypeName().GetLastFragment()))
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
        */
    }
}
