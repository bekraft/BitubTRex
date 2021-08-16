using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Xbim.Common;

using Xbim.Ifc4.Interfaces;
using Xbim.Ifc4.Kernel;

namespace Bitub.Ifc
{
    public static class IfcRelationShipExtensions
    {
        #region Decomposition & spatial containment

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
        public static IEnumerable<T> Children<T>(this IIfcObjectDefinition o) where T : IIfcObjectDefinition
        {
            var productSubs = o.SubObjects<T>();
            if (o is IIfcSpatialElement s)
                return Enumerable.Concat(productSubs, s.ContainsElements.SelectMany(r => r.RelatedElements.OfType<T>()));
            else
                return productSubs;
        }

        /// <summary>
        /// Returns all products of a spatial element.
        /// </summary>
        /// <typeparam name="T">The product type</typeparam>
        /// <param name="o">The parent object</param>
        /// <returns></returns>
        public static IEnumerable<T> ChildProducts<T>(this IIfcSpatialElement o) where T : IIfcProduct
        {
            return o.ContainsElements.SelectMany(r => r.RelatedElements.OfType<T>());
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

        #endregion

        #region Property relations

        public static IEnumerable<T> PropertiesAll<T>(this IIfcObject p) where T : IIfcProperty
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

        public static IEnumerable<T> PropertySets<T>(this IIfcObject o) where T : IIfcPropertySetDefinition
        {
            return o.IsDefinedBy.SelectMany(r => r.PropertySet<T>());
        }

        /// <summary>
        /// All property sets of object. Returns a tuple of set name vs. array of properties.
        /// </summary>
        /// <typeparam name="T">Property type scope</typeparam>
        /// <param name="p">Object in context</param>
        /// <returns>Sequence of pset names vs. array of hosted properties.</returns>
        public static IEnumerable<Tuple<string, T[]>> PropertiesSets<T>(this IIfcObject p) where T : IIfcProperty
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

        /// <summary>
        /// All properties of type T within property set.
        /// </summary>
        /// <typeparam name="T">Property type scope</typeparam>
        /// <param name="set"></param>
        /// <returns>Sequence of properties</returns>
        public static IEnumerable<T> Properties<T>(this IIfcPropertySetDefinition set) where T : IIfcProperty
        {
            if (set is IIfcPropertySet pSet)
                return pSet.HasProperties.OfType<T>();
            else
                return Enumerable.Empty<T>();
        }

        #endregion

        #region General relation handling

        /// <summary>
        /// Will transfer all existing relations of a (more abstract) template to a (more specific) target instance.
        /// </summary>
        /// <typeparam name="T1">Template type</typeparam>
        /// <typeparam name="T2">Target type as specialisation of T1</typeparam>
        /// <param name="target">The target instance to attach relations to</param>
        /// <param name="template">The template instance providing relations (only existing by instance)</param>
        /// <returns>The modified target instance</returns>
        public static T2 CreateSameRelationshipsLike<T1, T2>(this T2 target, T1 template) where T1 : IPersistEntity where T2 : T1
        {
            // Scan trough hosted indirect relations of template type T
            foreach (var relationProperty in typeof(T1)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(property => typeof(IEnumerable).IsAssignableFrom(property.GetMethod.ReturnType) && property.GetMethod.ReturnType.IsGenericType)
                .Where(property => typeof(IIfcRelationship).IsAssignableFrom(property.GetMethod.ReturnType.GenericTypeArguments[0])))
            {   
                // Scan through relation objects of type IEnumerable<? extends IIfcRelationship>
                foreach (var relation in (relationProperty.GetValue(template) as IEnumerable))
                {
                    foreach (var invRelationProperty in relation.GetType()
                        .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                        .Where(property => typeof(IItemSet).IsAssignableFrom(property.ReflectedType) && property.ReflectedType.IsGenericType)
                        .Where(property => typeof(T1).IsAssignableFrom(property.ReflectedType.GetGenericArguments()[0])))
                    {
                        var itemSet = invRelationProperty.GetValue(relation);
                        itemSet.GetType().GetMethod("Add").Invoke(itemSet, new object[] { target });
                    }
                }                
            }
            return target;
        }

        /// <summary>
        /// Finds a relation typed by lower constraint <c>TParam</c> which implements <see cref="IItemSet"/>.
        /// </summary>
        /// <typeparam name="TParam">The relation type</typeparam>
        /// <param name="t">The host type</param>
        /// <param name="relationName">The relation name</param>
        /// <returns>Reflected property info.</returns>
        public static PropertyInfo GetLowerConstraintRelationType<TParam>(this Type t, string relationName)
        {
            var propertyInfo = t.GetInterfaces()
                .SelectMany(t => t.GetProperties())
                .Where(p => p.Name == relationName && typeof(IItemSet).IsAssignableFrom(p.PropertyType))
                .Where(p => p.PropertyType.GetGenericArguments().All(t => t.IsAssignableFrom(typeof(TParam))))
                .FirstOrDefault();
            
            return propertyInfo;
        }

        /// <summary>
        /// Will add related instances to host instance using given <c>relationName</c> and lower constraint type <c>TParam</c>.
        /// </summary>
        /// <typeparam name="TParam">The relation type</typeparam>
        /// <typeparam name="T">The host type (implicit)</typeparam>
        /// <param name="hostInstance">The host instance</param>
        /// <param name="relationName">The relation name</param>
        /// <param name="instances">The related instances</param>
        /// <returns>The modifid host instance</returns>
        public static T AddRelationsByLowerConstraint<TParam, T>(this T hostInstance, string relationName, IEnumerable<TParam> instances) where T: IPersistEntity
        {
            var propertyInfo = hostInstance.GetType().GetLowerConstraintRelationType<TParam>(relationName);
            if (null == propertyInfo)
                throw new ArgumentException($"Relation '{relationName}' is know implementing type of '{typeof(TParam).Name}' or not existing.");

            var items = propertyInfo.GetValue(hostInstance);

            items.GetType().GetMethod("AddRange").Invoke(items, new object[] { instances.Cast<TParam>().ToList() });

            return hostInstance;
        }

        #endregion
    }
}