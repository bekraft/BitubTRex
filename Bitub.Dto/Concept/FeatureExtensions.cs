using System;
using System.Linq;

namespace Bitub.Dto.Concept
{
    public static class FeatureExtensions
    {
        public static bool IsValid(this FeatureConcept concept)
        {
            if (concept.Canonical?.IsEmpty() ?? true)
                return false;

            switch (concept.CompositeCase)
            {
                case FeatureConcept.CompositeOneofCase.None:
                    // Simple named concept
                    return true;
                case FeatureConcept.CompositeOneofCase.DataFeature:
                    // All data features of the same type
                    var definedType = concept.DataFeature.DataValueCase != DataConcept.DataValueOneofCase.None;
                    return definedType && concept.DataFeature.ToAnyValue() != null;
                case FeatureConcept.CompositeOneofCase.RoleFeature:
                    // Existing filler
                    return !concept.RoleFeature.IsEmpty();
                default:                    
                    throw new NotImplementedException();
            }
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
