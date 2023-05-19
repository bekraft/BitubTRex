using System;
using System.Collections.Generic;
using System.Linq;

using System.Xml.Serialization;

namespace Bitub.Dto.Cpi.Topology
{
    [XmlType("object3D")]
    public sealed class Object3D : CpiObject, IReferences
    {
        [XmlAttribute("matID")]
        public int MaterialId { get; set; }

        [XmlAttribute("negative")]
        public string NegativeFlag { get; set; }        

        [XmlIgnore]
        public bool IsNegative
        {
            get => NegativeFlag == "true";
            set => NegativeFlag = value.ToString().ToLower();
        }

        [XmlAttribute("refID")]
        public int RefCpiId { get; set; }
    }
}
