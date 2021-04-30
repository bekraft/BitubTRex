using System;
using System.Collections.Generic;
using Bitub.Dto.Spatial;

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
        public readonly MeshPtOffsetArray Meshed;
        public int Index { get; private set; }
    
        public Facet(MeshPtOffsetArray mesh, int index)
        {
            Meshed = mesh;
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
            var maxIndex = mesh.Mesh.FacetCount();
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

        public FacetType Type { get => Meshed.Mesh.Type; }

        public Orientation Orientation { get => Meshed.Mesh.Orient; }

        public uint Shift { get => Meshed.PtOffsetArray.Offset; }

        /// <summary>
        /// Returns the shifted A vertex of the facet.
        /// </summary>
        public uint A { get => Meshed.Mesh.Vertex[IndexOffset(Index, 0)] + Shift; }

        public bool HasNormals { get => Meshed.Mesh.Normal.Count > 0; }

        public bool HasUVs { get => Meshed.Mesh.Uv.Count > 0; }

        public XYZ NormalOf(int offset)
        {
            switch (Meshed.Mesh.Normal.Count)
            {
                case 0:
                    return null;
                case 3:
                    // Planar case
                    return new XYZ
                    {
                        X = Meshed.Mesh.Normal[0],
                        Y = Meshed.Mesh.Normal[1],
                        Z = Meshed.Mesh.Normal[2]
                    };
                default:
                    // Default case
                    return new XYZ
                    {
                        X = Meshed.Mesh.Normal[IndexOffset(Index, offset) * 3],
                        Y = Meshed.Mesh.Normal[IndexOffset(Index, offset) * 3 + 1],
                        Z = Meshed.Mesh.Normal[IndexOffset(Index, offset) * 3 + 2]
                    };
            }
        }

        /// <summary>
        /// Returns the shifted B vertex of the facet.
        /// </summary>
        public uint B
        {
            get {
                return Meshed.Mesh.Vertex[IndexOffset(Index, 1)] + Shift;
            }
        }

        /// <summary>
        /// Returns the shifted C vertex of the facet.
        /// </summary>
        public uint C
        {
            get {
                return Meshed.Mesh.Vertex[IndexOffset(Index, 2)] + Shift;
            }
        }

        /// <summary>
        /// Returns the shifted D vertex of the facet.
        /// </summary>
        public uint D
        {
            get {
                if(FacetType.QuadMesh == Type)
                    return Meshed.Mesh.Vertex[IndexOffset(Index, 3)] + Shift;
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
                return Meshed.Mesh.Vertex[IndexOffset(Index, Math.Abs(offset % Size))] + Shift;
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
            return (Meshed == other.Meshed) && (Index == other.Index);
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
            return Meshed.Mesh.Vertex.Count > IndexOffset(index, Size - 1);                
        }

        /// <summary>
        /// The count of vertices for this facet depending on its type
        /// </summary>
        public int Size
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
            hashCode = hashCode * -1521134295 + EqualityComparer<MeshPtOffsetArray>.Default.GetHashCode(Meshed);
            hashCode = hashCode * -1521134295 + Index.GetHashCode();
            return hashCode;
        }
    }
}
