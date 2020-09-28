using System;
using System.Collections.Generic;
using System.Linq;

using Bitub.Dto.Spatial;

namespace Bitub.Dto.Scene
{
    public enum CrsOrientation
    {
        RightHandedZUp,
        LeftHandedYUp
    }

    public static class SceneMeshExtension
    {
        /// <summary>
        /// Creates an enumerable of facets referring to a single mesh and point array.
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="offsetArray"></param>
        /// <returns></returns>
        public static IEnumerable<Facet> ToFacets(this Mesh mesh, PtOffsetArray offsetArray)
        {
            var offsetMesh = new MeshPtOffsetArray(mesh, offsetArray);
            return Enumerable.Range(0, mesh.FacetCount()).Select(index => new Facet(offsetMesh, index));
        }

        /// <summary>
        /// Returns the facet count of the given mesh (depending on the type).
        /// </summary>
        /// <param name="mesh">The mesh</param>
        /// <returns>The count of facets</returns>
        public static int FacetCount(this Mesh mesh)
        {
            switch(mesh.Type)
            {
                case FacetType.QuadMesh:
                    return mesh.Vertex.Count / 4;
                case FacetType.TriMesh:
                    return mesh.Vertex.Count / 3;
                case FacetType.TriFan:
                case FacetType.TriStripe:
                    // having an edge in common
                    return mesh.Vertex.Count - 2;
                default:
                    throw new NotImplementedException($"Missing implementation for '{mesh.Type}'");
            }
        }

        public static Mesh ShiftMeshOffset(this Mesh mesh, int offset)
        {
            var newMesh = new Mesh { Type = mesh.Type };            
            newMesh.Vertex.AddRange(mesh.Vertex.Select(i => (uint) (i + offset)));
            newMesh.Uv.AddRange(mesh.Uv);
            newMesh.Normal.AddRange(mesh.Normal);
            return newMesh;
        }

        /// <summary>
        /// True, if component has a mesh body.
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static bool IsMeshed(this Component c)
        {
            return c.Representations.Any(r => r.Bodies.Any(b => b.Faces.Count > 0));
        }

        /// <summary>
        /// True, if component has a wire representation.
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static bool IsWired(this Component c)
        {
            return c.Representations.Any(r => r.Wires.Any(w => w.Patches.Count > 0));
        }

        public static bool IsTriangle(this Facet t)
        {
            switch(t.Type)
            {
                case FacetType.TriFan:
                case FacetType.TriStripe:
                case FacetType.TriMesh:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsQuad(this Facet t)
        {
            switch (t.Type)
            {
                case FacetType.QuadMesh:
                    return true;
                default:
                    return false;
            }
        }

        public static XYZ ToXYZ(this PtArray ptArray, int index)
        {
            return new XYZ {
                X = ptArray.Xyz[index],
                Y = ptArray.Xyz[index + 1],
                Z = ptArray.Xyz[index + 2]
            };
        }

        #region Cross-Casting to System.Numerics

        public static System.Numerics.Quaternion ToNetQuaternion(this Quaternion q)
        {
            return new System.Numerics.Quaternion((float)q.X, (float)q.Y, (float)q.Z, (float)q.W);
        }

        #endregion
    }
}
