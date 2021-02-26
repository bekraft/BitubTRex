using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bitub.Dto.Rest
{
    abstract public class DtoServiceEndpoint<T>
    {
        #region Internals
        private readonly DtoApiClient apiClient;
        #endregion

        protected DtoServiceEndpoint(DtoApiClient client)
        {
            apiClient = client;
        }

        public string ResourceURI { get; protected set; }

        public Task<DtoResult<T>> GetAsync()
        {
            throw new NotImplementedException();
        }
    }
}
