using System;
using System.Collections.Generic;
using System.Linq;

using Xbim.Common;
using Xbim.Ifc4.Interfaces;
using Xbim.Ifc4.UtilityResource;

using Google.Protobuf.WellKnownTypes;

using Bitub.Dto;
using Bitub.Dto.Concept;

namespace Bitub.Ifc.Concept
{
    public static class IfcConceptExtensions
    {
        #region DataConcept conversion

        public static DataConcept ToIdFeature(this IIfcRoot o)
        {
            return o.GlobalId.ToDataConcept(DataOp.Equals);
        }

        public static DataConcept ToD(this IIfcRoot o)
        {
            string objName = o.Name ?? "Anonymous";
            return new DataConcept { Value = objName, Type = DataType.Label, Op = DataOp.Equals };
        }

        public static DataConcept ToDataConcept(this IfcGloballyUniqueId guid)
        {
            return new DataConcept { Type = DataType.Guid, Guid = guid.ToGlobalUniqueId() };
        }

        public static DataConcept ToDataConcept(this Xbim.Ifc2x3.UtilityResource.IfcGloballyUniqueId guid)
        {
            return new DataConcept { Type = DataType.Guid, Guid = guid.ToGlobalUniqueId() };
        }

        public static DataConcept ToDataConcept(this Xbim.Ifc2x3.Interfaces.IIfcValue p, DataOp dataOp = DataOp.Equals)
        {
            return ToDataConcept(p as IExpressValueType, dataOp);
        }

        public static DataConcept ToDataConcept(this IIfcValue p, DataOp dataOp = DataOp.Equals)
        {
            return ToDataConcept(p as IExpressValueType, dataOp);
        }

        public static DataConcept ToDataConcept(this IExpressValueType p, DataOp dataOp = DataOp.Equals)
        {
            if (p is IExpressIntegerType ifcInteger)
                return new DataConcept { Type = DataType.Integer, Digit = ifcInteger.Value, Op = dataOp };
            else if (p is IExpressStringType ifcString)
                return new DataConcept { Type = DataType.Label, Value = ifcString.Value, Op = dataOp };
            else if (p is IExpressRealType ifcReal)
                return new DataConcept { Type = DataType.Decimal, Digit = ifcReal.Value, Op = dataOp };
            else if (p is IExpressNumberType ifcNumber)
                return new DataConcept { Type = DataType.Decimal, Digit = ifcNumber.Value, Op = dataOp };
            else if (p is IExpressLogicalType ifcLogical)
                return new DataConcept { Type = DataType.Logical, Logical = ifcLogical.Value.ToLogical(), Op = dataOp };
            else if (p is IExpressBooleanType ifcBoolean)
                return new DataConcept { Type = DataType.Boolean, Logical = new Logical { Known = ifcBoolean.Value }, Op = dataOp };
            else if (p is Xbim.Ifc4.MeasureResource.IfcIdentifier ifcIdentifier4)
                return new DataConcept { Type = DataType.Id, Value = (ifcIdentifier4 as IExpressStringType)?.Value };
            else if (p is Xbim.Ifc2x3.MeasureResource.IfcIdentifier ifcIdentifier2x3)
                return new DataConcept { Type = DataType.Id, Value = (ifcIdentifier2x3 as IExpressStringType)?.Value };
            else if (p is Xbim.Ifc4.DateTimeResource.IfcDateTime ifcDateTime4)
                return new DataConcept { Type = DataType.Timestamp, TimeStamp = Timestamp.FromDateTime(ifcDateTime4.ToDateTime()) };
            else if (p is Xbim.Ifc4.DateTimeResource.IfcTimeStamp ifcTimeStamp4)
                return new DataConcept { Type = DataType.Timestamp, TimeStamp = Timestamp.FromDateTime(ifcTimeStamp4.ToDateTime()) };
            else if (p is Xbim.Ifc2x3.MeasureResource.IfcTimeStamp ifcTimeStamp2x3)
                return new DataConcept { Type = DataType.Timestamp, TimeStamp = Timestamp.FromDateTime(Xbim.Ifc2x3.MeasureResource.IfcTimeStamp.ToDateTime(ifcTimeStamp2x3)) };
            throw new NotImplementedException($"Missing cast of '{p.GetType()}'");
        }

        /// <summary>
        /// Converts each property into one or more <see cref="DataConcept"/> instances.
        /// </summary>
        /// <param name="p">The IFC property.</param>
        /// <returns>Data concepts</returns>
        public static IEnumerable<DataConcept> ToDataConcept(this IIfcSimpleProperty p)
        {
            if (p is IIfcPropertySingleValue psv)
            {
                yield return psv.NominalValue.ToDataConcept();
            }
            else if (p is IIfcPropertyBoundedValue pbv)
            {
                if (null != pbv.UpperBoundValue)
                    yield return pbv.UpperBoundValue.ToDataConcept(DataOp.LessThanEquals);
                if (null != pbv.LowerBoundValue)
                    yield return pbv.LowerBoundValue.ToDataConcept(DataOp.GreaterThanEquals);
                if (null != pbv.SetPointValue)
                    yield return pbv.SetPointValue.ToDataConcept(DataOp.Equals);
            }
            else if (p is IIfcPropertyEnumeratedValue pev)
            {
                foreach (var dataConcept in pev.EnumerationValues.Select(v => v.ToDataConcept(DataOp.Equals)))
                    yield return dataConcept;
            }
            else if (p is IIfcPropertyListValue plv)
            {
                foreach (var dataConcept in plv.ListValues.Select(v => v.ToDataConcept(DataOp.Equals)))
                    yield return dataConcept;
            }
            else
            {
                throw new NotImplementedException($"Not yet implemented: {p.ExpressType.Name}");
            }
        }

        #endregion

        #region FeatureConcept conversion

        public static Qualifier ToCanonical(this IIfcRoot o)
        {
            return new string[] { o.Name ?? "Anonymous", o.GlobalId.ToString() }.ToQualifier();
        }

        public static IEnumerable<FeatureConcept> ToFeatureConcepts<T>(this IIfcObject o, CanonicalFilter filter = null) where T : IIfcSimpleProperty
        {
            // Pass by default, ignore match results
            return o.PropertySets<IIfcPropertySetDefinition>()
                .SelectMany(set => set.ToFeatureConcepts<T>())
                .Where(f => filter?.IsPassedBy(f.Canonical, out _) ?? true);
        }

        public static IEnumerable<FeatureConcept> ToFeatureConcepts<T>(this IIfcPropertySetDefinition set) where T : IIfcSimpleProperty
        {
            return set.Properties<T>().SelectMany(p => p.ToFeatureConcepts(set.Name.ToString().ToQualifier()));
        }

        public static IEnumerable<FeatureConcept> ToFeatureConcepts(this IIfcSimpleProperty p, Qualifier superCanonical)
        {
            var canonical = superCanonical.Append(p.Name.ToString());
            return p.ToDataConcept().Select(dataConcept => new FeatureConcept { Canonical = canonical, DataFeature = dataConcept });            
        }      

        /// <summary>
        /// Converts the whole object into an concept assertion wrapped by an concrete object concept.
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public static IEnumerable<FeatureConcept> ToFeatureConceptAssertion(this IIfcObject p)
        {
            var productConcept = new FeatureConcept { Canonical = p.ToCanonical() };
            yield return productConcept;
        }

        #endregion
    }
}
