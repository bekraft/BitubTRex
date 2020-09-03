using System;
using System.Collections.Generic;
using System.Linq;

using Bitub.Ifc.Transform;
using Bitub.Transfer;
using Bitub.Transfer.Classify;

using Xbim.Common;
using Xbim.Ifc4.Interfaces;

namespace Bitub.Ifc.Classify
{
    public static class XbimModelExtension
    {
        public static IEnumerable<Feature> ToFeatureSet<T>(this IIfcPropertySet set) where T : IIfcProperty
        {
            throw new NotImplementedException();
        }

        public static Feature ToFeature(this IIfcProperty p, string setNamespace)
        {
            var f = new Feature { Name = new string[] { setNamespace, p.Name.ToString() }.ToQualifier() };
            if (p is IIfcPropertySingleValue psv)
                ;
            else if (p is IIfcPropertyBoundedValue pbv)
                ;
            else if (p is IIfcPropertyEnumeratedValue pev)
                ;
            else if (p is IIfcPropertyListValue plv)
                ;
            else if (p is IIfcPropertyReferenceValue prv)
                ;
            else if (p is IIfcPropertyTableValue ptv)
                ;

            throw new NotImplementedException($"Missing implementation for '{p.GetType().Name}'");
        }

        public static DataConcept ToDataConcept(this IIfcValue v, DataOp dataOp = DataOp.Equals)
        {
            if (v.UnderlyingSystemType.IsPrimitive)
            {
                if (v.UnderlyingSystemType == typeof(double) || v.UnderlyingSystemType == typeof(float))
                    return new DataConcept { Op = dataOp, Digit = (double)v.Value, Type = DataType.Decimal };
                else if (v.UnderlyingSystemType == typeof(long) || v.UnderlyingSystemType == typeof(int))
                    return new DataConcept { Op = dataOp, Digit = (double)v.Value, Type = DataType.Integer };
                else if (v.UnderlyingSystemType == typeof(string))
                    return new DataConcept { Op = dataOp, Serialized = v.Value?.ToString(), Type = DataType.String };
            }

            throw new NotImplementedException($"Missing implementation for '{v.GetType().Name}'");
        }
    }
}
