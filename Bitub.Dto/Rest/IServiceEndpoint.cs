using System;

namespace Bitub.Dto.Rest
{
    public interface IServiceEndpoint
    {
        string ResourceUri { get; }
        bool IsRooted { get; }
    }
}
