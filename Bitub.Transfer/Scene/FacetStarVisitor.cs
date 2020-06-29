using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Bitub.Transfer.Scene
{
    /// <summary>
    /// A facet star visitor visiting facets by connecting vertices (called star).
    /// 
    /// There are two modes, first option <c>FirstWins</c> takes first facet and the second one <c>SameFaceFirst</c> will
    /// attempt to stay on the same aggregating super-face. Use the second option to optimize investigation for UV computation.
    /// </summary>
    public class FacetStarVisitor : IFacetVisitor
    {
        public enum InvestigationStrategy
        {
            /// <summary>
            /// Take the first facet found.
            /// </summary>
            FirstWins,

            /// <summary>
            /// Try to stay at the same face mesh as long it is possible
            /// </summary>
            SameFaceFirst
        }

        /// <summary>
        /// Vertices pending investigation queue.
        /// </summary>
        private readonly Stack<uint> _pending = new Stack<uint>();

        /// <summary>
        /// Known facets.
        /// </summary>
        private readonly ISet<Facet> _visited = new HashSet<Facet>();

        /// <summary>
        /// Base mesh.
        /// </summary>
        public readonly FacetStars Index;

        /// <summary>
        /// The investigation strategy.
        /// </summary>
        public readonly InvestigationStrategy Strategy;

        /// <summary>
        /// Current vertex.
        /// </summary>
        public uint? PivotVertex { get; private set; } = null;

        /// <summary>
        /// Indicates whether the investigation starts with a new face.
        /// </summary>
        public bool IsNewFace { get; private set; }

        /// <summary>
        /// Informing delegate. True, if a vertex has been already visited and shouldn't called next time.
        /// </summary>
        public Func<uint, bool> VertexIsVisited { get; set; } = (v) => false;

        private MeshPtOffsetArray _representative;

        /// <summary>
        /// A new packing state wrapping the given facet index.
        /// </summary>
        /// <param name="index">The facet index.</param>
        /// <param name="isVisitedDelegate">A delegate providing a flag whether a vertex has been visited yet or not</param>
        /// <param name="strategy">The investigation strategy</param>
        public FacetStarVisitor(FacetStars index, Func<uint, bool> isVisitedDelegate, InvestigationStrategy strategy = InvestigationStrategy.FirstWins)
        {
            Index = index;
            Strategy = strategy;
            FindStar(index);
        }

        private bool FindStar(FacetStars facetedMesh)
        {
            // Find a vertex having an unvisited facet atleast
            Func<Facet, bool> predicate;

            switch (Strategy)
            {
                case InvestigationStrategy.FirstWins:
                    predicate = f => !_visited.Contains(f);
                    break;
                case InvestigationStrategy.SameFaceFirst:
                    predicate = f => !_visited.Contains(f) && (null == _representative || f.Meshed == _representative);
                    break;
                default:
                    throw new NotImplementedException($"Not implemented for '{Strategy}'");
            }

            var star = facetedMesh.FirstOrDefault(f => f.Any(predicate));
            if (null != star)
                _pending.Push(star.Key);

            return null != star;
        }

        public bool HasNextCandidate
        {
            get {
                if (_pending.Count > 0)
                {   // In general case of connected meshes
                    return true;
                }
                else if (FindStar(Index))
                {   // In case of non-connected meshed faces
                    return true;
                }
                else
                {   // Last fall back, try reset and find any unvisited facet
                    if (null == _representative)
                        return false;
                    else
                        _representative = null;

                    return FindStar(Index);
                }
            }
        }

        public void Append(uint vertex)
        {
            _pending.Push(vertex);
        }

        public IEnumerable<Facet> Visited 
        { 
            get => _visited; 
        }

        protected IEnumerable<Facet> InvestigateNext()
        {
            PivotVertex = _pending.Pop();

            IEnumerable<Facet> facets = Index[PivotVertex.Value];
            foreach (Facet f in facets)
            {
                if (!_visited.Contains(f))
                {
                    switch (Strategy)
                    {
                        case InvestigationStrategy.FirstWins:
                            break;
                        case InvestigationStrategy.SameFaceFirst:
                            if (null != _representative && f.Meshed != _representative)
                                // Skip, if face isn't representative
                                continue;

                            break;
                        default:
                            throw new NotImplementedException($"Not implemented for '{Strategy}'");
                    }

                    IsNewFace = _representative != f.Meshed;
                    _representative = f.Meshed;
                    _visited.Add(f);

                    yield return f;
                }
            }
        }

        public IEnumerator<Facet> GetEnumerator()
        {
            while(HasNextCandidate)
            {
                foreach(var f in InvestigateNext())
                {
                    for (int i = 0; i < f.Size; i++)
                    {
                        uint v = f[i];
                        if (!VertexIsVisited(v))
                            _pending.Push(v);
                    }

                    yield return f;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
