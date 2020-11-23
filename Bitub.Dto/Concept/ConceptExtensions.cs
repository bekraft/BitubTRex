using System;
using System.Linq;
using System.Collections.Generic;

namespace Bitub.Dto.Concept
{
    public static class ConceptExtensions
    {
        public static ConceptSpace ToConceptSpace(this IEnumerable<Classifier> ontologyClassification, 
            StringComparison stringComparison = StringComparison.OrdinalIgnoreCase, Qualifier rootCanonical = null)
        {
            var canonicalCache = new Dictionary<Qualifier, Concept>(new QualifierCaseEqualityComparer(stringComparison));
            var inverseCache = new Dictionary<Qualifier, Qualifier>(new QualifierCaseEqualityComparer(stringComparison));
            foreach (var classifier in ontologyClassification)
            {
                Concept parent = null;
                foreach (var canonical in classifier.Path)
                {
                    Concept concept = null;
                    if (!canonicalCache.TryGetValue(canonical, out concept))
                    {
                        canonicalCache.Add(canonical, concept = new Concept { Canonical = canonical });
                    }

                    parent?.Subsumes.Add(concept.Canonical);
                    
                    inverseCache[canonical] = parent?.Canonical;
                    parent = concept;
                }
            }

            var roots = inverseCache.Where(g => null == g.Value).ToArray();
            if (null == rootCanonical)
            {
                if (roots.Length == 1)
                {
                    rootCanonical = roots[0].Key;
                }
                else 
                {
                    rootCanonical = System.Guid.NewGuid().ToQualifier();                    
                }
            }

            var conceptSpace = new ConceptSpace { Canonical = rootCanonical };
            conceptSpace.Concepts.AddRange(canonicalCache.Values);
            return conceptSpace;
        }

        public static object ToAnyValue(this Feature feature)
        {
            switch (feature.DataOrFillerCase)
            {
                case Feature.DataOrFillerOneofCase.Data:
                    return feature.Data.ToAnyValue();
                case Feature.DataOrFillerOneofCase.Filler:
                    return feature.Filler;
                default:
                    throw new NotImplementedException($"Missing implementation {feature.DataOrFillerCase}");
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
