using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;
using System.Collections;

namespace Bitub.Dto.Rest
{
    [Flags]
    public enum DtoAggregateMethodFlag
    {
        Everything = 0,
        ExceptNulls = 1,
        ExceptDefaults = 2,
        ExceptEmptyStrings = 4,
        ExceptId = 8,
        NonIdAndDefinedOnly = 1 | 2 | 4 | 8
    }

    public sealed class DtoDescriptor<T> : IEnumerable<PropertyInfo> where T : IDtoEntity
    {
        private readonly Dictionary<string, PropertyInfo> property;
        private readonly object[] emptyArray = { };
        private readonly object defaultValue;

        public DtoDescriptor()
        {
            property = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => (p.SetMethod?.IsPublic ?? false))
                .ToDictionary(p => p.Name);
            defaultValue = typeof(T).IsValueType ? Activator.CreateInstance(typeof(T)) : null;
        }

        public IEnumerable<(string, object)> Decompose(T instance)
        {
            foreach (var propInfo in property.Values)
                yield return (propInfo.Name, propInfo.GetMethod.Invoke(instance, emptyArray));
        }

        public T Aggregate(T instance, IEnumerable<(string, object)> properties, DtoAggregateMethodFlag updateFlag = DtoAggregateMethodFlag.Everything)
        {
            foreach (var (name, value) in properties)
            {
                bool doUpdate = true;
                if (null == value && updateFlag.HasFlag(DtoAggregateMethodFlag.ExceptNulls))
                    doUpdate = false;
                if (defaultValue == value && updateFlag.HasFlag(DtoAggregateMethodFlag.ExceptDefaults))
                    doUpdate = false;
                if (value is string s && string.IsNullOrWhiteSpace(s) && updateFlag.HasFlag(DtoAggregateMethodFlag.ExceptEmptyStrings))
                    doUpdate = false;
                if (name == nameof(IDtoEntity.Id) && updateFlag.HasFlag(DtoAggregateMethodFlag.ExceptId))
                    doUpdate = false;

                if (doUpdate)
                    property[name].SetMethod.Invoke(instance, new[] { value });
            }
            return instance;
        }

        public T Aggregate(T intoInstance, T fromInstance, DtoAggregateMethodFlag updateFlag = DtoAggregateMethodFlag.Everything)
        {
            return Aggregate(intoInstance, Decompose(fromInstance), updateFlag);
        }

        public IEnumerable<T> Aggregate(IEnumerable<T> intoInstances, IEnumerable<T> fromInstances, DtoAggregateMethodFlag updateFlag = DtoAggregateMethodFlag.Everything)
        {
            var byId = fromInstances.ToDictionary(i => i.Id);
            return intoInstances.Select(i =>
            {
                T from;
                if (byId.TryGetValue(i.Id, out from))
                    return Aggregate(i, from, updateFlag);
                else
                    return i;
            });
        }

        public Dictionary<string, object[]> Decompose(IEnumerable<T> instances, params string[] propertyNameFilter)
        {
            var result = new Dictionary<string, List<object>>();
            var filter = new HashSet<string>(propertyNameFilter);
            foreach (var instance in instances)
            {
                foreach (var (name, value) in Decompose(instance).Where(t => filter.Count == 0 || filter.Contains(t.Item1)))
                {
                    List<object> values;
                    if (!result.TryGetValue(name, out values))
                        result.Add(name, values = new List<object>());

                    values.Add(value);
                }
            }
            return result.ToDictionary(g => g.Key, g => g.Value.ToArray());
        }

        public IEnumerator<PropertyInfo> GetEnumerator()
        {
            return property.Select(g => g.Value).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
