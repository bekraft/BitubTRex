using System;
using System.Collections.Generic;
using System.Linq;

using System.Xml.Serialization;

namespace Bitub.Dto.Cpi.Topology
{
    public class RootContainer : CpiObject
    {
        [XmlAttribute("mRot")]
        public string Rotation { get; set; } = "1 0 0 0 1 0 0 0 1";
        [XmlAttribute("vTrans")]
        public string Translation { get; set; } = "0 0 0";

        [XmlElement("container")]
        public List<Container> Container { get; set; } = new List<Container>();
    }
}
