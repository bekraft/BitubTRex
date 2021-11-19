using System;
namespace Bitub.Dto.Rest
{
    public interface IDtoEntity
    {
        public object Id { get; }
    }

    public interface IDtoEntity<T> : IDtoEntity
    {
        public new T Id { get; }
    }

}
