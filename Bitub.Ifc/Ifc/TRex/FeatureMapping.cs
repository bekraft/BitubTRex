using System;
using System.Collections.Generic;

using Bitub.Dto;
using Bitub.Dto.Concept;

namespace Bitub.Ifc.TRex
{
    public sealed class FeatureMapping
    {
        public FeatureMapping()
        { }

        public Qualifier FeatureSource { get; set; }

        public Qualifier FeatureTarget { get; set; }

        public FeatureStageStrategy StageStrategy { get; set; } = FeatureStageStrategy.LastOf;

        public FeatureStageRange StageRange { get; set; } = FeatureStageRange.GlobalRange;
    }

    public sealed class FeatureEntityMapping
    {
        public CanonicalFilterRule EntityTypeScope { get; set; }
        public IDictionary<Qualifier, FeatureConcept> Feature { get; private set; }

        public FeatureEntityMapping(StringComparison stringComparison)
        {
            Feature = new Dictionary<Qualifier, FeatureConcept>(new QualifierCaseEqualityComparer(stringComparison));
        }
    }
}