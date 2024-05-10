using System;
using System.Linq;

namespace Bitub.Dto.Concept
{
    public static class ELConceptExtensions
    {
        public static bool IsValid(this ELConcept concept)
        {
            if (concept.Canonical?.IsEmpty() ?? true)
                return false;

            return concept.Feature.All(f => f.IsValid()) 
                && concept.Equivalent.All(q => !q.IsEmpty()) 
                && concept.Subsumes.All(q => !q.IsEmpty());
        }

        public static bool IsValid(this ELFeature feature)
        {
            if (feature.Name?.IsEmpty() ?? true)
                return false;

            switch (feature.FeatureCase)
            {
                case ELFeature.FeatureOneofCase.None:
                    return false;
                case ELFeature.FeatureOneofCase.Data:
                    return feature.Data.IsValid();
                case ELFeature.FeatureOneofCase.Role:
                    return feature.Role.IsValid();
                default:
                    throw new NotImplementedException();
            }
        }

        public static bool IsValid(this RoleConcept roleConcept)
        {
            return !roleConcept.Qualifier.IsEmpty();
        }

        public static bool IsValid(this DataConcept dataConcept)
        {
            return dataConcept.DataValueCase != DataConcept.DataValueOneofCase.None && dataConcept.ToAnyValue() != null;
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
