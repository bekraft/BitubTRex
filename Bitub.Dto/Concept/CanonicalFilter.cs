using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

using Bitub.Dto.Xml;

namespace Bitub.Dto.Concept
{
    public enum MatchingType
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
        /// In extension to <see cref="Sub"/>, additional match classifiers of <see cref="Equiv"/>.
        /// </summary>
        SubOrEquiv,
        /// <summary>
        /// If the given specimen is a super concept of at least one filter classifier
        /// </summary>
        Super,
        /// <summary>
        /// In extension to <see cref="Super"/>, additional match classifiers of <see cref="Equiv"/>.
        /// </summary>
        SuperOrEquiv,
        /// <summary>
        /// If the given specimen is an equivalent (case sensitive) match of at least one filter classifier.
        /// </summary>
        Equiv,
        /// <summary>
        /// Using regular expression to match.
        /// </summary>
        Regex
    }

    /// <summary>
    /// Classifying filter given a sequence of filter classifiers, a matching type and name comparison type.
    /// </summary>
    public class Matcher
    {
        public StringComparison StringComparison { get; private set; }

        public Matcher(MatchingType matchingType, StringComparison stringComparison)
        {
            MatchingType = matchingType;
        }

        public MatchingType MatchingType { get; private set; }

        public List<Classifier> Filter { get; set; } = new List<Classifier>();

        /// <summary>
        /// Tests classifier againts filter.
        /// </summary>
        /// <param name="specimen">The classifier specimen</param>
        /// <param name="matches">A out list of matches of the filter</param>
        /// <returns>A nullable flag indicating a pass-through, or an implicit pass-through if filter is empty</returns>
        public bool? IsPassedBy(Classifier specimen, out Classifier[] matches)
        {
            if (Filter?.Count == 0)
            {
                matches = new Classifier[0];
                return null;
            }
            else
            {
                switch (MatchingType)
                {
                    case MatchingType.Exists:
                        // Means sub or super in terms of classifier                    
                        matches = Filter.Where(c => c.IsMatching(specimen, StringComparison)).ToArray();
                        break;
                    case MatchingType.Sub:
                        matches = Filter.Where(c => c.IsSuperClassifierOf(specimen, true, StringComparison)).ToArray();
                        break;
                    case MatchingType.SubOrEquiv:
                        matches = Filter.Where(c => c.IsSuperClassifierOf(specimen, false, StringComparison)).ToArray();
                        break;
                    case MatchingType.Super:
                        matches = Filter.Where(c => specimen.IsSuperClassifierOf(c, true, StringComparison)).ToArray();
                        break;
                    case MatchingType.SuperOrEquiv:
                        matches = Filter.Where(c => specimen.IsSuperClassifierOf(c, false, StringComparison)).ToArray();
                        break;
                    case MatchingType.Equiv:
                        matches = Filter.Where(c => c.IsEquivTo(specimen)).ToArray();
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }

            return matches.Length > 0;
        }

        /// <summary>
        /// With true value, if the given specimen passes the filter. Otherwise false.
        /// </summary>
        /// <param name="singleSpecimen">The specimen to test against.</param>
        /// <param name="matches">A out list of matches of the filter</param>
        /// <returns>A nullable flag indicating a pass-through, or an implicit pass-through if filter is empty</returns>
        public bool? IsPassedBy(Qualifier singleSpecimen, out Classifier[] matches)
        {
            if (Filter?.Count == 0)
            {
                matches = new Classifier[0];
                return null;
            }
            else
            {
                switch (MatchingType)
                {
                    case MatchingType.Exists:
                        matches = Filter.Where(c => c.IsMatching(singleSpecimen, StringComparison)).ToArray();
                        break;
                    case MatchingType.Sub:
                    case MatchingType.SubOrEquiv:
                        matches = Filter.Where(c => c.Path.Any(q => q.IsSuperQualifierOf(singleSpecimen))).ToArray();
                        break;
                    case MatchingType.Super:
                    case MatchingType.SuperOrEquiv:
                        matches = Filter.Where(c => c.Path.Any(q => singleSpecimen.IsSuperQualifierOf(q))).ToArray();
                        break;
                    case MatchingType.Equiv:
                        matches = Filter.Where(c => c.IsEquivTo(singleSpecimen)).ToArray();
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }

            return matches.Length > 0;
        }
    }
}
