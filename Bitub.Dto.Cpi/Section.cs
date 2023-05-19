using System.Xml.Serialization;

namespace Bitub.Dto.Cpi
{
    public abstract class Section
    {
        protected Section(string initialVersion = null)
        {
            if (!string.IsNullOrEmpty(initialVersion))
                Version = initialVersion;
        }

        [XmlAttribute("version")]
        public string Version { get; set; } = "1.2";
    }
}
