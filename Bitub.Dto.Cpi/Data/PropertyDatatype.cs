
using System.Xml.Serialization;

namespace Bitub.Dto.Cpi.Data
{
    public enum PropertyDatatype
    {
        [XmlEnum("xs:string")]
        String,
        [XmlEnum("xs:double")]
        Double,
        [XmlEnum("xs:int")]
        Integer,
        [XmlEnum("xs:boolean")]
        Boolean,
        [XmlEnum("xs:IDREF")]
        Reference,
        [XmlEnum("xs:ID")]
        Identifer,
    }
}
