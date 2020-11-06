using Microsoft.Extensions.Logging;

using System;
using System.Linq;
using System.Reflection;
using Xbim.Ifc;
using Xbim.Ifc4.GeometryResource;
using Xbim.Ifc4.Interfaces;
using Xbim.Ifc4.Kernel;
using Xbim.Ifc4.MeasureResource;
using Xbim.Ifc4.PresentationAppearanceResource;
using Xbim.Ifc4.RepresentationResource;
using Xbim.Ifc4.UtilityResource;

namespace Bitub.Ifc
{
    /// <summary>
    /// IFC 4 and 4x1 Builder Implementation
    /// </summary>
    public class Ifc4Builder : IfcBuilder
    {
        public Ifc4Builder(IfcStore aStore, ILoggerFactory loggerFactory = null) 
            : base(aStore, new AssemblyScope(typeof(Xbim.Ifc4.EntityFactoryIfc4).Assembly), assemblyXbimIfc4.GetName(), loggerFactory)
        {
        }

        protected override IIfcProject InitProject()
        {
            IfcProject project = store.Instances.OfType<IfcProject>().FirstOrDefault();
            if (null == project)
                project = store.Instances.New<IfcProject>();

            ChangeOrNewLengthUnit(IfcSIUnitName.METRE);
            if (null == project.ModelContext)
                project.RepresentationContexts.Add(store.NewIfc4GeometricContext("Body", "Model"));
            return project;
        }

        protected override IIfcOwnerHistory NewOwnerHistoryEntry(string comment)
        {
            return NewOwnerHistoryEntry(comment, IfcChangeActionEnum.ADDED);
        }

        private IfcOwnerHistory NewOwnerHistoryEntry(string version, IfcChangeActionEnum change)
        {
            IfcOwnerHistory newVersion = store.NewIfc4OwnerHistoryEntry(version, change);            
            return newVersion;
        }

        protected IfcSIUnit ChangeOrNewLengthUnit(IfcSIUnitName name, IfcSIPrefix? prefix = null)
        {
            var project = store.Instances.OfType<IfcProject>().First();
            var assigment = project.UnitsInContext;
            if (null == assigment)
                assigment = store.NewIfc4UnitAssignment(IfcUnitEnum.LENGTHUNIT, name, prefix);

            // Test for existing
            var unit = assigment.Units.Where(u => (u as IfcSIUnit)?.UnitType == IfcUnitEnum.LENGTHUNIT).FirstOrDefault() as IfcSIUnit;
            if (null == unit)
            {
                unit = store.Instances.New<IfcSIUnit>(u => { u.UnitType = IfcUnitEnum.LENGTHUNIT; });
                assigment.Units.Add(unit);
            }
                
            unit.Name = name;
            unit.Prefix = prefix;
            return unit;
        }

        public IfcShapeRepresentation NewGeometricRepresentation(IfcProduct product, 
                                                                 IfcGeometricRepresentationItem representationItem,
                                                                 IfcStyleAssignmentSelect style = null,
                                                                 string representationContext = "Model",
                                                                 string representationContextId = "Body")
        {
            if (store != product.Model || store != representationItem.Model)
                throw new ArgumentException("Model mismatch");

            IfcShapeRepresentation shapeRepresentation = null;
            Wrap(s =>
            {
                var productDefinitionShape = product.Representation;
                if (null == productDefinitionShape)
                    productDefinitionShape = s.Instances.New<IfcProductDefinitionShape>();

                if (null != style)
                {
                    s.Instances.New<IfcStyledItem>(i =>
                    {
                        i.Item = representationItem;
                        i.Styles.Add(style);
                    });
                }

                product.Representation = productDefinitionShape;

                shapeRepresentation = s.Instances.New<IfcShapeRepresentation>();
                productDefinitionShape.Representations.Add(shapeRepresentation);

                var project = Scopes.OfType<IfcProject>().FirstOrDefault();
                var contexts = project
                    .RepresentationContexts
                    .Where<IfcGeometricRepresentationContext>(c => c.ContextType == representationContext);

                IfcGeometricRepresentationContext context = null;
                if (contexts.Count() > 1)
                    context = contexts.Where(c => c.ContextIdentifier == representationContextId).FirstOrDefault();
                else
                    context = contexts.FirstOrDefault();

                if (null == context)
                    context = store.NewIfc4GeometricContext(representationContextId, representationContext);

                shapeRepresentation.ContextOfItems = context;
                shapeRepresentation.Items.Add(representationItem);
            });

            return shapeRepresentation;
        }
    }
}