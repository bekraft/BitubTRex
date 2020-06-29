using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Xbim.Ifc;
using Xbim.Ifc4.Interfaces;
using Xbim.Common;

using Bitub.Ifc;

namespace Bitub.Ifc
{
    /// <summary>
    /// Follows the <i>Decomposes</i> aggregation relation ship.
    /// </summary>
    public static class IfcSpatialEnumerableIndexExtension
    {
        public static IEnumerable<EntityIndex<IfcLabeledElement<IPersistEntity>>> FlattenEntityIndex(this EntityIndex<IfcLabeledElement<IPersistEntity>> parent)
        {
            if (parent.Entity is IIfcSpatialElement s)
            {
                // Depth first for structural elements branch
                foreach (var relAggregate in s.Decomposes.Reverse())
                {
                    foreach (var subElement in relAggregate.RelatedObjects.OfType<IIfcSpatialElement>().Reverse())
                    {
                        var e = new EntityIndex<IfcLabeledElement<IPersistEntity>>(
                            parent,
                            new IfcLabeledElement<IPersistEntity>(subElement),
                            (t => t.FlattenEntityIndex()) 
                        );
                        yield return e;
                    }
                }

                // Finally return all products
                foreach (var product in s.ContainsElements.SelectMany(x => x.RelatedElements))
                {
                    yield return new EntityIndex<IfcLabeledElement<IPersistEntity>>(parent, new IfcLabeledElement<IPersistEntity>(product));
                }
            }
        }

        public static IEnumerable<EntityIndex<IfcLabeledElement<IPersistEntity>>> FlattenEntityIndex(this IIfcProject p)
        {
            var stack = new Stack<IIfcSpatialElement>(p.SpatialStructuralElements.Reverse());
            var index = new Dictionary<IIfcSpatialElement, EntityIndex<IfcLabeledElement<IPersistEntity>>>();
            while (0 < stack.Count)
            {
                var element = stack.Pop();
                EntityIndex<IfcLabeledElement<IPersistEntity>> parent = null;
                index.TryGetValue(element.IsContainedIn, out parent);

                // Depth first for structural elements branch
                foreach (var relAggregate in element.Decomposes.Reverse())
                {
                    foreach (var subElement in relAggregate.RelatedObjects.OfType<IIfcSpatialStructureElement>().Reverse())
                    {
                        var e = new EntityIndex<IfcLabeledElement<IPersistEntity>>(
                            parent,
                            new IfcLabeledElement<IPersistEntity>(subElement)

                        );
                        index.Add(subElement, e);
                        stack.Push(subElement);

                        yield return e;
                    }
                }

                // Finally return all products
                foreach (var product in element.ContainsElements.SelectMany(x => x.RelatedElements))
                {
                    yield return new EntityIndex<IfcLabeledElement<IPersistEntity>>(parent, new IfcLabeledElement<IPersistEntity>(product));
                }
            }
        }

        public static IEnumerable<EntityIndex<IfcLabeledElement<IPersistEntity>>> FlattenEntityIndex(this IfcStore s)
        {
            return FlattenEntityIndex(s.Instances.OfType<IIfcProject>().FirstOrDefault());
        }
    }
}