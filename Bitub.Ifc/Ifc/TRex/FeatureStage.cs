using System;
using System.Collections.Generic;
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

        public override int GetHashCode()
        {
            int hashCode = -1497673742;
            hashCode = hashCode * -1521134295 + EqualityComparer<FeatureConcept>.Default.GetHashCode(feature);
            hashCode = hashCode * -1521134295 + stage.GetHashCode();
            return hashCode;
        }
    }

}
