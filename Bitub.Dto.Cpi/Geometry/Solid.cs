using System;
using System.Collections.Generic;

using System.Xml.Serialization;

namespace Bitub.Dto.Cpi.Geometry
{
    public enum FaceType
    {
        Planar, Curved
    }

    [XmlType("solid")]
    public class Solid
    {
        [XmlElement("face")]
        public List<Face> Face { get; set; } = new List<Face>();
    }

    [XmlType("face")]
    public class Face
    {
        [XmlAttribute("shape")]
        public FaceType ShapeType { get; set; }
        [XmlElement("t")]
        public List<Triangle> Tessellation { get; set; } = new List<Triangle>();
    }

    [XmlType("t")]
    public class Triangle
    {
        [XmlAttribute("p1")]
        public int P1 { get; set; }
        [XmlAttribute("p2")]
        public int P2 { get; set; }
        [XmlAttribute("p3")]
        public int P3 { get; set; }

        public bool IsValid
        {
            get => P1 != P2 && P2 != P3 && P3 != P1;
        }
    }
}
