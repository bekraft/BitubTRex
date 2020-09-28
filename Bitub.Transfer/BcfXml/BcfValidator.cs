using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;

namespace Bitub.Transfer.BcfXml
{
    public class BcfValidator
    {
        static readonly string[] BcfSchemata = new string[] { "markup.xsd", "project.xsd", "version.xsd", "visinfo.xsd" };

        private readonly IDictionary<string, XmlSchemaSet> _schemaSets;

        public BcfValidator()
        {
            var assembly = typeof(BcfFile).Assembly;
            _schemaSets = BcfSchemata.ToDictionary(name => name, name =>
            {
                var set = new XmlSchemaSet();
                set.Add(XmlSchema.Read(assembly.GetManifestResourceStream($"Bitub.Transfer.Resources.BCF21.{name}"), (s, e) => Console.WriteLine($"{s} = ${e}")));
                set.Compile();
                return set;
            });            
        }
    }
}
