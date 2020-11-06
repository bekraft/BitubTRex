using System;
using System.Collections.Generic;
using System.Linq;

namespace Bitub.Dto
{
    public static class ClassifierExtensions
    {
        /// <summary>
        /// Counterpart of <see cref="IsNothing(Classifier)"/>
        /// </summary>
        /// <param name="classifier">The classifier</param>
        /// <returns>True if the classifier holds any path of qualifiers</returns>
        public static bool IsSomething(this Classifier classifier)
        {
            return !classifier.IsNothing();
        }

        /// <summary>
        /// True, if the classifier is empty.
        /// </summary>
        /// <param name="classifier">The classifier</param>
        /// <returns>True if empty</returns>
        public static bool IsNothing(this Classifier classifier)
        {
            return null == classifier || classifier.Path.Count == 0;
        }

        /// <summary>
        /// If a classifier contains only unique qualifiers.
        /// </summary>
        /// <param name="classifier">The classifier</param>
        /// <returns>True, if each path qualifier is unique (no cycles)</returns>
        public static bool IsValid(this Classifier classifier)
        {
            var set = new HashSet<Qualifier>(classifier?.Path);
            return null != classifier && set.Count == classifier.Path.Count;
        }

        /// <summary>
        /// True, if the given classifier is semantically equal to to the qualifier. Means: The only path node
        /// of the classifier is equal to the given qualifier.
        /// </summary>
        /// <param name="classifier">A classifier</param>
        /// <param name="other">A qualifier</param>
        /// <param name="comparisonType">Comparison type</param>
        /// <returns>True, if classifier's only qualifier is the same as given</returns>
        public static bool IsEquivTo(this Classifier classifier, Qualifier other, StringComparison comparisonType = StringComparison.Ordinal)
        {
            return 1 == classifier.Path.Count && classifier.Path[0].IsEqualTo(other, comparisonType);
        }

