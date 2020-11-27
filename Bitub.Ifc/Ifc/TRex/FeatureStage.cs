using System;

using Bitub.Dto.Concept;

namespace Bitub.Ifc.TRex
{
    public class FeatureStage : IEquatable<FeatureStage>, IComparable<FeatureStage>
    {
        public readonly FeatureConcept feature;
        public readonly int stage;

        public FeatureStage(int s, FeatureConcept f)
        {
            feature = f;
            stage = s;
        }

        public int CompareTo(FeatureStage other)
        {
            return Math.Sign(stage - other.stage);
        }

        public override bool Equals(object obj)
        {
            if (obj is FeatureStage fs)
                return Equals(fs);
            else
                return false;
        }

        public bool Equals(FeatureStage other)
        {
            return feature.Canonical.Equals(other?.feature?.Canonical);
        }
    }

}
