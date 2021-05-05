using System;
using System.Collections.Generic;
using System.Linq;

using Bitub.Dto.Spatial;

namespace Bitub.Dto.Scene
{
    public partial class ShapeBody
    {
        /// <summary>
        /// Returns a sequence of float values as triples of point coordinates.
        /// </summary>
        /// <returns>Sequence of floats</returns>
        public IEnumerable<float> GetCoordinateTriples()
        {
            return Points.SelectMany(pta => pta.Xyz);
        }

        /// <summary>
        /// Returns a sequence of points.
        /// </summary>
        /// <returns>Sequence of points</returns>
        public IEnumerable<XYZ> GetPoints()
        {
            return Points.SelectMany(pta => pta.ToXYZ());
        }

        /// <summary>
        /// Returns the total sum of points held by shape body point arrays.
        /// </summary>
        /// <returns>Total count of points (one third of coordinate values)</returns>
        public int GetTotalPointCount()
        {
            return Points.Select(pta => pta.Xyz.Count / 3).Sum();
        }

        /// <summary>
        /// Aggregates all sub shapes into a sequence of indexed factes.
        /// </summary>
        /// <returns>A sequence of facet arrays wrapping each a separate mesh</returns>
        public IEnumerable<Facet[]> GetFacets()
        {
            var ptArrayMap = Points.ToDictionary(pta => pta.Id, pta => new PtOffsetArray(pta));
            return Bodies.Select(b => b.GetFacets(ptArrayMap).ToArray());
        }

        /// <summary>
        /// Aggregates all sub shapes into a consequtive sequence of indexed factes. Indexes will
        /// refer to the output of <see cref="ToTriples(ShapeBody)"/> or <see cref="ToPoints(ShapeBody)"/>.
        /// </summary>
        /// <param name="startOffset">An optional start offset of index.</param>
        /// <returns>A sequence of facet arrays wrapping each a separate mesh</returns>
        public IEnumerable<Facet[]> GetContinuousFacets(uint startOffset = 0)
        {
            var ptArrayMap = GetPtOffsetArray(startOffset).ToDictionary(pta => pta.Points.Id);
            return Bodies.Select(b => b.GetFacets(ptArrayMap).ToArray());
        }

        public IEnumerable<PtOffsetArray> GetPtOffsetArray(uint startOffset = 0)
        {
            uint offset = startOffset;
            foreach (var ptArray in Points)
            {
                yield return new PtOffsetArray(ptArray, offset);
                offset += (uint)ptArray.Xyz.Count / 3;
            }
        }
    }
}
