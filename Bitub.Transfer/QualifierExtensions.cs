namespace Bitub.Transfer
{
    public static class QualifierExtensions
    {
        public static bool IsSuperQualifierOf(this Qualifier supQualifier, Qualifier subQualifier, bool caseInvariant = false)
        {
            if (supQualifier.GuidOrNameCase != supQualifier.GuidOrNameCase)
                return false;
            if (supQualifier.GuidOrNameCase != Qualifier.GuidOrNameOneofCase.Named)
                return false;

            if (supQualifier.Named.Frags.Count >= subQualifier.Named.Frags.Count)
                return false;

            for (int i=0; i<supQualifier.Named.Frags.Count; i++)
            {
                var supFrag = supQualifier.Named.Frags[i];
                var subFrag = subQualifier.Named.Frags[i];
                if(caseInvariant)
                {
                    supFrag = supFrag.ToUpperInvariant();
                    subFrag = subFrag.ToUpperInvariant();
                }

                if (!supFrag.Equals(subFrag))
                    return false;
            }

            return true;
        }

        public static string ToLabel(this Qualifier c, string separator = ".", int fromStart = 0, int fromEnd = 0)
        {
            switch (c.GuidOrNameCase)
            {
                case Qualifier.GuidOrNameOneofCase.Anonymous:
                    switch(c.Anonymous.NumericalOrStringCase)
                    {
                        case GlobalUniqueId.NumericalOrStringOneofCase.Guid:
                            return new System.Guid(c.Anonymous.Guid.Raw.ToByteArray()).ToString();
                        case GlobalUniqueId.NumericalOrStringOneofCase.Base64:
                            return c.Anonymous.ToBase64String();
                    }
                    return null;
                case Qualifier.GuidOrNameOneofCase.Named:
                    return c.Named.ToLabel(separator, fromStart, fromEnd);
                default:
                    return null;
            }
        }

    }
}
