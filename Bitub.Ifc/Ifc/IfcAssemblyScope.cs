using Bitub.Dto;
using System;
using System.Collections.Generic;
using System.Reflection;
using Xbim.Common;
using Xbim.Common.Metadata;

using Xbim.Common.Step21;

namespace Bitub.Ifc
{
    public class IfcAssemblyScope : AssemblyScope 
    {
        public static readonly IfcAssemblyScope Ifc2x3 = IfcAssemblyScope.FromFactoryType<Xbim.Ifc2x3.EntityFactoryIfc2x3>(XbimSchemaVersion.Ifc2X3);
        public static readonly IfcAssemblyScope Ifc4 = IfcAssemblyScope.FromFactoryType<Xbim.Ifc4.EntityFactoryIfc4>(XbimSchemaVersion.Ifc4);
        public static readonly IfcAssemblyScope Ifc4x1 = IfcAssemblyScope.FromFactoryType<Xbim.Ifc4.EntityFactoryIfc4x1>(XbimSchemaVersion.Ifc4x1);

        public static readonly IDictionary<XbimSchemaVersion, IfcAssemblyScope> SchemaAssemblyScope;

        public readonly IEntityFactory factory;
        public readonly ExpressMetaData metadata;
        public readonly Qualifier schemaQualifier;

        static IfcAssemblyScope()
        {
           SchemaAssemblyScope = new Dictionary<XbimSchemaVersion, IfcAssemblyScope>() {
               { XbimSchemaVersion.Ifc2X3, Ifc2x3 },
               { XbimSchemaVersion.Ifc4, Ifc4 },
               { XbimSchemaVersion.Ifc4x1, Ifc4x1 }
           };
        }

        public static IfcAssemblyScope FromFactoryType<TFactory>(XbimSchemaVersion schemaVersion) where TFactory : IEntityFactory, new()
        {
            return new IfcAssemblyScope(new TFactory(), schemaVersion.ToString().ToQualifier());
        }

        private IfcAssemblyScope(IEntityFactory entityFactory, Qualifier schema)
            : base(entityFactory.GetType().Assembly)
        {
            factory = entityFactory;
            metadata = ExpressMetaData.GetMetadata(entityFactory.GetType().Module);
            schemaQualifier = schema;
        }

        public bool IsIfcEntityType(Type t)
        {
            return metadata.ExpressType(t) != null;
        }

        public override Qualifier GetModuleQualifer(Module module)
        {
            return schemaQualifier;
        }
    }
}
