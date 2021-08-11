using System;
using System.Linq;

using Xbim.Common;

using Bitub.Dto;

namespace Bitub.Ifc
{
    /// <summary>
    /// Generic Ifc entity creator scope bound to a builder.
    /// </summary>
    /// <typeparam name="T">The type of entity</typeparam>
    public class IfcEntityScope<T> : TypeScope where T : Xbim.Common.IPersist
    {
        #region Internals

        private readonly IfcBuilder builder;

        public IfcEntityScope(IfcBuilder builder) 
            : base(typeof(T), builder.ifcAssembly, new System.Reflection.Module[] { builder.ifcAssembly.factory.GetType().Module })
        {
            this.builder = builder;
        }

        #endregion

        public IfcEntityScope<E> GetEntityScopeOf<E>() where E : T
        {
            return new IfcEntityScope<E>(builder);
        }

        public E New<E>(Type t, Action<E> mod = null) where E : IPersistEntity
        {
            var result = (E)builder.model.Instances.New(this[t.ToQualifier()]);
            mod(result);
            return result;
        }

        public E New<E>(Action<E> mod = null) where E : T
        {
            E result = (E)builder.model.Instances.New(this[typeof(E).ToQualifier()]);
            mod(result);
            return result;
        }

        public T New(Qualifier qualifiedType)
        {
            return (T)builder.model.Instances.New(this[qualifiedType]);
        }

        public E NewOf<E>(object value) where E : Xbim.Common.IExpressValueType
        {
            var valueType = Implementing<E>().First();
            var ctor = valueType.GetConstructor(new Type[] { value.GetType() });
            return (E)ctor.Invoke(new object[] { value });
        }

        public E NewOf<E>(Action<E> mod = null) where E : T, Xbim.Common.IPersistEntity
        {
            E result = (E)builder.model.Instances.New(Implementing<E>().First());
            mod?.Invoke(result);            
            return result;
        }
    }
}