        /// <summary>
        /// True, if the given classifier is semantically equal to to the second classifier. Means: Both have the 
        /// same path length with the same sequence of qualifiers
        /// </summary>
        /// <param name="classifier">A classifier</param>
        /// <param name="other">Another classifier</param>
        /// <param name="comparisonType">Comparison type</param>
        /// <returns>True, if classifier's only qualifier is the same as given</returns>
        public static bool IsEquivTo(this Classifier classifier, Classifier other, StringComparison comparisonType = StringComparison.Ordinal)
        {
            if (classifier.Path.Count != other.Path.Count)
                return false;
            
            for (int i=0; i<classifier.Path.Count; i++)
            {
                if (!classifier.Path[i].IsEqualTo(other.Path[i], comparisonType))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// True, if the given qualifier is held by this classifier.
        /// </summary>
        /// <param name="classifier">The classifier</param>
        /// <param name="qualifier">The qualifier</param>
        /// <param name="comparisonType">The comparison in case of named qualifiers</param>
        /// <returns>True, if the qualifier is held by this classifier</returns>
        public static bool IsMatching(this Classifier classifier, Qualifier qualifier, StringComparison comparisonType = StringComparison.Ordinal)
        {
            return classifier?.Path.Any(q => q.IsEqualTo(qualifier, comparisonType)) ?? false;
        }
        
        /// <summary>
        /// Test the classifier whether it is a sub or super
        /// </summary>
        /// <param name="classifier"></param>
        /// <param name="other"></param>
        /// <param name="comparisonType"></param>
        /// <returns></returns>
        public static bool IsMatching(this Classifier classifier, Classifier other, StringComparison comparisonType = StringComparison.Ordinal)
        {
            // Consequitely exclusion by edge cases
            if (classifier.Path.Count == 0 || other.Path.Count == 0)
                return false;
            else if (other.Path.Count > classifier.Path.Count)
                return false;
            else if (other.Path.Count == classifier.Path.Count)
                return IsEquivTo(classifier, other, comparisonType);

            // Test most expensive case, other path length is less than context classifier
            int j = 0;
            for (int i=0; i<classifier.Path.Count; i++)
            {
                if (j < other.Path.Count)
                {
                    if (classifier.Path[i].IsEqualTo(other.Path[j], comparisonType))
                        // Scan for next path fragment
                        j++;
                    else if (j > 0)
                        // If already found first => fails
                        return false;
                }
                else
                {
                    break;
                }
            }
            // Only true, if other completely matches
            return j == other.Path.Count;
        }

        /// <summary>
        /// The grad of classifier concept.
        /// </summary>
        /// <param name="classifier">A classifier</param>
        /// <returns>Some value greater or equal than 0</returns>
        public static int Grad(this Classifier classifier) => classifier.Path.Count;

        /// <summary>
        /// Tests whether the classifier in context is a super classifier of the given sub classifier.
        /// </summary>
        /// <param name="supClassifier">An suggested super classifier</param>
        /// <param name="strictSuper">A flag indicating whether to include equiv case or strict super, too</param>
        /// <param name="subClassifier">An expected sub classifier</param>
        /// <param name="comparisonType">A method of name comparision</param>
        /// <returns>True, if assumption holds</returns>
        public static bool IsSuperClassifierOf(this Classifier supClassifier, Classifier subClassifier, bool strictSuper, StringComparison comparisonType = StringComparison.Ordinal)
        {
            // Consequitely exclusion by edge cases
            if (supClassifier.Path.Count == 0 || subClassifier.Path.Count == 0)
                return false;
            else if (subClassifier.Path.Count < supClassifier.Path.Count)
                return false;

            // Test most expensive case, supClassifier path length is less than sub classifier's
            for (int i = 0; i < supClassifier.Path.Count; i++)
            {
                if (!supClassifier.Path[i].IsEqualTo(subClassifier.Path[i], comparisonType))
                    return false;
            }
            // Only true, if super classifier completely matches
            return !strictSuper || (supClassifier.Path.Count < subClassifier.Path.Count);
        }

        /// <summary>
        /// True, if the given qualifier is a super qualifier of at least on path fragment.
        /// </summary>
        /// <param name="classifier">The classifier which is a sub set of the qualifier</param>
        /// <param name="superQualifier">The qualifier</param>
        /// <param name="comparison">The string comparision method</param>
        /// <returns>True, if the given qualifier is (paritally) a super qualifier of the classifier</returns>
        public static bool IsSubMatching(this Classifier classifier, Qualifier superQualifier, StringComparison comparisonType = StringComparison.Ordinal)
        {
            switch(superQualifier.GuidOrNameCase)
            {
                case Qualifier.GuidOrNameOneofCase.Anonymous:
                    // Same as matching if using GUIDs
                    return classifier.IsMatching(superQualifier);
                default:
                    return classifier.Path.Any(q => superQualifier.IsSuperQualifierOf(q, comparisonType));
            }
        }

        /// <summary>
        /// Filters the classifier for sub matches of given super qualifier.
        /// </summary>
        /// <param name="classifier"></param>
        /// <param name="superQualifier"></param>
        /// <param name="comparison"></param>
        /// <returns>An enumerable of the qualifiers</returns>
        public static IEnumerable<Qualifier> FilterSubMatching(this Classifier classifier, Qualifier superQualifier, StringComparison comparison = StringComparison.Ordinal)
        {
            switch (superQualifier.GuidOrNameCase)
            {
                case Qualifier.GuidOrNameOneofCase.Anonymous:
                    // Same as matching if using GUIDs
                    return classifier.IsMatching(superQualifier) ? new Qualifier[] { superQualifier } : Enumerable.Empty<Qualifier>();
                default:
                    return classifier.Path.Where(q => superQualifier.IsSuperQualifierOf(q, comparison));
            }
        }

        /// <summary>
        /// Extracts a filtered enumeration sub qualifiers which hava a common root with given qualifier.
        /// </summary>
        /// <param name="classifier">The classifier</param>
        /// <param name="qualifier">The qualifier</param>
        /// <param name="comparison">The string comparision method</param>
        /// <returns></returns>
        public static IEnumerable<Qualifier> ToSubQualifiers(this Classifier classifier, Qualifier qualifier, StringComparison comparison = StringComparison.Ordinal)
        {
            switch (qualifier.GuidOrNameCase)
            {
                case Qualifier.GuidOrNameOneofCase.Anonymous:
                    return classifier.Path.Where(q => q.Equals(qualifier));
                default:
                    return classifier.Path.Select(q => q.ToSubQualifierOf(qualifier, comparison)).Where(q => !q.IsEmpty());
            }
        }

        /// <summary>
        /// Fetches the superset classifier if there's any.
        /// </summary>
        /// <param name="classifier">The classifier</param>
        /// <param name="qualifier">The qualifier</param>
        /// <returns>A new super classifier trace or a classifier with <see cref="IsNothing(Classifier)"/> returning true.</returns>
        public static Classifier ToSuperClassifierOf(this Classifier classifier, Qualifier qualifier)
        {
            var superClassifier = new Classifier();
            for (int i=0; i < classifier.Path.IndexOf(qualifier); i++)
            {
                superClassifier.Path.Add(classifier.Path[i]);
            }
            return superClassifier;
        }

        /// <summary>
        /// Fetches the subset classifier if there's any.
        /// </summary>
        /// <param name="classifier">The classifier</param>
        /// <param name="qualifier">The qualifier</param>
        /// <returns>A new subset classifier trace or a classifier with <see cref="IsNothing(Classifier)"/> returning true.</returns>
        public static Classifier ToSubClassifierOf(this Classifier classifier, Qualifier qualifier)
        {
            var subClassifier = new Classifier();
            for (int i = classifier.Path.IndexOf(qualifier); i > 0 && i < classifier.Path.Count; i++)
            {
                subClassifier.Path.Add(classifier.Path[i]);
            }
            return subClassifier;
        }
    }
}
