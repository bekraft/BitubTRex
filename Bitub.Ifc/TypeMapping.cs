using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Xml.Linq;

using Microsoft.Extensions.Logging;

namespace Bitub.Ifc
{
    public interface ILabeled
    {
        XName XLabel { get; }
    }

    public class TypePair
    {
        public XName SourceLabel { set; get; }
        public XName TargetLabel { set; get; }

        public TypePair(XName s, XName t)
        {
            SourceLabel = s;
            TargetLabel = t;
        }
    }

    public interface ITypeMapping : IEnumerable<TypePair>
    {
    }

    public class TypeMapping<S, T> : ITypeMapping where S : ILabeled where T : ILabeled
    {
        private ILogger Log;
        private IDictionary<XName, XName> m_entityMap;

        public IComponentFactory<T> Factory { get; set; }

        public TypeMapping(ILoggerFactory loggerFactory = null)
        {
            Log = loggerFactory?.CreateLogger<TypeMapping<S,T>>();
            m_entityMap = new Dictionary<XName, XName>();
        }

        public TypeMapping(TypeMapping<S,T> other)
        {
            Log = other.Log;
            m_entityMap = new Dictionary<XName, XName>(other.m_entityMap);
        }

        public void Read(XElement root)
        {
            Read(root.Elements("TypeMapping"));
        }

        public void Read(IEnumerable<XElement> mappings)
        {
            foreach (var mapping in mappings)
            {
                m_entityMap.Add(mapping.Attribute("source").Value, mapping.Attribute("target").Value);
            }
        }

        public void ReadFile(string fileName)
        {
            Read(XElement.Load(fileName));
        }

        public XName this[XName sourceType]
        {
            get {
                return m_entityMap[sourceType];
            }
        }

        public T Map(S source)
        {
            XName mapped;
            if (m_entityMap.TryGetValue(source.XLabel, out mapped))
            {
                return Factory.New(mapped);
            }
            else
            {
                Log?.LogWarning($"Unknown type mapping for \"{source.XLabel}\" detected.");
                return default(T);                
            }
        }

        public IEnumerator<TypePair> GetEnumerator()
        {
            return m_entityMap.Select(k => new TypePair(k.Key, k.Value)).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return m_entityMap.Select(k => new TypePair(k.Key, k.Value)).GetEnumerator();
        }

        public void Append(S source, T target)
        {
            Add(source.XLabel, target.XLabel);
        }

        public void Add(XName source, XName target)
        {
            m_entityMap.Add(source, target);
        }

        public void Remove(XName source)
        {
            m_entityMap.Remove(source);
        }
    }

    public interface IComponentFactory<C> where C : ILabeled 
    {
        C New(XName o);
    }
}
