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

        public IEnumerable<string> SpaceNames
        {
            get => assemblySpaces.Select(a => a.GetName().Name);
        }

        public TypeScope<TBase> GetScopeOf<TBase>(string assemblySpaceName = null)
        {
            return new TypeScope<TBase>(this, assemblySpaceName);
        }
    }
}
