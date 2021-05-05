using System;
using System.Collections.Generic;
using System.Linq;

namespace Bitub.Dto.Scene
{
    public partial class Mesh
    {
        /// <summary>
        /// Creates an enumerable of facets referring to a single mesh and point array.
        /// </summary>
        /// <param name="offsetArray"></param>
        /// <returns></returns>
        public IEnumerable<Facet> ToFacets(PtOffsetArray offsetArray)
        {
            var offsetMesh = new MeshPtOffsetArray(this, offsetArray);
            return Enumerable.Range(0, FacetCount).Select(index => new Facet(offsetMesh, index));
        }

        /// <summary>
        /// Returns the facet count of the given mesh (depending on the type).
        /// </summary>
        /// <returns>The count of facets</returns>
        public int FacetCount
        {
            get {
                switch (Type)
                {
                    case FacetType.QuadMesh:
                        return Vertex.Count / 4;
                    case FacetType.TriMesh:
                        return Vertex.Count / 3;
                    case FacetType.TriFan:
                    case FacetType.TriStripe:
                        // having an edge in common
                        return Vertex.Count - 2;
                    default:
                        throw new NotImplementedException($"Missing implementation for '{Type}'");
                }
            }
        }

        /// <summary>
        /// Returns a new mesh with shifted indexes.
        /// </summary>
        /// <param name="offset">The delta offset to add to all indexes.</param>
        /// <returns>A new mesh</returns>
        public Mesh ShiftMeshOffset(uint offset)
        {
            var newMesh = new Mesh { Type = this.Type };
            newMesh.Vertex.AddRange(this.Vertex.Select(i => i + offset));
            newMesh.Uv.AddRange(this.Uv);
            newMesh.Normal.AddRange(this.Normal);
            return newMesh;
        }
    }
}
