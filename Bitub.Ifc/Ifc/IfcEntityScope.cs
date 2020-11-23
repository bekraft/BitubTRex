using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Bitub.Dto;
using Xbim.Common.Step21;

namespace Bitub.Ifc
{
    /// <summary>
    /// Generic Ifc entity creator scope bound to a builder.
    /// </summary>
    /// <typeparam name="T">The type of entity</typeparam>
    public class IfcEntityScope<T> : TypeScope<T>
    {
        #region Internals

        private readonly IfcBuilder builder;

        internal protected IfcEntityScope(IfcBuilder builder, AssemblyName spaceName) 
            : base(builder.assemblyScope, spaceName, new Regex(@"(.)+(?=(\.\w+$))", RegexOptions.Compiled), builder.store.SchemaVersion.ToString())
        {
            this.builder = builder;
        }

        #endregion

        public E New<E>(Action<E> mod = null) where E : T
        {
            E result = (E)builder.store.Instances.New(this[typeof(E).ToQualifier()]);
            mod(result);
            return result;
        }

        public T New(Qualifier qualifiedType)
        {
            return (T)builder.store.Instances.New(this[qualifiedType]);
        }

        public E NewOf<E>(object value) where E : T, Xbim.Common.IExpressValueType
        {
            var valueType = Implementing<E>().First();
            var ctor = valueType.GetConstructor(new Type[] { value.GetType() });
            return (E)ctor.Invoke(new object[] { value });
        }

        public E NewOf<E>(Action<E> mod = null) where E : T, Xbim.Common.IPersistEntity
        {
            E result = (E)builder.store.Instances.New(Implementing<E>().First());
            mod?.Invoke(result);            
            return result;
        }
    }
}
