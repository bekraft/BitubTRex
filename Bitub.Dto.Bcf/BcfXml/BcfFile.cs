﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Bitub.Dto.BcfXml
{
    public class BcfFile : IDisposable
    {
        public static readonly XmlDeserializationEvents XmlDeserializationEvents = new XmlDeserializationEvents();

        #region Internals
        private ZipArchive zip;       
        private BcfVersion version;
        private BcfProjectExtension projectExtension;

        internal static XmlParserContext parserContext = CreateParserContext();

        protected BcfFile(ZipArchive bcfArchive)
        {
            zip = bcfArchive;
            projectExtension = Deserialize<BcfProjectExtension>(zip.GetEntry("project.bcfp")?.Open()) ?? new BcfProjectExtension { Project = new BcfProject() };
            version = Deserialize<BcfVersion>(zip.GetEntry("bcf.version")?.Open()) ?? new BcfVersion { DetailedVersion = "2.1", VersionId = "2.1" };
        }

        internal static T Deserialize<T>(Stream xmlStream)
        {
            if (null != xmlStream)
                return (T)new XmlSerializer(typeof(T)).Deserialize(GetReader(xmlStream), XmlDeserializationEvents);
            else
                return default(T);
        }

        internal static Stream Serialize<T>(T obj, Stream s)
        {
            if (null != obj)
                new XmlSerializer(typeof(T)).Serialize(s, obj);
            return s;
        }

        internal static XmlParserContext CreateParserContext()
        {
            NameTable nt = new NameTable();
            XmlNamespaceManager manager = new XmlNamespaceManager(nt);
            manager.AddNamespace("xsi", "urn:http://www.w3.org/2001/XMLSchema-instance");
            XmlParserContext ctx = new XmlParserContext(null, manager, null, XmlSpace.None);
            return ctx;
        }

        #endregion

        public BcfProject Project { get => projectExtension?.Project; }
        public string BcfVersion { get => version?.DetailedVersion ?? "2.1"; }

        public static BcfFile ReadFrom(string bcfArchiveName)
        {
            return ReadFrom(new FileStream(bcfArchiveName, FileMode.Open));
        }

        public static BcfFile ReadFrom(Stream fileStream)
        {
            return new BcfFile(new ZipArchive(fileStream, ZipArchiveMode.Read));
        }

        public static BcfFile NewBcfArchive(string bcfArchiveName, BcfProjectExtension projectExtension, string version = "2.1")
        {
            var bcfVersion = new BcfVersion { VersionId = version, DetailedVersion = version };
            using (var bcfArchive = new ZipArchive(new FileStream(bcfArchiveName, FileMode.OpenOrCreate), ZipArchiveMode.Create))
            {
                using(var bcfv = bcfArchive.CreateEntry("bcf.version").Open())
                    Serialize(bcfVersion, bcfv);
                using (var bcfp = bcfArchive.CreateEntry("project.bcfp").Open())
                    Serialize(projectExtension, bcfp);
            }
            return new BcfFile(new ZipArchive(new FileStream(bcfArchiveName, FileMode.Open), ZipArchiveMode.Update));
        }
        
        internal protected static XmlReader GetReader(Stream s)
        {            
            var reader = XmlReader.Create(s, new XmlReaderSettings             
            { 
                ConformanceLevel = ConformanceLevel.Fragment,
                ValidationFlags = XmlSchemaValidationFlags.None,                
            }, CreateParserContext());
            return reader;
        }

        private Func<string, Stream> CreateStreamAccessor(string topicId)
        {
            return (fileName) =>
            {
                if (fileName.StartsWith("../"))
                    return zip.GetEntry(fileName.Replace("../", ""))?.Open();
                else
                    return zip.GetEntry($"{topicId}/{fileName}")?.Open();
            };
        }

        private Func<Regex, string[]> CreateFileFilter(string topicId, string[] filesInTopic)
        {
            return (regEx) => filesInTopic
                    .Where(n => regEx.IsMatch(n))
                    .Select(n => n.Replace($"{topicId}/", ""))
                    .ToArray();
        }

        protected virtual BcfIssue ReadBcfIssue(string topicId, string[] filesInTopic)
        {
            return new BcfIssue(CreateStreamAccessor(topicId), CreateFileFilter(topicId, filesInTopic));
        }

        private IEnumerable<string> GetRestrictedEnum(XmlSchemaSimpleTypeContent content)
        {
            if (content is XmlSchemaSimpleTypeRestriction restriction)
            {
                return restriction.Facets.OfType<XmlSchemaEnumerationFacet>().Select(f => f.Value);
            }
            return Enumerable.Empty<string>();
        }

        /// <summary>
        /// Returns a lookup of available extensions to the current topic's attributes.
        /// </summary>
        public ILookup<string, string> Extensions
        {
            get {
                if (!string.IsNullOrWhiteSpace(projectExtension.ExtensionSchema))
                {   
                    using (var redefineSchema = zip.GetEntry(projectExtension.ExtensionSchema)?.Open())
                    {
                        return XmlSchema.Read(redefineSchema, (s, e) => { })
                            .Includes.OfType<XmlSchemaRedefine>()
                            .SelectMany(s =>
                                s.Items.OfType<XmlSchemaSimpleType>().SelectMany(t => GetRestrictedEnum(t.Content).Select(Value => (t.Name, Value))))
                            .ToLookup(s => s.Name, s => s.Value);
                    }
                }
                else
                {
                    return null;
                }
            }
        }

        public IEnumerable<BcfIssue> Issues
        {
            get => zip.Entries
                .ToLookup(e => CommonExtensions.guidRegExpression.Match(e.FullName)?.Value)
                .Where(g => !string.IsNullOrWhiteSpace(g.Key))
                .Select(g => ReadBcfIssue(g.Key, g.Select(e => e.FullName).ToArray()));
        }

        public void Dispose()
        {
            if (null == zip)
                throw new ObjectDisposedException("Already disposed");

            zip.Dispose();
            zip = null;
        }
    }
}
