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
                && concept.Subsumes.All(q => !q.IsEmpty());
        }

        public static bool IsValid(this Feature feature)
        {
            if (feature.Name?.IsEmpty() ?? true)
                return false;

            switch (feature.FeatureCase)
            {
                case Feature.FeatureOneofCase.None:
                    return false;
                case Feature.FeatureOneofCase.Data:
                    return feature.Data.IsValid();
                case Feature.FeatureOneofCase.Role:
                    return feature.Role.IsValid();
                default:
                    throw new NotImplementedException();
            }
        }

        public static bool IsValid(this FeatureRole FeatureRole)
        {
            return !FeatureRole.Qualifier.IsEmpty();
        }

        public static bool IsValid(this FeatureData FeatureData)
        {
            return FeatureData.DataValueCase != FeatureData.DataValueOneofCase.None && FeatureData.ToAnyValue() != null;
        }

        public static object ToAnyValue(this FeatureData FeatureData)
        {
            switch (FeatureData.DataValueCase)
            {
                case FeatureData.DataValueOneofCase.None:
                    return null;
                case FeatureData.DataValueOneofCase.Digit:
                    return FeatureData.Digit;
                case FeatureData.DataValueOneofCase.Value:
                    return FeatureData.Value;
                case FeatureData.DataValueOneofCase.TimeStamp:
                    return FeatureData.TimeStamp.ToDateTime();
                case FeatureData.DataValueOneofCase.Logical:
                    return FeatureData.Logical.ToBoolean();
                case FeatureData.DataValueOneofCase.Guid:
                    switch (FeatureData.Guid.GuidOrStringCase)
                    {
                        case GlobalUniqueId.GuidOrStringOneofCase.Guid:
                            return FeatureData.Guid.Guid.ToGuid();
                        case GlobalUniqueId.GuidOrStringOneofCase.Base64:
                            return FeatureData.Guid.Base64;
                    }
                    return null;
                default:
                    throw new NotImplementedException($"Missing implementation for '{FeatureData.DataValueCase}'");
            }
        }
    }
}
