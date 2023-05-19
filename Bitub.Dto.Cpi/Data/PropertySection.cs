using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Xml.Serialization;

namespace Bitub.Dto.Cpi.Data
{
    [XmlType("propertySection")]
    public sealed class PropertySection : Section
    {
        public PropertySection() : base("1.3")
        { }

        #region Pre 1.7 ?
        [XmlElement("property")]
        public List<Property> Property { get; set; } = new List<Property>();

        #endregion
        #region Post 1.3
        [XmlElement("propInst")]
        public List<Attribute> Attribute { get; set; } = new List<Attribute>();

        [XmlElement("propSet")]
        public List<AttributeSet> AttributeSet { get; set; } = new List<AttributeSet>();
        #endregion
    }

    [XmlType("propSet")]
    public sealed class AttributeSet : ReferencesCpiObject
    {
        [XmlText]
        public string AttributeIndexesChained { get; set; }

        [XmlIgnore]
        public int[] AttributeIndexes
        {
            get => AttributeIndexesChained?.Split(' ').Select(s => int.Parse(s)).ToArray();
            set => AttributeIndexesChained = string.Join(" ", value);
        }
    }

    [XmlType("propInst")]
    public sealed class Attribute : INamed
    {
        [XmlAttribute("nr")]
        public int Nr { get; set; }

        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("datatype")]
        public PropertyDatatype Datatype { get; set; } = PropertyDatatype.String;
        
        [XmlText]
        public string Value { get; set; }

        public object AsTypedValue()
        {
            switch (Datatype)
            {
                case PropertyDatatype.String:
                    return Value;
                case PropertyDatatype.Boolean:
                    return Value.Trim().ToLower() == "true";
                case PropertyDatatype.Double:
                    return double.Parse(Value);
                case PropertyDatatype.Integer:
                    return int.Parse(Value);
                case PropertyDatatype.Reference:
                case PropertyDatatype.Identifer:
                    return Value.Trim();
                default:
                    throw new NotImplementedException();
            }
        }
    }

    [XmlType("property")]
    public sealed class Property : NamedReferencesCpiObject
    {
        [XmlAttribute("datatype")]
        public PropertyDatatype Datatype { get; set; } = PropertyDatatype.String;
        [XmlText]
        public string Value { get; set; }

        public object AsTypedValue()
        {
            switch (Datatype)
            {
                case PropertyDatatype.String:
                    return Value;
                case PropertyDatatype.Boolean:
                    return Value.Trim().ToLower() == "true";
                case PropertyDatatype.Double:
                    return double.Parse(Value);
                case PropertyDatatype.Integer:
                    return int.Parse(Value);
                case PropertyDatatype.Reference:
                case PropertyDatatype.Identifer:
                    return Value.Trim();
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
