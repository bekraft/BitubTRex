using System;
using System.Collections.Generic;

using System.Xml.Serialization;

namespace Bitub.Dto.Cpi
{
    public interface INamed
    {
        string Name { get; set; }
    }

    public interface IReferences
    {
        int RefCpiId { get; set; }
    }

    public abstract class CpiObject : INamed
    {
        [XmlAttribute("ID")]
        public int CpiId { get; set; }
        [XmlAttribute("name")]
        public string Name { get; set; }
    }

    public abstract class ReferencesCpiObject : IReferences
    {
        [XmlAttribute("refID")]
        public int RefCpiId { get; set; }
    }

    public abstract class NamedReferencesCpiObject : ReferencesCpiObject, INamed
    {
        [XmlAttribute("name")]
        public string Name { get; set; }
    }
}
