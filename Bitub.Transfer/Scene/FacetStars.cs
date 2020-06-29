using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Bitub.Transfer.Scene
{
    /// <summary>
    /// Facet star lookup by vertex. Wraps one or more BREP bodies referencing the same 
    /// point array.
    /// </summary>
    public sealed class FacetStars : ILookup<uint, Facet>
    {
        #region Private
        private Dictionary<uint, List<Facet>> _index;
        #endregion

        public readonly FaceBody Body;
        public readonly PtOffsetArray PtOffsetArray;

        /// <summary>
        /// Grouping of a star index.
        /// </summary>
        public class VertexFacetGrouping : IGrouping<uint, Facet>
        {
            public uint Key { get; private set; }
            public readonly IEnumerable<Facet> Star;

            internal VertexFacetGrouping(uint vertex, IEnumerable<Facet> star)
            {
                Key = vertex;
                Star = star;
            }

            public IEnumerator<Facet> GetEnumerator()
            {
                return Star.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return Star.GetEnumerator();
            }
        }

        /// <summary>
        /// A new indexed star lookup wrapping a single body definition.
        /// </summary>
        /// <param name="body">The body to index</param>
        /// <param name="offset">An optional offset in vertex array</param>
        public FacetStars(FaceBody body, PtOffsetArray ptOffsetArray)
        {
            _index = new Dictionary<uint, List<Facet>>();
            Body = body;
            PtOffsetArray = ptOffsetArray;
            Ingest(body);
        }

        public IEnumerable<Facet> this[uint vertex]
        {
            get {
                List<Facet> star;
                if (_index.TryGetValue(vertex, out star))
                    return star;
                else
                    return Enumerable.Empty<Facet>();
            }
        }

        /// <summary>
        /// Count of stars within index.
        /// </summary>
        public int Count { get => _index.Count; }

        public bool Contains(uint vertex)
        {
            return _index.ContainsKey(vertex);
        }

        public IEnumerator<IGrouping<uint, Facet>> GetEnumerator()
        {
            return _index.Select(kv => new VertexFacetGrouping(kv.Key, kv.Value)).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerable<uint> Vertices { get => _index.Keys; }

        /// <summary>
        /// Whether the mesh is watertight (no gaps) according to the topology. Each star
        /// should have 3 adjacent Facet at minimum (assuming euclidean embedding).
        /// </summary>        
        public bool IsTopoWT { get => _index.All(kv => kv.Value.Count > 2); }

        /// <summary>
        /// Returns the star of given vertex ID.
        /// </summary>
        /// <param name="vertex"></param>
        /// <returns></returns>
        public IEnumerable<Facet> StarOf(uint vertex)
        {
            List<Facet> starOf;
            if(_index.TryGetValue(vertex, out starOf))
            {
                return starOf.ToArray();
            }
            return Enumerable.Empty<Facet>();
        }

        private bool Ingest(Facet f)
        {
            if (!f.IsValid())
                return false;

            List<Facet> starOf;
            for (int i = 0; i < f.Size; i++)
            {
                uint vertex = f[i];
                if (!_index.TryGetValue(vertex, out starOf))
                {
                    starOf = new List<Facet>();
                    _index.Add(vertex, starOf);
                }                
                starOf.Add(f);
            }

            return true;
        }

        /// <summary>
        /// Ingest a BREP body into facet index. The references body has to reference the same 
        /// point array otherwise the result isn't predictable.
        /// </summary>
        /// <param name="body">A body</param>
        /// <returns>True, if only new facets have been added</returns>
        public bool Ingest(FaceBody body)
        {
            bool isIngestedAll = true;
            foreach(var facet in body.Faces.SelectMany(f => new MeshPtOffsetArray(f.Mesh, PtOffsetArray).ToFacets()))
            {
                isIngestedAll &= Ingest(facet);
            }
            return isIngestedAll;
        }
    }
}
