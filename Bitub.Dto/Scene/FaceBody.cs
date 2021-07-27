using System;
using System.Collections.Generic;
using System.Linq;

namespace Bitub.Dto.Scene
{
    public partial class FaceBody
    {
        public IEnumerable<Facet> ToFacets(PtArray ptArray)
        {
            return Faces.SelectMany(f => f.Mesh.ToFacets(new PtOffsetArray(ptArray)));
        }

        public IEnumerable<Facet> ToFacets(PtOffsetArray ptArray)
        {
            return Faces.SelectMany(f => f.Mesh.ToFacets(ptArray));
        }
    }
}
