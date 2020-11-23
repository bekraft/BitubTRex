using System;
using System.Collections.Generic;
using System.Linq;

using Bitub.Dto;
using Bitub.Dto.Concept;

using Xbim.Common;

using Google.Protobuf.WellKnownTypes;

using Xbim.Ifc4.Interfaces;

namespace Bitub.Ifc.Concept
{
    public static class FeatureConceptExtensions
    {
        public static readonly string FeatureQualifier_Id = "GlobalId";
        public static readonly string FeatureQualifier_Name = "Name";

        public static Qualifier ToCanonical(this IIfcRoot o)
        {
            return new string[] { o.Name ?? "Anonymous", o.GlobalId.ToString() }.ToQualifier();
        }

        public static IEnumerable<FeatureConcept> ToBaseFeatures(this IIfcObject o)
        {
            var id = o.ToIdFeature();
            var name = o.ToNameFeature();
            var baseCanonical = new[] { name.Data.Value, id.Data.Value }.ToQualifier();
            var idFeature = new FeatureConcept { Canonical = baseCanonical.Append(FeatureQualifier_Id) };
            idFeature.Features.Add(id);
            yield return idFeature;
            yield return new FeatureConcept { Canonical = baseCanonical.Append(FeatureQualifier_Name) };
        }

        public static IEnumerable<FeatureConcept> ToFeatures<T>(this IIfcObject o, CanonicalFilter filter = null) where T : IIfcSimpleProperty
        {
            // Pass by default, ignore match results
            return o.PropertySets<IIfcPropertySetDefinition>()
                .SelectMany(set => set.ToFeatures<T>())
                .Where(f => filter?.IsPassedBy(f.Canonical, out _) ?? true);
        }

        public static Feature ToIdFeature(this IIfcRoot o)
        {
            return new Feature { Data = o.GlobalId.ToDataConcept(DataOp.Equals) };
        }

        public static Feature ToNameFeature(this IIfcRoot o)
        {
            string objName = o.Name ?? "Anonymous";
            return new Feature { Data = new DataConcept { Value = objName, Type = DataType.Label, Op = DataOp.Equals } };
        }

        public static IEnumerable<FeatureConcept> ToFeatures<T>(this IIfcPropertySetDefinition set) where T : IIfcSimpleProperty
        {
            return set.Properties<T>().Select(p => p.ToFeature(set.Name));
        }

        public static FeatureConcept ToFeature(this IIfcSimpleProperty p, params string[] preQualifiers)
        {
            Qualifier q = preQualifiers.ToQualifier();
            q.Named.Frags.Add(p.Name.ToString());
            
            var f = new FeatureConcept { Canonical = q };
            if (p is IIfcPropertySingleValue psv)
            {
                f.Features.Add(new Feature { Data = psv.NominalValue.ToDataConcept() });
            }
            else if (p is IIfcPropertyBoundedValue pbv)
            {                
                f.Op = ConceptOp.AllOf;
                if (null != pbv.UpperBoundValue)
                    f.Features.Add(new Feature { Data = pbv.UpperBoundValue.ToDataConcept(DataOp.LessThanEquals) });
                if (null != pbv.LowerBoundValue)
                    f.Features.Add(new Feature { Data = pbv.LowerBoundValue.ToDataConcept(DataOp.GreaterThanEquals) });

                if (null != pbv.SetPointValue)
                    f.Features.Add(new Feature { Data = pbv.SetPointValue.ToDataConcept(DataOp.Equals) });
            }
            else if (p is IIfcPropertyEnumeratedValue pev)
            {
                f.Op = ConceptOp.AllOf;
                pev.EnumerationValues.Select(v => v.ToDataConcept(DataOp.Equals)).ForEach(c => f.Features.Add(new Feature { Data = c }));
            }
            else if (p is IIfcPropertyListValue plv)
            {
                f.Op = ConceptOp.AllOf;
                plv.ListValues.Select(v => v.ToDataConcept(DataOp.Equals)).ForEach(c => f.Features.Add(new Feature { Data = c }));
            }
            else 
            {
                throw new NotImplementedException($"Not yet implemented: {p.ExpressType.Name}");
            }

            return f;
        }

        public static DataConcept ToDataConcept(this Xbim.Ifc4.UtilityResource.IfcGloballyUniqueId guid)
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

        public static DataConcept ToDataConcept(this Xbim.Ifc4.Interfaces.IIfcValue p, DataOp dataOp = DataOp.Equals)
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
    }
}
