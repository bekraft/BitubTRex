using System;
using System.Collections.Generic;
using System.Linq;

namespace Bitub.Dto.Scene
{
    public enum MeshManifoldResult
    {
        Closed, Open, OpenNonmanifold
    }

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
        public int FacetCount => Type switch
        {
            FacetType.QuadMesh => Vertex.Count / 4,
            FacetType.TriMesh => Vertex.Count / 3,
            FacetType.TriFan =>
                // having an edge in common
                Vertex.Count - 2,
            FacetType.TriStripe =>
                // having an edge in common
                Vertex.Count - 2,
            _ => throw new NotImplementedException($"Missing implementation for '{Type}'")
        };

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

        public MeshManifoldResult DecomposeManifold(PtOffsetArray ptArray, out List<Body> components)
        {
            var hull = new Dictionary<uint, List<Arc<uint>>>();
            components = new List<Body>();
            var queue = new Queue<Arc<uint>>();

            // First index all facets
            foreach (var f in ToFacets(ptArray))
            {
                foreach (var b in f.Loop)
                {
                    List<Arc<uint>> star;
                    if (!hull.TryGetValue(b.Origin, out star))
                    {
                        star = new List<Arc<uint>>();
                        hull[b.Origin] = star;
                    }

                    if (star.Contains(b))
                    {
                        queue.Enqueue(b);
                    }
                    else
                    {
                        star.Add(b);
                    }
                }
            }


            throw new NotImplementedException();
        }

        /// <summary>
        /// Computes the simplex (tetrahedral) volume approximation. The result will be nearly exact,
        /// if the mesh is geometrically closed. If negative, the mesh orientation is inside out.
        /// </summary>
        /// <param name="ptArray">The embedding in 3D space.</param>
        /// <returns>An approximated signed volume in model units.</returns>
        public double GetSimplexVolume(PtOffsetArray ptArray)
        {
            // First index all facets
            double sumVolume = 0;
            foreach (var t in ToFacets(ptArray).SelectMany(x => x.Triangles))
            {
                
            }
            return sumVolume;
        }

        /// <summary>
        /// Unifies the overall mesh orientation such that all facets have the same loop order.
        /// </summary>
        public void UnifyMeshOrientation()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Will invert the overall mesh orientation such that all loops are reorded reversely.
        /// </summary>
        public void InvertMeshOrientation()
        {
            throw new NotImplementedException();
        }
    }
}