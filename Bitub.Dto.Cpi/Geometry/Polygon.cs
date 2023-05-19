using System;
using System.Collections.Generic;
using System.Linq;

using System.Xml.Serialization;

namespace Bitub.Dto.Cpi.Geometry
{
    public enum PolygonType
    {
        Polygon
    }

    [XmlType("pl")]
    public class Polygon
    {
        [XmlAttribute("type")]
        public PolygonType Type { get; set; } = PolygonType.Polygon;

        [XmlAttribute("length")]
        public int BoundaryLoopCount { get; set; }

        [XmlAttribute("points")]
        public string BoundaryLoopChained { get; set; }

        [XmlIgnore]
        public int[] BoundaryLoop
        {
            get => BoundaryLoopChained?.Split(' ').Select(s => int.Parse(s)).ToArray();
            set => BoundaryLoopChained = string.Join(" ", value);
        }

        [XmlArray("tess")]
        public List<Triangle> Tesselation { get; set; } = new List<Triangle>();
    }
}
