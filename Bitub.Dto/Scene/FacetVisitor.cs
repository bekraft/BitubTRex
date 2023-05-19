using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Bitub.Dto.Scene
{
    /// <summary>
    /// Simple straight-through facet enumerator returning each facet once.
    /// </summary>
    public class FacetVisitor : IFacetVisitor
    {
        public FaceBody[] BodyArray { get; private set; }

        public bool IsNewFace { get; private set; }

        public uint? PivotVertex { get => null; }

        public readonly PtOffsetArray PtOffsetArray;

        public FacetVisitor(PtOffsetArray ptOffsetArray, params FaceBody[] faceBodies)
        {
            PtOffsetArray = ptOffsetArray;
            BodyArray = faceBodies.Select(b => b).ToArray();
        }

        public IEnumerator<Facet> GetEnumerator()
        {
            foreach (var body in BodyArray)
            {
                MeshPtOffsetArray recentFace = null;
                foreach (var facet in body.Faces.SelectMany(f => new MeshPtOffsetArray(f.Mesh, PtOffsetArray).ToFacets()))
                {
                    IsNewFace = facet.meshed != recentFace;
                    recentFace = facet.meshed;                    
                    yield return facet;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
