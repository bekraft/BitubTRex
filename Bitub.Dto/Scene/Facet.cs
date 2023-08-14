using System;
using System.Linq;
using System.Collections.Generic;

using Bitub.Dto.Spatial;
using Bitub.Dto.Scene;

namespace Bitub.Dto.Scene
{
    /// <summary>
    /// A generic mesh facet implementing different types of mesh indexing.
    /// There are 3 types of indexer: <c>Index</c> denotes the index of the facet within the associated
    /// point array, <c>Shift</c> denotes the shifted offset of of the point array and the internal offset denotes
    /// the offset of a single vertex of a facet (0 to 3).
    /// </summary>
    public sealed class Facet : IEquatable<Facet>
    {
        public readonly MeshPtOffsetArray meshed;
        public int Index { get; private set; }
    
        public Facet(MeshPtOffsetArray mesh, int index)
        {
            meshed = mesh;
            Index = index;
        }

        /// <summary>
        /// Creates an enumerable of facets having only a single instance of a single facet sliding over
        /// the mesh vertices. It means, that you cannot rely on object identity (always the same facet with different index).
        /// </summary>
        /// <param name="mesh">The mesh</param>
        /// <returns>An enumerable facet</returns>
        public static IEnumerable<Facet> TransientFacetsOf(MeshPtOffsetArray mesh)
        {
            var maxIndex = mesh.Mesh.FacetCount;
            if (maxIndex > 0)
            {
                Facet f = new Facet(mesh, 0);
                for (int i = 0; i < maxIndex; i++)
                {
                    f.Index = i;
                    yield return f;
                }
            }
        }

        public FacetType Type { get => meshed.Mesh.Type; }

        public Orientation Orientation { get => meshed.Mesh.Orient; }

        public IEnumerable<Arc<uint>> Loop
        {
            get => ArcExtensions.FromLoop(Enumerable.Range(0, (int)Size).Cast<uint>());
        }

        public IEnumerable<Arc<XYZ>> LoopXYZ
        {
            get => ArcExtensions.FromLoop(Enumerable.Range(0, (int)Size).Select(k => GetXYZ(k)));
        }

        /// <summary>
        /// Returns a <see cref="Simplex2"/> struct wrapping only the index information.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Simplex2> Triangles
        {
            get 
            {
                yield return new Simplex2 { A = A, B = B, C = C };
                if (!IsTriangle)
                    yield return new Simplex2 { A = A, B = C, C = D };
            }
        }

        /// <summary>
        /// A new <see cref="Simplex2"/> with reordered vertices keeping the global orientation.
        /// </summary>
        /// <param name="pivot">The pivot index (A index)</param>
        /// <returns>A reordered topological triangle</returns>
        public IEnumerable<Simplex2> ToTriangles(uint? pivot)
        {
            if (!pivot.HasValue)
                pivot = A;

            uint? pivotIdx = null;
            for (uint k = 0; k < Size; ++k)
            {
                if (Vertex(k) == pivot)
                    pivotIdx = k;
            }

            if (!pivotIdx.HasValue)
                throw new ArgumentException($"Pivot {pivot} has no equivalent within facet.");

            yield return new Simplex2 { A = Vertex(pivotIdx.Value), B = Vertex((pivotIdx.Value + 1) % Size), C = Vertex((pivotIdx.Value + 2) % Size) };
            if (!IsTriangle)
                yield return new Simplex2 { A = Vertex(pivotIdx.Value), B = Vertex((pivotIdx.Value + 2) % Size), C = Vertex((pivotIdx.Value + 3) % Size) };
        }

        public uint Shift 
        { 
            get => meshed.PtOffsetArray.Offset; 
        }

        public uint Vertex(uint offset)
        {
            if (offset >= Size || offset < 0)
                throw new ArgumentException($"Offset {offset} out of allowed range [0;{Size-1}].");

            return meshed.Mesh.Vertex[IndexOffset(Index, (int)offset)] + Shift;
        }

        /// <summary>
        /// Returns the shifted A vertex of the facet.
        /// </summary>
        public uint A 
        { 
            get => meshed.Mesh.Vertex[IndexOffset(Index, 0)] + Shift; 
        }

        public bool HasNormals 
        { 
            get => meshed.Mesh.Normal.Count > 0; 
        }

        public bool HasUVs 
        { 
            get => meshed.Mesh.Uv.Count > 0; 
        }

        /// <summary>
        /// Will return the normal of the face at given vertex (0 to n-1).
        /// </summary>
        /// <param name="offset">The offset starting from 0.</param>
        /// <param name="computeIfAbsent">Compute, if no normal exists</param>
        /// <returns>A normal vector pointing towards spectator.</returns>
        public XYZ GetNormal(int offset, bool computeIfAbsent = false)
        {
            switch (meshed.Mesh.Normal.Count)
            {
                case 0:
                    //meshed.PtOffsetArray.Points
                    return null;
                case 3:
                    // Planar case
                    return new XYZ
                    {
                        X = meshed.Mesh.Normal[0],
                        Y = meshed.Mesh.Normal[1],
                        Z = meshed.Mesh.Normal[2]
                    };
                default:
                    // Default case, each point has a normal
                    var baseOffset = IndexOffset(Index, offset) * 3;
                    return new XYZ
                    {
                        X = meshed.Mesh.Normal[baseOffset],
                        Y = meshed.Mesh.Normal[baseOffset + 1],
                        Z = meshed.Mesh.Normal[baseOffset + 2]
                    };
            }
        }

