using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Bitub.Dto;

namespace Bitub.Ifc
{
    /// <summary>
    /// An assembly type classification helper.
    /// </summary>
    public class AssemblyScope
    {
        public readonly Assembly[] assemblySpaces;
        public readonly StringComparison comparisonType;

        public AssemblyScope(params Assembly[] assemblies) : this(StringComparison.Ordinal, assemblies)
        {
        }

        public AssemblyScope(StringComparison stringComparisonType, params Assembly[] assemblies)
        {
            assemblySpaces = assemblies;
            comparisonType = stringComparisonType;
        }

        public IEnumerable<Type> Implementing(Type baseType)
        {
            return assemblySpaces.SelectMany(a => a.ExportedTypes.Where(t => t.IsSubclassOf(baseType) || t.GetInterfaces().Any(i => i == baseType)));
        }

        public IEnumerable<Type> GetLocalType(Qualifier name)
        {
            return assemblySpaces.SelectMany(a => a.ExportedTypes.Where(t => 0 == string.Compare(t.Name, name.GetLastFragment(), comparisonType)));
        }

        public IEnumerable<Qualifier> TypeNames
        {
            get => assemblySpaces.SelectMany(a => a.ExportedTypes.Select(t => t.ToQualifier()));            
        }

        public IEnumerable<AssemblyName> SpaceNames
        {
            get => assemblySpaces.Select(a => a.GetName());
        }

        public IEnumerable<Module> Modules
        {
            get => assemblySpaces.SelectMany(a => a.Modules);
        }

        public virtual Qualifier GetModuleQualifer(Module module)
        {
            return module.Name.ToQualifier();
        }

        public virtual TypeScope GetScopeOf<TBase>()
        {
            return GetScopeOf<TBase>(Modules.ToArray());
        }

        public virtual TypeScope GetScopeOf<TBase>(Module module)
        {
            return new TypeScope(typeof(TBase), this, new Module[] { module });
        }

        public virtual TypeScope GetScopeOf<TBase>(IEnumerable<Module> modules)
        {
            return new TypeScope(typeof(TBase), this, modules.ToArray());
        }
    }
}
