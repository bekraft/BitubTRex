using System;
using System.Linq;

namespace Bitub.Dto.Concept
{
    public static class FeatureExtensions
    {
        public static bool IsValid(this FeatureConcept featureConcept)
        {
            if (featureConcept.Canonical?.IsEmpty() ?? true)
                return false;

            switch (featureConcept.DataOrRoleCase)
            {
                case FeatureConcept.DataOrRoleOneofCase.None:
                    return false;
                case FeatureConcept.DataOrRoleOneofCase.DataConcept:
                    // All data features of the same type
                    var allOfSameType = featureConcept.DataConcept.Data.Select(data => data.DataValueCase).Distinct().Count() == 1;
                    // No data feature quantifiers mentioned twice if all have to match
                    var distinctTypes = featureConcept.Op == ConceptOp.OneOf
                            || featureConcept.DataConcept.Data.Count == featureConcept.DataConcept.Data.Select(data => data.Type).Distinct().Count();
                    return distinctTypes && allOfSameType;
                case FeatureConcept.DataOrRoleOneofCase.RoleConcept:
                    // No role filler mentioned twice
                    var distinctFillers = featureConcept.RoleConcept.Filler.Distinct().Count() == featureConcept.RoleConcept.Filler.Count;
                    return distinctFillers;
                default:                    
                    throw new NotImplementedException();
            }
        }

        public static bool Subsumes(this FeatureConcept otherFeature)
        {
            throw new NotImplementedException();   
        }

        public static object ToAnyValue(this DataConcept dataConcept)
        {
            switch (dataConcept.DataValueCase)
            {
                case DataConcept.DataValueOneofCase.None:
                    return null;
                case DataConcept.DataValueOneofCase.Digit:
                    return dataConcept.Digit;
                case DataConcept.DataValueOneofCase.Value:
                    return dataConcept.Value;
                case DataConcept.DataValueOneofCase.TimeStamp:
                    return dataConcept.TimeStamp.ToDateTime();
                case DataConcept.DataValueOneofCase.Logical:
                    return dataConcept.Logical.ToBoolean();
                case DataConcept.DataValueOneofCase.Guid:
                    switch (dataConcept.Guid.GuidOrStringCase)
                    {
                        case GlobalUniqueId.GuidOrStringOneofCase.Guid:
                            return dataConcept.Guid.Guid.ToGuid();
                        case GlobalUniqueId.GuidOrStringOneofCase.Base64:
                            return dataConcept.Guid.Base64;
                    }
                    return null;
                default:
                    throw new NotImplementedException($"Missing implementation for '{dataConcept.DataValueCase}'");
            }
        }
    }
}
