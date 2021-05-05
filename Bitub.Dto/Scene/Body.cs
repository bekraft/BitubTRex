using System;
using System.Collections.Generic;
using System.Linq;

namespace Bitub.Dto.Scene
{
    public partial class Body
    {
        public IEnumerable<Facet> GetFacets(IEnumerable<PtArray> ptArrayContainer)
        {
            return GetFacets(ptArrayContainer.ToDictionary(pta => pta.Id, pta => new PtOffsetArray(pta)));
        }

        internal IEnumerable<Facet> GetFacets(IDictionary<RefId, PtOffsetArray> ptArrayMap)
        {
            switch (BodySelectCase)
            {
                case Body.BodySelectOneofCase.FaceBody:
                    return FaceBody
                        .Faces
                        .SelectMany(f => f.Mesh.ToFacets(ptArrayMap[FaceBody.Pts]));
                case Body.BodySelectOneofCase.MeshBody:
                    return MeshBody.Tess.ToFacets(ptArrayMap[MeshBody.Pts]);
                case Body.BodySelectOneofCase.WireBody:
                    throw new NotSupportedException("Wires are not supported for facet derivation");
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
