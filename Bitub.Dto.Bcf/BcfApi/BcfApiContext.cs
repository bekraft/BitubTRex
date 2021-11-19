using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Bitub.Dto.Rest;

namespace Bitub.Dto.BcfApi
{
    public class BcfApiContext : ApiContext
    {
        public BcfApiContext(string clientId) : base(clientId)
        {

        }        

        public override Task<DtoResult<IDtoAuthenticated>> Authenticate()
        {
            throw new NotImplementedException();
        }
    }
}
