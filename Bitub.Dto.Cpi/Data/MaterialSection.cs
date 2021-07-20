using System;
using System.Collections.Generic;

using System.Xml.Serialization;

namespace Bitub.Dto.Cpi.Data
{
    public sealed class MaterialSection : Section
    {
        public MaterialSection() : base("1.4")
        { }

        [XmlElement("material")]
        public List<Material> Material { get; set; } = new List<Material>();
    }

    public sealed class Material : CpiObject
    {
        [XmlElement("diff")]
        public Color Diffuse { get; set; } = Color.gray;
        [XmlElement("amb")]
        public Color Ambient { get; set; } = Color.gray;
        [XmlElement("spec")]
        public Color Specular { get; set; } = Color.white;
        [XmlElement("trans")]
        public RangeValue Transparency { get; set; } = RangeValue.zero;
        [XmlElement("shin")]
        public RangeValue Shinyness { get; set; } = RangeValue.one;
    }

    public sealed class Color
    {
        public static readonly Color white = new Color { R = 255, G = 255, B = 255, A = 1 };
        public static readonly Color gray = new Color { R = 127, G = 127, B = 127, A = 1 };
        public static readonly Color black = new Color { A = 1 };

        [XmlAttribute("r")]
        public byte R { get; set; }

        [XmlAttribute("g")]
        public byte G { get; set; }

        [XmlAttribute("b")]
        public byte B { get; set; }

        [XmlAttribute("a")]
        public float A { get; set; }
    }

    public sealed class RangeValue
    {
        public static readonly RangeValue one = new RangeValue { V = 255 };
        public static readonly RangeValue zero = new RangeValue { V = 0 };

        [XmlAttribute("v")]
        public float V { get; set; }
    }
}
