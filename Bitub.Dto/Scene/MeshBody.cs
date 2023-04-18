using System.Collections.Generic;

namespace Bitub.Dto.Scene
{
    public partial class MeshBody
    {
        public IEnumerable<Facet> ToFacets(PtArray ptArray)
        {
            return Tess.ToFacets(new PtOffsetArray(ptArray));
        }

        public IEnumerable<Facet> ToFacets(PtOffsetArray ptArray)
        {
            return Tess.ToFacets(ptArray);
        }
    }
}
