using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xbim.Common;

namespace Bitub.Ifc
{
    public interface IEnumerableEntityIndex<T> where T : ILabeled
    {
        IEnumerable<EntityIndex<T>> FlattenEntityIndex();
    }

    public class EntityIndex<T> where T : ILabeled
    {
        private LazyUnfold m_unfoldProvider = (t => Enumerable.Empty<EntityIndex<T>>());

        public delegate IEnumerable<EntityIndex<T>> LazyUnfold(EntityIndex<T> i);

        public EntityIndex<T> Parent { private set; get; }

        public T Entity { private set;  get; }

        public IEnumerable<EntityIndex<T>> Children {
            get {
                return m_unfoldProvider(this);
            }
        }

        public int CountChildren
        {
            get {
                return Children.Count();
            }
        }

        public bool IsRoot => null == Parent;

        public bool IsLeaf => 0 == CountChildren;

        public EntityIndex(EntityIndex<T> parent, T entity)
        {
            Parent = parent;
            Entity = entity;
        }

        public EntityIndex(EntityIndex<T> parent, T entity, LazyUnfold provider) : this(parent, entity)
        {
            m_unfoldProvider = provider;
        }
    }
}
