using System;

namespace Bitub.Dto.Rest
{
    public interface IDtoAuthenticated
    {
        string Schema { get; }
        string Token { get; }
        TimeSpan ExpiresIn { get; }
        string RefreshToken { get; }
    }
}
