using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Bitub.Dto.Rest
{
    abstract public class DtoContext
    {
        #region Internals
        private AuthenticationHeaderValue authHeaderValue;
        internal readonly object contextMonitor = new object();
        #endregion

        public readonly string clientId;

        protected DtoContext(string clientId)
        {
            this.clientId = clientId;
        }

        public AuthenticationHeaderValue AuthSchemeAndToken
        {
            get { lock (contextMonitor) return authHeaderValue; }
            set { lock (contextMonitor) authHeaderValue = value; }
        }

        abstract public Task<DtoResult<IDtoAuthenticated>> Authenticate();
        
        public string BaseURL { get; set; }

        public JsonSerializerOptions JsonOptions { get; set; } = new JsonSerializerOptions();

        public virtual T FromJson<T>(string json)
        {
            return JsonSerializer.Deserialize<T>(json, JsonOptions);
        }

        public virtual string ToJson<T>(T dto)
        {
            return JsonSerializer.Serialize(dto, JsonOptions);
        }
    }
}
