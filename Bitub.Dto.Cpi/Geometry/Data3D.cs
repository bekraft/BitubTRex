using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Xml.Serialization;

namespace Bitub.Dto.Cpi.Geometry
{
    public sealed class Data3D
    {
        [XmlAttribute("refID")]
        public string RefID { get; set; }

        [XmlElement("p")]
        public List<Point> Point { get; set; } = new List<Point>();
        [XmlElement("solid")]
        public List<Solid> Solid { get; set; } = new List<Solid>();
    }
}
