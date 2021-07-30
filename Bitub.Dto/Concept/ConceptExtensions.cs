using System;
using System.Linq;
using System.Collections.Generic;

namespace Bitub.Dto.Concept
{
    public static class ConceptExtensions
    {
        public static Domain ToDomain(this IEnumerable<Classifier> ontologyClassification, 
            StringComparison stringComparison = StringComparison.OrdinalIgnoreCase, Qualifier rootCanonical = null)
        {
            var canonicalCache = new Dictionary<Qualifier, FeatureConcept>(new QualifierCaseEqualityComparer(stringComparison));
            foreach (var classifier in ontologyClassification)
            {
                FeatureConcept parent = null;
                foreach (var canonical in classifier.Path)
                {
                    FeatureConcept concept = null;
                    if (!canonicalCache.TryGetValue(canonical, out concept))
                    {
                        canonicalCache.Add(canonical, concept = new FeatureConcept { Canonical = canonical });
                    }

                    if (null != parent)
                        concept.Superior.Add(parent.Canonical);                    
                    
                    parent = concept;
                }
            }

            var roots = canonicalCache.Values.Where(c => c.Superior.Count == 0).ToArray();
            if (null == rootCanonical)
            {
                if (roots.Length == 1)
                {
                    rootCanonical = roots[0].Canonical;
                }
                else 
                {
                    rootCanonical = System.Guid.NewGuid().ToQualifier();                    
                }
            }

            var conceptSpace = new Domain { Canonical = rootCanonical };
            conceptSpace.Concepts.AddRange(canonicalCache.Values);
            return conceptSpace;
        }
    }
}
