using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace Bitub.Ifc
{
    public static class TypeExtension
    {
        public static XName XLabel(this Type t)
        {
            return XName.Get(t.Name.ToUpper(), t.Assembly.GetName().Name);
        }
    }

    /// <summary>
    /// A type scope wrapping a partial aspect of a type registry.
    /// </summary>
    /// <typeparam name="T">The base type</typeparam>
    public class TypeScope<T>
    {
        public readonly RegisteredTypeFactory Factory;
        public readonly Type Base;
        public readonly string TypeSpace;

        private readonly IDictionary<XName, Type> TypeRegistry;

        internal protected TypeScope(RegisteredTypeFactory rfactory, string typeSpace = null)
        {
            Factory = rfactory;
            Base = typeof(T);
            TypeSpace = typeSpace;
            TypeRegistry = rfactory
                .Implementing(typeof(T))
                .Where(t => !t.IsAbstract && !t.IsInterface && (null==TypeSpace || t.Assembly.GetName().Name == TypeSpace))
                .ToDictionary<Type,XName>(t => t.XLabel() );
        }

        public IEnumerable<XName> Implementations
        {
            get => TypeRegistry.Keys;
        }

        /// <summary>
        /// Find exact match.
        /// </summary>
        /// <param name="t">Some type name</param>
        /// <returns>A type matching the given name</returns>
        public Type this[XName t]
        {
            get => TypeRegistry[t];
        }

        /// <summary>
        /// Find match by super constraint.
        /// </summary>
        /// <typeparam name="E">Super type</typeparam>
        /// <returns>Enumerable of sub types</returns>
        public IEnumerable<Type> Implementing<E>()
        {
            return TypeRegistry.Values.Where(t => t.IsSubclassOf(typeof(E)) || t.GetInterfaces().Any(i => i == typeof(E)));
        }
    }

    /// <summary>
    /// A type registry bound to existing assemblies.
    /// </summary>
    public class RegisteredTypeFactory
    {
        public readonly IEnumerable<Assembly> BoundAssemblies;

        public RegisteredTypeFactory(params Assembly[] assemblies)
        {
            BoundAssemblies = assemblies;
        }

        public IEnumerable<Type> Implementing(Type baseType)
        {
            return BoundAssemblies.SelectMany(a => a.ExportedTypes.Where(t => t.IsSubclassOf(baseType) || t.GetInterfaces().Any(i => i == baseType)));
        }

        public IEnumerable<Type> GetLocalType(XName name)
        {
            return BoundAssemblies.SelectMany(a => a.ExportedTypes.Where(t => t.Name == name.LocalName));
        }

        public IEnumerable<XName> TypeNames
        {
            get => BoundAssemblies.SelectMany(a => a.ExportedTypes.Select(t => XName.Get(t.Assembly.GetName().Name, t.Name)));            
        }

        public IEnumerable<string> TypeSpaces
        {
            get => BoundAssemblies.Select(a => a.GetName().Name);
        }

        public TypeScope<TBase> GetScopeOf<TBase>(string typeSpace = null)
        {
            return new TypeScope<TBase>(this, typeSpace);
        }
    }
}