        public XYZ GetXYZ(int offset)
        {
            var baseOffset = (int)meshed.Mesh.Vertex[IndexOffset(Index, offset)] * 3;
            return new XYZ
            {
                X = meshed.PtOffsetArray.Points.Xyz[baseOffset],
                Y = meshed.PtOffsetArray.Points.Xyz[baseOffset + 1],
                Z = meshed.PtOffsetArray.Points.Xyz[baseOffset + 2]
            };
        }

        /// <summary>
        /// Returns the facet normal according the the embedding plane.
        /// </summary>
        public XYZ Normal
        {
            get {
                var a = GetXYZ(0);
                var ab = GetXYZ(1).Sub(a);
                var ac = GetXYZ(2).Sub(a);
                return ab.Cross(ac).ToNormalized();
            }
        }

        /// <summary>
        /// Returns the shifted B vertex of the facet.
        /// </summary>
        public uint B
        {
            get {
                return meshed.Mesh.Vertex[IndexOffset(Index, 1)] + Shift;
            }
        }

        /// <summary>
        /// Returns the shifted C vertex of the facet.
        /// </summary>
        public uint C
        {
            get {
                return meshed.Mesh.Vertex[IndexOffset(Index, 2)] + Shift;
            }
        }

        /// <summary>
        /// Returns the shifted D vertex of the facet.
        /// </summary>
        public uint D
        {
            get {
                if(FacetType.QuadMesh == Type)
                    return meshed.Mesh.Vertex[IndexOffset(Index, 3)] + Shift;
                else
                    throw new NotSupportedException($"{Type} does not support indices > 2");
            }
        }

        /// <summary>
        /// Continous ring accessor to shifted facet vertices.
        /// </summary>
        /// <param name="offset">The offset (vertex indexer)</param>
        public uint this[int offset]
        {
            get {
                return meshed.Mesh.Vertex[IndexOffset(Index, Math.Abs(offset % (int)Size))] + Shift;
            }
        }

        /// <summary>
        /// Whether the given triangle is topologically valid.
        /// </summary>
        /// <returns>True, if valid</returns>
        public bool IsValid()
        {
            switch(Type)
            {
                case FacetType.TriFan:
                case FacetType.TriStripe:
                case FacetType.TriMesh:
                    return (A != B) && (B != C);
                case FacetType.QuadMesh:
                    return (A != B) && (B != C) && (C != D);
                default:
                    throw new NotImplementedException($"Missing implementation of '{Type}'");

            }
        }

        public bool Equals(Facet other)
        {
            return (meshed == other.meshed) && (Index == other.Index);
        }

        public override bool Equals(object obj)
        {
            if (obj is Facet f)
                return Equals(f);
            else
                return false;
        }       

        public bool IsValidIndex(int index)
        {
            // True, if more vertices than the given offset index
            return meshed.Mesh.Vertex.Count > IndexOffset(index, (int)Size - 1);                
        }

        /// <summary>
        /// The count of vertices for this facet depending on its type
        /// </summary>
        public uint Size
        {
            get {
                switch (Type)
                {
                    case FacetType.TriFan:
                    case FacetType.TriStripe:
                    case FacetType.TriMesh:
                        return 3;
                    case FacetType.QuadMesh:
                        return 4;
                    default:
                        throw new NotImplementedException($"Missing implementation of '{Type}'");
                }
            }
        }

        public bool IsTriangle 
        {
            get {
                switch (Type)
                {
                    case FacetType.TriFan:
                    case FacetType.TriStripe:
                    case FacetType.TriMesh:
                        return true;
                    default:
                        return false;
                }
            }
        }

        public bool IsQuad
        {
            get {
                switch (Type)
                {
                    case FacetType.QuadMesh:
                        return true;
                    default:
                        return false;
                }
            }
        }

        internal int IndexOffset(int index, int offset)
        {
            switch(Type)
            {
                case FacetType.TriMesh:
                    return index * 3 + offset;
                case FacetType.QuadMesh:
                    return index * 4 + offset;
                case FacetType.TriFan:
                    if (offset > 0)
                        return index + offset;                     
                    else
                        return 0;
                case FacetType.TriStripe:
                    if (0 == index % 2)
                        return index + offset;
                    else
                        return index + (2 - offset);
                default:
                    throw new NotImplementedException($"Missing implementation of {Type}");
            }
        }

        public override int GetHashCode()
        {
            int hashCode = 749459680;
            hashCode = hashCode * -1521134295 + EqualityComparer<MeshPtOffsetArray>.Default.GetHashCode(meshed);
            hashCode = hashCode * -1521134295 + Index.GetHashCode();
            return hashCode;
        }

        public override string ToString()
        {
            if (IsTriangle)
                return $"{Type}{Index}[{A}-{B}-{C}]";
            else
                return $"{Type}{Index}[{A}-{B}-{C}-{D}]";
        }
    }
}
