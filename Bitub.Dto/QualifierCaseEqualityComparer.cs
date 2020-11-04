using System;
using System.Collections.Generic;

namespace Bitub.Dto
{
    /// <summary>
    /// Qualifier case-sensitive equality comparer. Will apply the initially given <see cref="StringComparer"/> instance
    /// to check the equality of two named qualifiers. In case of anonymous qualification, any two will be equal, if
    /// both have the same type and ID.
    /// </summary>
    public class QualifierCaseEqualityComparer : IEqualityComparer<Qualifier>
    {
        public readonly StringComparison stringComparison;

        public QualifierCaseEqualityComparer(StringComparison stringComparison)
        {
            this.stringComparison = stringComparison;
        }

        public bool Equals(Qualifier x, Qualifier y)
        {
            return Equals(x, y, stringComparison);
        }

        public static bool Equals(Qualifier x, Qualifier y, StringComparison stringComparison)
        {
            if (!x.IsCompliantTo(y))
                return false;

            switch (x.GuidOrNameCase)
            {
                case Qualifier.GuidOrNameOneofCase.Anonymous:
                    return x.Equals(y); // Let protobuf decide
                case Qualifier.GuidOrNameOneofCase.Named:
                    if (x.Named.Frags.Count != y.Named.Frags.Count)
                        // If frags count are different => not equal
                        return false;
                    for (int i=0; i < x.Named.Frags.Count; i++)
                    {   // Test frag by frag
                        if (0 != string.Compare(x.Named.Frags[i], y.Named.Frags[i], stringComparison))
                            return false;
                    }
                    return true;
                case Qualifier.GuidOrNameOneofCase.None:
                    return true;
            }

            return false;
        }

        public int GetHashCode(Qualifier obj)
        {
            return obj.GetHashCode();
        }
    }
}
