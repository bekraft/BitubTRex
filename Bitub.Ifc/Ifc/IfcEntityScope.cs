using System;
using System.Linq;
using System.Xml.Linq;
using Xbim.Ifc4.Interfaces;

namespace Bitub.Ifc
{
    /// <summary>
    /// Generic Ifc entity creator scope bound to a builder.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class IfcEntityScope<T> : TypeScope<T>
    {
        readonly IfcBuilder Builder;

        internal IfcEntityScope(IfcBuilder builder) : base(builder, builder.IfcTypeSpace)
        {
            Builder = builder;
        }

        public E New<E>(Action<E> mod = null) where E : T
        {
            E result = (E)Builder.Store.Instances.New(this[typeof(E).XLabel()]);
            mod(result);
            return result;
        }

        public T New(XName pName)
        {
            return (T)Builder.Store.Instances.New(this[pName]);
        }

        public E NewOf<E>(object value) where E : T, Xbim.Common.IExpressValueType
        {
            var valueType = Implementing<E>().First();
            var ctor = valueType.GetConstructor(new Type[] { value.GetType() });
            return (E)ctor.Invoke(new object[] { value });
        }

        public E NewOf<E>(Action<E> mod = null) where E : T, Xbim.Common.IPersistEntity
        {
            E result = (E)Builder.Store.Instances.New(Implementing<E>().First());
            mod?.Invoke(result);
            return result;
        }
    }
}
