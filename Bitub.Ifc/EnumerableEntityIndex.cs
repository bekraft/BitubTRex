using System;
using System.Collections.Generic;
using System.Linq;

namespace Bitub.Ifc
{
    public class EntityIndex<T>
    {
        private LazyUnfold unfoldProvider = (t => Enumerable.Empty<EntityIndex<T>>());

        public delegate IEnumerable<EntityIndex<T>> LazyUnfold(EntityIndex<T> i);

        public EntityIndex<T> Parent { private set; get; }

        public T Entity { private set;  get; }

        public IEnumerable<EntityIndex<T>> Children 
        {
            get => unfoldProvider(this);            
        }

        public int CountChildren
        {
            get => Children.Count();            
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
            unfoldProvider = provider;
        }
    }
}
