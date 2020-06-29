using System;
using System.Xml.Linq;
using Xbim.Common;

using Microsoft.Extensions.Logging;

using Xbim.Ifc4.Interfaces;

namespace Bitub.Ifc
{
    /// <summary>
    /// Meta class of an Ifc element aka labeled element. A label is a composition of the schema version and the express entity name.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class IfcLabeledElement<T> : ILabeled where T : IPersistEntity
    {
        public T Element;

        public XName XLabel => "{" + Element.Model.SchemaVersion + "}" + Element.ExpressType.ExpressName;

        public IfcLabeledElement(T e)
        {
            Element = e;
        }
    }

    /// <summary>
    /// An Ifc product type factory based on a specific Ifc schema version.
    /// </summary>
    public class IfcProductFactory : IComponentFactory<IfcLabeledElement<IIfcProduct>>
    {
        /// <summary>
        /// The builder instance.
        /// </summary>
        public readonly IfcBuilder Builder;

        public IfcProductFactory(IfcBuilder b)
        {
            Builder = b;
        }

        public IfcLabeledElement<IIfcProduct> New(XName label)
        {
            if (null == label)
                throw new ArgumentNullException("label");

            IIfcProduct p = Builder.NewProduct(label);
            if (null != p)
            {
                var c = new IfcLabeledElement<IIfcProduct>(p);
                Builder.Log.LogDebug($"Wrapping \"{label}\" into \"{c?.XLabel}\".");
                return c;
            }
            else
            {
                Builder.Log.LogWarning($"Unable to create instance of \"{label}\" by factory.");
            }

            return null;
        }
    }
}
