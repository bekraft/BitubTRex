using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml.Linq;

using Bitub.Dto;

namespace Bitub.Ifc
{
    /// <summary>
    /// A type scope wrapping a partial aspect of a type registry.
    /// </summary>
    /// <typeparam name="T">The base type</typeparam>
    public class TypeScope<T>
    {
        public readonly AssemblyScope assemblyScope;
        public readonly Type baseType;
        public readonly AssemblyName assemblySpace;

        #region Internals
        private readonly IDictionary<Qualifier, Type> typeRegistry;
        private readonly Regex typeReplacePattern;
        private readonly string typeReplaceBy;
        #endregion

        public TypeScope(AssemblyScope scope, string assemblySpaceName) 
            : this(scope, new AssemblyName(assemblySpaceName), null, "")
        { }

        public TypeScope(AssemblyScope scope, string assemblySpaceName, Regex replacePattern, string replaceBy)
            : this(scope, new AssemblyName(assemblySpaceName), replacePattern, replaceBy)
        { }

        public TypeScope(AssemblyScope scope, AssemblyName assemblySpaceName, Regex replacePattern, string replaceBy)
        {
            assemblyScope = scope;
            baseType = typeof(T);
            assemblySpace = assemblySpaceName;
            typeReplacePattern = replacePattern;
            typeReplaceBy = replaceBy;

            typeRegistry = scope
                .Implementing(typeof(T))
                .Where(t => !t.IsAbstract && !t.IsInterface && IsTypeOfAssemblySpace(t))
                .ToDictionary(t => GetScopeQualifier(t));
        }

        public bool IsTypeOfAssemblySpace(Type t)
        {
            return (null == assemblySpace || t.Assembly.GetName().Name.Equals(assemblySpace));
        }

        public Qualifier GetScopeQualifier(Type t)
        {
            return t.ToQualifier(typeReplacePattern, typeReplaceBy);
        }

        public IEnumerable<Qualifier> TypeQualifiers
        {
            get => typeRegistry.Keys;
        }

        public IEnumerable<Type> Types
        {
            get => typeRegistry.Values;
        }

        /// <summary>
        /// Find exact match.
        /// </summary>
        /// <param name="q">Some type name</param>
        /// <returns>A type matching the given name</returns>
        public Type this[Qualifier q]
        {
            get => typeRegistry[q];
        }

        /// <summary>
        /// Find match by super constraint.
        /// </summary>
        /// <typeparam name="E">Super type</typeparam>
        /// <returns>Enumerable of sub types</returns>
        public IEnumerable<Type> Implementing<E>()
        {
            return typeRegistry.Values.Where(t => t.IsSubclassOf(typeof(E)) || t.GetInterfaces().Any(i => i == typeof(E)));
        }
    }
}
