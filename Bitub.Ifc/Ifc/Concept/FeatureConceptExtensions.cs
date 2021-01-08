using System;
using System.Collections.Generic;
using System.Linq;

using Bitub.Dto;
using Bitub.Dto.Concept;

using Xbim.Common;

using Google.Protobuf.WellKnownTypes;

using Xbim.Ifc4.Interfaces;

namespace Bitub.Ifc.Concept
{
    public static class FeatureConceptExtensions
    {
        public static Qualifier ToCanonical(this IIfcRoot o)
        {
            return new string[] { o.Name ?? "Anonymous", o.GlobalId.ToString() }.ToQualifier();
        }

        public static FeatureConcept AppendRoleFeature(this FeatureConcept featureConcept, params Qualifier[] fillers)
        {
            switch (featureConcept.DataOrRoleCase)
            {
                case FeatureConcept.DataOrRoleOneofCase.RoleConcept:
                    featureConcept.RoleConcept.Filler.AddRange(fillers);
                    return featureConcept;
                case FeatureConcept.DataOrRoleOneofCase.DataConcept:
                    throw new NotSupportedException($"Use '{nameof(AppendDataFeature)}' instead.");
                case FeatureConcept.DataOrRoleOneofCase.None:
                    featureConcept.RoleConcept = new RoleFeature();
                    featureConcept.RoleConcept.Filler.AddRange(fillers);
                    return featureConcept;
            }
            throw new NotImplementedException();
        }

        public static FeatureConcept AppendDataFeature(this FeatureConcept featureConcept, params DataConcept[] dataConcepts)
        {
            switch (featureConcept.DataOrRoleCase)
            {
                case FeatureConcept.DataOrRoleOneofCase.DataConcept:
                    featureConcept.DataConcept.Data.AddRange(dataConcepts);
                    return featureConcept;
                case FeatureConcept.DataOrRoleOneofCase.RoleConcept:
                    throw new NotSupportedException($"Use '{nameof(AppendDataFeature)}' instead.");
                case FeatureConcept.DataOrRoleOneofCase.None:
                    featureConcept.DataConcept = new DataFeature();
                    featureConcept.DataConcept.Data.AddRange(dataConcepts);
                    return featureConcept;
            }
            throw new NotImplementedException();
        }

        public static IIfcProperty ToIfcProperty(this FeatureConcept featureConcept, IfcAssemblyScope assemblyScope)
        {
            throw new NotImplementedException();
        }

        public static Classifier ToClassifierOnValueEquals(this FeatureConcept featureConcept, string separator = "|")
        {
            var dataFragment = string.Join(separator, featureConcept.DataConcept?.Data
                .Where(f => f.Op == DataOp.Equals)
                .Select(f => f.ToAnyValue()?.ToString())
                .Where(s => null != s));

            return featureConcept.Canonical.Append(dataFragment).ToClassifier();
        }

    }
}
