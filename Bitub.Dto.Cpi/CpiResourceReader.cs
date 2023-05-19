using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using System.Xml;
using System.Xml.Serialization;

namespace Bitub.Dto.Cpi
{
    public class CpiResourceReader
    {
        private List<ContentType> contentTypes;        
        private IdMappingContainer idMappingContainer;
        private IDictionary<ContentType, Section> content;            

        public CpiResourceReader(Stream s)
        {
            Init(s);
        }

        private void Init(Stream s)
        {
            content = new Dictionary<ContentType, Section>();
            using (var reader = XmlReader.Create(s))
            {
                var sectionMap = Enum.GetValues(typeof(ContentType))
                    .Cast<ContentType>().ToDictionary(t => t.ToString());
                IsFragment = true;

                while (reader.Read())
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                            if (reader.Name == "objects")
                            {
                                IsFragment = false;
                                ProjectID = reader.GetAttribute("projectID");
                                SourceApplication = reader.GetAttribute("sourceApp");
                            }
                            else if (reader.Name == "content")
                            {
                                var contentSerializer = new XmlSerializer(typeof(ContentSection), new XmlRootAttribute("content"));
                                using (var nested = reader.ReadSubtree())
                                {
                                    var contentSection = contentSerializer.Deserialize(nested) as ContentSection;
                                    contentTypes = contentSection.Section.Select(section => section.SectionType).ToList();
                                }
                            }
                            else if (reader.Name == "IDMapping")
                            {
                                var mappingSerializer = new XmlSerializer(typeof(IdMappingContainer), new XmlRootAttribute("IDMapping"));
                                using (var nested = reader.ReadSubtree())
                                {
                                    idMappingContainer = mappingSerializer.Deserialize(nested) as IdMappingContainer;
                                }
                            }
                            else
                            {
                                ContentType ct;
                                if (sectionMap.TryGetValue(reader.Name, out ct))
                                {
                                    content[ct] = ReadSection(reader.ReadSubtree(), ct);
                                }
                            }
                            break;
                    }
                }
            }
        }

        private Section ReadSection(XmlReader partialReader, ContentType ct)
        {
            switch(ct)
            {
                case ContentType.objectSection:
                    return ReadObjectSection(partialReader);
                case ContentType.objectDataSection:
                    return ReadObjectDataSection(partialReader);
                case ContentType.extendedObjectSection:
                    return null;
                default:
                    throw new NotImplementedException();
            }
        }

        public bool IsFragment { private set; get; }

        public string Version { private set; get; }

        public string ProjectID { private set; get; }
        
        public string SourceApplication { private set; get; }

        public ContentType[] DeclaredContents { get => contentTypes.ToArray(); }

        public ContentType[] Contents { get => content.Select(s => s.Key).ToArray(); }

        public IDictionary<int, string> MappedID { get => idMappingContainer?.Mapping.ToDictionary(m => m.CpiId, m => m.SourceId); }

        public Section GetSection(ContentType ct)
        {
            return content[ct];
        }

        public bool HasSection(ContentType ct)
        {
            return content.ContainsKey(ct);
        }

        public ObjectSection ObjectSection { get => content[ContentType.objectSection] as ObjectSection; }

        public ObjectDataSection ObjectDataSection { get => content[ContentType.objectDataSection] as ObjectDataSection; }

        public static ObjectSection ReadObjectSection(XmlReader partialReader)
        {
            var serializer = new XmlSerializer(typeof(ObjectSection), new XmlRootAttribute("objectSection"));
            return serializer.Deserialize(partialReader) as ObjectSection;
        }

        public ObjectDataSection ReadObjectDataSection(XmlReader partialReader)
        {
            var serializer = new XmlSerializer(typeof(ObjectDataSection), new XmlRootAttribute("objectDataSection"));
            return serializer.Deserialize(partialReader) as ObjectDataSection;
        }
    }
}
