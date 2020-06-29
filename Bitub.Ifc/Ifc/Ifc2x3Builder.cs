using Xbim.Ifc4.Interfaces;
using Xbim.Ifc;

using Microsoft.Extensions.Logging;

using Xbim.Ifc2x3.UtilityResource;
using Xbim.Ifc2x3.Kernel;
using Xbim.Ifc2x3.MeasureResource;

using IfcChangeActionEnum = Xbim.Ifc2x3.UtilityResource.IfcChangeActionEnum;
using IfcSIUnitName = Xbim.Ifc2x3.MeasureResource.IfcSIUnitName;
using IfcSIPrefix = Xbim.Ifc2x3.MeasureResource.IfcSIPrefix;
using IfcUnitEnum = Xbim.Ifc2x3.MeasureResource.IfcUnitEnum;

using System.Linq;

namespace Bitub.Ifc
{
    /// <summary>
    /// IFC 2x3 Builder Implementation.
    /// </summary>
    public class Ifc2x3Builder : IfcBuilder
    {
        public Ifc2x3Builder(IfcStore aStore, ILoggerFactory loggerFactory = null)
            : base(aStore, new System.Reflection.Assembly[]{ typeof(Xbim.Ifc2x3.EntityFactoryIfc2x3).Assembly, typeof(Xbim.Ifc4.EntityFactoryIfc4).Assembly }, "Xbim.Ifc2x3", loggerFactory)
        {
        }

        protected override IIfcProject InitProject()
        {
            IfcProject project = Store.Instances.OfType<IfcProject>().FirstOrDefault();
            if (null == project)
                project = Store.Instances.New<IfcProject>();

            ChangeOrNewLengthUnit(IfcSIUnitName.METRE);
            if (null == project.ModelContext)
                project.RepresentationContexts.Add(Store.NewIfc2x3GeometricContext("Body", "Model"));
            return project;
        }

        protected override IIfcOwnerHistory NewOwnerHistoryEntry(string comment)
        {
            return NewOwnerHistoryEntry(comment, IfcChangeActionEnum.ADDED);
        }

        private IfcOwnerHistory NewOwnerHistoryEntry(string version, IfcChangeActionEnum change)
        {
            IfcOwnerHistory newVersion = Store.NewIfc2x3OwnerHistoryEntry(version, change);
            return newVersion;
        }

        protected IfcSIUnit ChangeOrNewLengthUnit(IfcSIUnitName name, IfcSIPrefix? prefix = null)
        {
            var project = Store.Instances.OfType<IfcProject>().First();
            var assigment = project.UnitsInContext;
            if (null == assigment)
                assigment = Store.NewIfc2x3UnitAssignment(IfcUnitEnum.LENGTHUNIT, name, prefix);

            // Test for existing
            var unit = assigment.Units.Where(u => (u as IfcSIUnit)?.UnitType == IfcUnitEnum.LENGTHUNIT).FirstOrDefault() as IfcSIUnit;
            if (null == unit)
            {
                unit = Store.Instances.New<IfcSIUnit>(u => { u.UnitType = IfcUnitEnum.LENGTHUNIT; });
                assigment.Units.Add(unit);
            }

            unit.Name = name;
            unit.Prefix = prefix;
            return unit;
        }

    }
}