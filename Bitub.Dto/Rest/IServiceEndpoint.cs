using System;
using System.Dynamic;
using System.Net.Http.Headers;

namespace Bitub.Dto.Rest
{
    public interface IServiceEndpoint
    {
        string ResourceURI { get; }
        bool IsRooted { get; }
        MediaTypeWithQualityHeaderValue ContentHeader { get; }
    }
}
