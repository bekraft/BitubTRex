using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Bitub.Dto;

namespace Bitub.Ifc
{
    /// <summary>
    /// A type scope wrapping a partial aspect of a type registry.
    /// </summary>
    public class TypeScope
    {
        public readonly AssemblyScope assemblyScope;
        public readonly Type baseType;
        public readonly Module[] modules;

        #region Internals
        private readonly IDictionary<Qualifier, Type> typeRegistry;
        #endregion

        public TypeScope(Type typeScope, AssemblyScope scope, string moduleName) 
            : this(typeScope, scope, scope.Modules.Where(m => m.Name.Equals(moduleName)).ToArray())
        { }

        public TypeScope(Type typeScope, AssemblyScope scope)
            : this(typeScope, scope, scope.Modules.ToArray())
        { }

        public TypeScope(Type typeScope, AssemblyScope scope, Module[] scopeModules)
        {
            assemblyScope = scope;
            baseType = typeScope;
            modules = scopeModules;            

            typeRegistry = scope
                .Implementing(baseType)
                .Where(t => !t.IsAbstract && !t.IsInterface && IsTypeOfAssemblySpace(t))
                .ToDictionary(t => GetScopedQualifier(t));
        }

        public bool IsTypeOfAssemblySpace(Type t)
        {
            return modules.Any(m => m == t.Module);
        }

        public virtual Qualifier GetScopedQualifier(Type t)
        {
            var baseQualifier = assemblyScope.GetModuleQualifer(t.Module);
            if (null != baseQualifier)
                return baseQualifier.Append(t.Name);
            else
                return t.ToQualifier();
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
