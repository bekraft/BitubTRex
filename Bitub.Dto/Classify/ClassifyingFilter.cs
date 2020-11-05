using System;
using System.Collections.Generic;
using System.Linq;

namespace Bitub.Dto.Classify
{
    public enum FilterMatchingType
    {
        /// <summary>
        /// If given specimen is part of the filter at any path position of at least one filter classifier.
        /// </summary>
        Exists,
        /// <summary>
        /// If the given specimen is a sub concept of at least one filter classifier.
        /// </summary>
        Sub,
        /// <summary>
        /// If the given specimen is a super concept of at least one filter classifier
        /// </summary>
        Super,
        /// <summary>
        /// If the given specimen is an exact (case sensitive) match of at least one filter classifier.
        /// </summary>
        Exact
    }

    /// <summary>
    /// Classifying filter given a sequence of filter classifiers, a matching type and name comparison type.
    /// </summary>
    public class ClassifyingFilter
    {
        public readonly StringComparison stringComparison;

        public ClassifyingFilter(FilterMatchingType filterMatchingType, StringComparison stringComparison)
        {
            MatchingType = filterMatchingType;
        }

        public FilterMatchingType MatchingType { get; private set; }

        public List<Classifier> Filter { get; set; } = new List<Classifier>();

        public bool? IsPassedBy(Classifier specimen)
        {
            if (Filter?.Count == 0)
                return null;

            switch (MatchingType)
            {
                case FilterMatchingType.Exists:
                    // Means sub or super in terms of classifier
                    return Filter.Any(c => c.IsMatching(specimen, stringComparison));
                case FilterMatchingType.Sub:
                    return Filter.Any(c => c.IsSuperClassifierOf(specimen, stringComparison));
                case FilterMatchingType.Super:
                    return Filter.Any(c => specimen.IsSuperClassifierOf(c, stringComparison));
                case FilterMatchingType.Exact:
                    return Filter.Any(c => c.IsEquivTo(specimen));
                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// With true value, if the given specimen passes the filter. Otherwise false.
        /// </summary>
        /// <param name="singleSpecimen">The specimen to test against.</param>
        /// <returns>True, false or null in case of empty filter.</returns>
        public bool? IsPassedBy(Qualifier singleSpecimen)
        {
            if (Filter?.Count == 0)
                return null;

            switch (MatchingType)
            {
                case FilterMatchingType.Exists:
                    return Filter.Any(c => c.IsMatching(singleSpecimen, stringComparison));
                case FilterMatchingType.Sub:
                    return Filter.SelectMany(c => c.Path).Any(q => q.IsSuperQualifierOf(singleSpecimen));
                case FilterMatchingType.Super:
                    return Filter.SelectMany(c => c.Path).Any(q => singleSpecimen.IsSuperQualifierOf(q));
                case FilterMatchingType.Exact:
                    return Filter.Any(c => c.IsEquivTo(singleSpecimen));
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
