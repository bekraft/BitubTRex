using System;
using System.Collections.Generic;

namespace Bitub.Dto.Scene
{
    public sealed class MeshPtOffsetArray : IEquatable<MeshPtOffsetArray>
    {
        public readonly Mesh Mesh;
        public readonly PtOffsetArray PtOffsetArray;

        public MeshPtOffsetArray(Mesh mesh, PtOffsetArray offsetArray)
        {
            Mesh = mesh;
            PtOffsetArray = offsetArray;
        }

        public bool Equals(MeshPtOffsetArray other)
        {
            if (null == other)
                return false;

            return other.Mesh.Equals(Mesh) && other.PtOffsetArray.Equals(PtOffsetArray);
        }

        public IEnumerable<Facet> ToFacets()
        {
            return Mesh.ToFacets(PtOffsetArray);
        }
    }
}
