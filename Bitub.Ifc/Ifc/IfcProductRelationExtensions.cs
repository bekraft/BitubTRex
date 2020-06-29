using System;
using System.Collections.Generic;
using System.Linq;

using Xbim.Ifc4.Interfaces;
using Xbim.Ifc4.Kernel;

namespace Bitub.Ifc
{
    public static class IfcProductRelationExtensions
    {
        /// <summary>
        /// Delegates the <i>IsDecomposedBy</i> relationship of a single object.
        /// </summary>
        /// <param name="p">An IfcObjectDefinition</param>
        /// <returns>An enumeration of no, one or more IfcObjectDefinition instances</returns>
        public static IEnumerable<T> SubObjects<T>(this IIfcObjectDefinition p) where T : IIfcObjectDefinition
        {
            return p
                .IsDecomposedBy
                .SelectMany(s => s.RelatedObjects.OfType<T>());
        }

        /// <summary>
        /// Flattens the <i>Decomposes</i> relationship of an object. According to IFC constraints
        /// it should report only a single instance of IfcObjectDefinition.
        /// </summary>
        /// <param name="p">An IfcObjectDefinition</param>
        /// <returns>An enumeration (having a sinlge or no object)</returns>
        public static IEnumerable<T> SuperObject<T>(this IIfcObjectDefinition p) where T : IIfcObjectDefinition
        {
            return p
                .Decomposes
                .Select(s => s.RelatingObject).OfType<T>().Distinct();
        }

        /// <summary>
        /// Returns an enumeration of child objects having either a decomposition relation or spatial containment
        /// relation with the argument object.
        /// </summary>
        /// <typeparam name="T">A preferred type</typeparam>
        /// <param name="o">The parent</param>
        /// <returns>An enumeration of objects of given type</returns>
        public static IEnumerable<T> Children<T>(this IIfcObjectDefinition o) where T : IIfcProduct
        {
            var productSubs = o.SubObjects<T>();
            if (o is IIfcSpatialElement s)
                return Enumerable.Concat(productSubs, s.ContainsElements.SelectMany(r => r.RelatedElements.OfType<T>()));
            else
                return productSubs;
        }

        /// <summary>
        /// Returns an enumeration of parent objects having either a decomposition relation or spatial containment
        /// relation with the argument object.
        /// </summary>
        /// <typeparam name="T">A preferred type</typeparam>
        /// <param name="o">The child</param>
        /// <returns>An enumeration of parent objects</returns>
        public static IEnumerable<T> Parent<T>(this IIfcObjectDefinition o) where T : IIfcObjectDefinition
        {
            var productSupers = o.SuperObject<T>();
            if (o is IIfcProduct p)
                return Enumerable.Concat(productSupers, new IIfcProduct[] { p.IsContainedIn }.OfType<T>()).Distinct();
            else
                return productSupers;
        }

        public static IEnumerable<T> PropertiesAll<T>(this IIfcProduct p) where T : IIfcProperty
        {
            return p.IsDefinedBy.SelectMany(r =>
            {
                if (r.RelatingPropertyDefinition is IIfcPropertySetDefinition set)
                    return set.Properties<T>();
                else if (r.RelatingPropertyDefinition is IfcPropertySetDefinitionSet setOfSet)
                    return setOfSet.PropertySetDefinitions.SelectMany(s => s.Properties<T>());
                else
                    return Enumerable.Empty<T>();
            });
        }

        public static IEnumerable<T> PropertySet<T>(this IIfcRelDefinesByProperties r) where T : IIfcPropertySetDefinition
        {
            if (r.RelatingPropertyDefinition is IIfcPropertySetDefinition set)
                return Enumerable.Repeat(set, 1).OfType<T>();
            else if (r.RelatingPropertyDefinition is IfcPropertySetDefinitionSet setOfSet)
                return setOfSet.PropertySetDefinitions.OfType<T>();
            else
                return Enumerable.Empty<T>();
        }

        public static IEnumerable<Tuple<string, T[]>> PropertiesSets<T>(this IIfcProduct p) where T : IIfcProperty
        {
            return p.IsDefinedBy.SelectMany(r =>
            {
                if (r.RelatingPropertyDefinition is IIfcPropertySetDefinition set)
                    return Enumerable.Repeat(new Tuple<string, T[]>(set.Name, set.Properties<T>().ToArray()), 1);
                else if (r.RelatingPropertyDefinition is IfcPropertySetDefinitionSet setOfSet)
                    return setOfSet.PropertySetDefinitions.Select(s => new Tuple<string,T[]>(s.Name, s.Properties<T>().ToArray()));
                else
                    return Enumerable.Empty<Tuple<string, T[]>>();
            });
        }

        public static IEnumerable<T> Properties<T>(this IIfcPropertySetDefinition set) where T : IIfcProperty
        {
            if (set is IIfcPropertySet pSet)
                return pSet.HasProperties.OfType<T>();
            else
                return Enumerable.Empty<T>();
        }
    }
}